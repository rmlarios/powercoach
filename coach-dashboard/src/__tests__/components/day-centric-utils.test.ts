import {
  transformToDaySlots,
  getNotationTooltip,
} from '@/components/builder/day-centric-utils';
import { BuilderWeek, BuilderDay, BuilderExercise } from '@/types/builder';

// ========================
// Mock Helpers
// ========================
function mockExercise(overrides: Partial<BuilderExercise> = {}): BuilderExercise {
  return {
    id: 'ex-1',
    exerciseId: 'squat-1',
    exerciseName: 'Back Squat',
    order: 1,
    sets: 3,
    repsMin: 5,
    repsMax: 5,
    restSeconds: 180,
    ...overrides,
  };
}

function mockDay(overrides: Partial<BuilderDay> = {}): BuilderDay {
  return {
    id: `day-${overrides.dayNumber ?? 1}`,
    dayNumber: 1,
    name: 'Day 1',
    focus: 'FullBody',
    exercises: [],
    ...overrides,
  };
}

function mockWeek(overrides: Partial<BuilderWeek> = {}): BuilderWeek {
  return {
    id: `week-${overrides.weekNumber ?? 1}`,
    weekNumber: 1,
    name: 'Week 1',
    days: [],
    ...overrides,
  };
}

// ========================
// transformToDaySlots
// ========================
describe('transformToDaySlots', () => {
  it('should return empty array for empty weeks', () => {
    expect(transformToDaySlots([])).toEqual([]);
  });

  it('should create one DaySlot per unique dayNumber', () => {
    const weeks: BuilderWeek[] = [
      mockWeek({
        weekNumber: 1,
        days: [
          mockDay({ dayNumber: 1, name: 'Push' }),
          mockDay({ dayNumber: 2, name: 'Pull' }),
        ],
      }),
    ];
    const result = transformToDaySlots(weeks);
    expect(result).toHaveLength(2);
    expect(result[0].dayNumber).toBe(1);
    expect(result[0].name).toBe('Push');
    expect(result[1].dayNumber).toBe(2);
    expect(result[1].name).toBe('Pull');
  });

  it('should group same exercise across multiple weeks into weekPrescriptions', () => {
    const weeks: BuilderWeek[] = [
      mockWeek({
        weekNumber: 1,
        days: [
          mockDay({
            dayNumber: 1,
            exercises: [
              mockExercise({ exerciseName: 'Squat', sets: 3, repsMin: 8, repsMax: 8 }),
            ],
          }),
        ],
      }),
      mockWeek({
        weekNumber: 2,
        id: 'week-2',
        days: [
          mockDay({
            id: 'w2d1',
            dayNumber: 1,
            exercises: [
              mockExercise({ id: 'ex-2', exerciseName: 'Squat', sets: 4, repsMin: 6, repsMax: 6 }),
            ],
          }),
        ],
      }),
    ];

    const result = transformToDaySlots(weeks);
    expect(result).toHaveLength(1); // Only 1 dayNumber
    expect(result[0].exercises).toHaveLength(1); // Same exercise grouped
    
    const exercise = result[0].exercises[0];
    expect(exercise.exerciseName).toBe('Squat');
    expect(exercise.weekPrescriptions.size).toBe(2);
    
    // Week 1 prescription
    const w1 = exercise.weekPrescriptions.get(1);
    expect(w1).toBeDefined();
    expect(w1!.sets).toBe(3);
    expect(w1!.reps).toBe('3x8');
    
    // Week 2 prescription
    const w2 = exercise.weekPrescriptions.get(2);
    expect(w2).toBeDefined();
    expect(w2!.sets).toBe(4);
    expect(w2!.reps).toBe('4x6');
  });

  it('should preserve rawNotation for compound sets', () => {
    const weeks: BuilderWeek[] = [
      mockWeek({
        weekNumber: 1,
        days: [
          mockDay({
            dayNumber: 1,
            exercises: [
              mockExercise({
                exerciseName: 'Bench',
                sets: 1,
                repsMin: 1,
                repsMax: 1,
                rawNotation: '1x1 3x4',
              }),
            ],
          }),
        ],
      }),
    ];

    const result = transformToDaySlots(weeks);
    const prescription = result[0].exercises[0].weekPrescriptions.get(1);
    expect(prescription!.reps).toBe('1x1 3x4');
  });

  it('should generate reps display from sets/repsMin/repsMax when no rawNotation', () => {
    const weeks: BuilderWeek[] = [
      mockWeek({
        weekNumber: 1,
        days: [
          mockDay({
            dayNumber: 1,
            exercises: [
              mockExercise({ sets: 4, repsMin: 8, repsMax: 12 }),
            ],
          }),
        ],
      }),
    ];

    const result = transformToDaySlots(weeks);
    const prescription = result[0].exercises[0].weekPrescriptions.get(1);
    expect(prescription!.reps).toBe('4x8-12');
  });

  it('should sort daySlots by dayNumber', () => {
    const weeks: BuilderWeek[] = [
      mockWeek({
        weekNumber: 1,
        days: [
          mockDay({ dayNumber: 3, name: 'Legs' }),
          mockDay({ dayNumber: 1, name: 'Push' }),
          mockDay({ dayNumber: 2, name: 'Pull' }),
        ],
      }),
    ];

    const result = transformToDaySlots(weeks);
    expect(result.map(d => d.dayNumber)).toEqual([1, 2, 3]);
  });

  it('should sort exercises by order within each daySlot', () => {
    const weeks: BuilderWeek[] = [
      mockWeek({
        weekNumber: 1,
        days: [
          mockDay({
            dayNumber: 1,
            exercises: [
              mockExercise({ exerciseId: 'b', exerciseName: 'Bench', order: 2 }),
              mockExercise({ exerciseId: 'a', exerciseName: 'Squat', order: 1 }),
              mockExercise({ exerciseId: 'c', exerciseName: 'OHP', order: 3 }),
            ],
          }),
        ],
      }),
    ];

    const result = transformToDaySlots(weeks);
    expect(result[0].exercises.map(e => e.exerciseName)).toEqual(['Squat', 'Bench', 'OHP']);
  });

  it('should use day.name for the DaySlot name, fallback to "Day N"', () => {
    const weeks: BuilderWeek[] = [
      mockWeek({
        weekNumber: 1,
        days: [
          mockDay({ dayNumber: 1, name: '' }),
          mockDay({ dayNumber: 2, name: 'Upper Body' }),
        ],
      }),
    ];

    const result = transformToDaySlots(weeks);
    expect(result[0].name).toBe('Day 1');
    expect(result[1].name).toBe('Upper Body');
  });

  it('should preserve exercise type metadata (emom, tempo, superset)', () => {
    const weeks: BuilderWeek[] = [
      mockWeek({
        weekNumber: 1,
        days: [
          mockDay({
            dayNumber: 1,
            exercises: [
              mockExercise({
                exerciseName: 'EMOM Squats',
                exerciseType: 'emom',
                emomConfig: { totalMinutes: 10, workSeconds: 40, restSeconds: 20 },
              }),
            ],
          }),
        ],
      }),
    ];

    const result = transformToDaySlots(weeks);
    const ex = result[0].exercises[0];
    expect(ex.exerciseType).toBe('emom');
    expect(ex.emomConfig).toEqual({ totalMinutes: 10, workSeconds: 40, restSeconds: 20 });
  });

  it('should include rpeTarget, percentageRM, weight, notes in prescriptions', () => {
    const weeks: BuilderWeek[] = [
      mockWeek({
        weekNumber: 1,
        days: [
          mockDay({
            dayNumber: 1,
            exercises: [
              mockExercise({
                sets: 3,
                repsMin: 5,
                repsMax: 5,
                rpeTarget: 8,
                percentageRM: 75,
                weight: 100,
                notes: 'Pause at bottom',
              }),
            ],
          }),
        ],
      }),
    ];

    const result = transformToDaySlots(weeks);
    const p = result[0].exercises[0].weekPrescriptions.get(1)!;
    expect(p.rpeTarget).toBe(8);
    expect(p.percentageRM).toBe(75);
    expect(p.weight).toBe(100);
    expect(p.notes).toBe('Pause at bottom');
  });

  it('should handle different exercises across weeks in the same day', () => {
    const weeks: BuilderWeek[] = [
      mockWeek({
        weekNumber: 1,
        days: [
          mockDay({
            dayNumber: 1,
            exercises: [
              mockExercise({ exerciseId: 'squat', exerciseName: 'Squat', order: 1 }),
            ],
          }),
        ],
      }),
      mockWeek({
        weekNumber: 2,
        id: 'week-2',
        days: [
          mockDay({
            id: 'w2d1',
            dayNumber: 1,
            exercises: [
              mockExercise({ id: 'ex-2', exerciseId: 'squat', exerciseName: 'Squat', order: 1 }),
              mockExercise({ id: 'ex-3', exerciseId: 'bench', exerciseName: 'Bench', order: 2 }),
            ],
          }),
        ],
      }),
    ];

    const result = transformToDaySlots(weeks);
    expect(result[0].exercises).toHaveLength(2);
    
    // Squat appears in both weeks
    const squat = result[0].exercises.find(e => e.exerciseName === 'Squat')!;
    expect(squat.weekPrescriptions.size).toBe(2);
    
    // Bench only in week 2
    const bench = result[0].exercises.find(e => e.exerciseName === 'Bench')!;
    expect(bench.weekPrescriptions.size).toBe(1);
    expect(bench.weekPrescriptions.has(2)).toBe(true);
  });
});

// ========================
// getNotationTooltip
// ========================
describe('getNotationTooltip', () => {
  it('should describe a simple notation', () => {
    const result = getNotationTooltip('3x8');
    expect(result).toContain('3x8');
  });

  it('should describe a range notation', () => {
    const result = getNotationTooltip('4x8-12');
    expect(result).toContain('4x8-12');
  });

  it('should describe compound notation with "+"', () => {
    const result = getNotationTooltip('1x1 3x4');
    expect(result).toContain('Single');
    expect(result).toContain('+');
    expect(result).toContain('3x4');
  });

  it('should include RPE when present', () => {
    const result = getNotationTooltip('3x5 @8');
    expect(result).toContain('@8');
  });

  it('should include percentage when present', () => {
    const result = getNotationTooltip('3x5 75%');
    expect(result).toContain('75%');
  });

  it('should show AMRAP', () => {
    const result = getNotationTooltip('AMRAP');
    expect(result).toContain('AMRAP');
  });

  it('should show invalid notation message for bad input', () => {
    const result = getNotationTooltip('not_valid!!!');
    expect(result).toContain('no válida');
  });

  it('should include tempo info', () => {
    const result = getNotationTooltip('3x8 TEMPO 3:1:0');
    expect(result).toContain('Tempo');
    expect(result).toContain('3:1:0');
  });
});
