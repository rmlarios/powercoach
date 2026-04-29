/**
 * Tests for PDF generation utilities (pdf-utils.ts).
 * PB-006-09: Tests de generación PDF
 */
import {
  transformToAllDays,
  getExercise1RM,
  formatWeight,
  calculateWeightForPrescription,
  getDaySummary,
} from '@/components/pdf/pdf-utils';
import type { BuilderWeek } from '@/types/builder';
import type { MaxLift } from '@/types/athlete';

// ============================================
// Helpers
// ============================================
// eslint-disable-next-line @typescript-eslint/no-explicit-any
function mockExercise(overrides: Record<string, any> = {}) {
  return {
    id: 'ex-entry-1',
    exerciseId: 'bench-press',
    exerciseName: 'Bench Press',
    order: 1,
    sets: 3,
    repsMin: 8,
    repsMax: 8,
    restSeconds: 120,
    ...overrides,
  };
}

// eslint-disable-next-line @typescript-eslint/no-explicit-any
function mockDay(overrides: Record<string, any> = {}) {
  return {
    id: 'day-1',
    dayNumber: 1,
    name: 'Push',
    focus: 'Push' as const,
    exercises: [],
    ...overrides,
  };
}

// eslint-disable-next-line @typescript-eslint/no-explicit-any
function mockWeek(overrides: Record<string, any> = {}): BuilderWeek {
  return {
    id: 'week-1',
    weekNumber: 1,
    name: 'Week 1',
    days: [],
    ...overrides,
  } as BuilderWeek;
}

function mockMaxLift(overrides: Partial<MaxLift> = {}): MaxLift {
  return {
    id: 'ml-1',
    exerciseId: 'bench-press',
    exerciseName: 'Bench Press',
    weight: 100,
    isTested: true,
    recordedAt: '2026-01-01',
    ...overrides,
  };
}

// ============================================
// transformToAllDays
// ============================================
describe('transformToAllDays (PDF)', () => {
  it('should return empty map for empty weeks', () => {
    const result = transformToAllDays([]);
    expect(result.size).toBe(0);
  });

  it('should group exercises by dayNumber across weeks', () => {
    const weeks: BuilderWeek[] = [
      mockWeek({
        weekNumber: 1,
        days: [
          mockDay({
            dayNumber: 1,
            exercises: [mockExercise({ sets: 3, repsMin: 8, repsMax: 8 })],
          }),
        ],
      }),
      mockWeek({
        id: 'week-2',
        weekNumber: 2,
        days: [
          mockDay({
            id: 'day-1-w2',
            dayNumber: 1,
            exercises: [mockExercise({ id: 'ex-entry-2', sets: 3, repsMin: 6, repsMax: 6 })],
          }),
        ],
      }),
    ];

    const result = transformToAllDays(weeks);
    expect(result.size).toBe(1);
    
    const day1 = result.get(1)!;
    expect(day1.name).toBe('Push');
    expect(day1.exercises).toHaveLength(1);
    expect(day1.exercises[0].exerciseName).toBe('Bench Press');
    expect(day1.exercises[0].weekPrescriptions.size).toBe(2);
    expect(day1.exercises[0].weekPrescriptions.get(1)!.reps).toBe('3x8');
    expect(day1.exercises[0].weekPrescriptions.get(2)!.reps).toBe('3x6');
  });

  it('should use rawNotation when available', () => {
    const weeks: BuilderWeek[] = [
      mockWeek({
        days: [
          mockDay({
            exercises: [
              mockExercise({ rawNotation: '1x1 3x4', sets: 1, repsMin: 1 }),
            ],
          }),
        ],
      }),
    ];

    const result = transformToAllDays(weeks);
    const prescription = result.get(1)!.exercises[0].weekPrescriptions.get(1)!;
    expect(prescription.reps).toBe('1x1 3x4');
  });

  it('should generate reps display with range when repsMax != repsMin', () => {
    const weeks: BuilderWeek[] = [
      mockWeek({
        days: [
          mockDay({
            exercises: [mockExercise({ sets: 3, repsMin: 8, repsMax: 12 })],
          }),
        ],
      }),
    ];

    const result = transformToAllDays(weeks);
    const prescription = result.get(1)!.exercises[0].weekPrescriptions.get(1)!;
    expect(prescription.reps).toBe('3x8-12');
  });

  it('should preserve exerciseType', () => {
    const weeks: BuilderWeek[] = [
      mockWeek({
        days: [
          mockDay({
            exercises: [mockExercise({ exerciseType: 'tempo' })],
          }),
        ],
      }),
    ];

    const result = transformToAllDays(weeks);
    expect(result.get(1)!.exercises[0].exerciseType).toBe('tempo');
  });

  it('should preserve percentageRM, rpeTarget, weight', () => {
    const weeks: BuilderWeek[] = [
      mockWeek({
        days: [
          mockDay({
            exercises: [
              mockExercise({ percentageRM: 75, rpeTarget: 8, weight: 80 }),
            ],
          }),
        ],
      }),
    ];

    const result = transformToAllDays(weeks);
    const p = result.get(1)!.exercises[0].weekPrescriptions.get(1)!;
    expect(p.percentageRM).toBe(75);
    expect(p.rpeTarget).toBe(8);
    expect(p.weight).toBe(80);
  });

  it('should handle multiple days', () => {
    const weeks: BuilderWeek[] = [
      mockWeek({
        days: [
          mockDay({
            dayNumber: 1,
            name: 'Push',
            exercises: [mockExercise()],
          }),
          mockDay({
            id: 'day-2',
            dayNumber: 2,
            name: 'Pull',
            exercises: [
              mockExercise({
                id: 'ex-2',
                exerciseId: 'deadlift',
                exerciseName: 'Deadlift',
              }),
            ],
          }),
        ],
      }),
    ];

    const result = transformToAllDays(weeks);
    expect(result.size).toBe(2);
    expect(result.get(1)!.name).toBe('Push');
    expect(result.get(2)!.name).toBe('Pull');
    expect(result.get(2)!.exercises[0].exerciseName).toBe('Deadlift');
  });

  it('should use fallback name "Day N" when name is empty', () => {
    const weeks: BuilderWeek[] = [
      mockWeek({
        days: [
          mockDay({ dayNumber: 3, name: '', exercises: [] }),
        ],
      }),
    ];

    const result = transformToAllDays(weeks);
    expect(result.get(3)!.name).toBe('Day 3');
  });

  it('should handle multiple exercises in same day', () => {
    const weeks: BuilderWeek[] = [
      mockWeek({
        days: [
          mockDay({
            exercises: [
              mockExercise({ exerciseId: 'bench', exerciseName: 'Bench Press', order: 1 }),
              mockExercise({ id: 'ex-2', exerciseId: 'ohp', exerciseName: 'OHP', order: 2 }),
            ],
          }),
        ],
      }),
    ];

    const result = transformToAllDays(weeks);
    expect(result.get(1)!.exercises).toHaveLength(2);
    expect(result.get(1)!.exercises[0].exerciseName).toBe('Bench Press');
    expect(result.get(1)!.exercises[1].exerciseName).toBe('OHP');
  });
});

// ============================================
// getExercise1RM
// ============================================
describe('getExercise1RM', () => {
  it('should return null when no maxLifts provided', () => {
    expect(getExercise1RM('bench-press')).toBeNull();
    expect(getExercise1RM('bench-press', undefined)).toBeNull();
  });

  it('should return null when exercise not found', () => {
    const lifts = [mockMaxLift({ exerciseId: 'squat', weight: 200 })];
    expect(getExercise1RM('bench-press', lifts)).toBeNull();
  });

  it('should return the weight when found', () => {
    const lifts = [mockMaxLift({ exerciseId: 'bench-press', weight: 100 })];
    expect(getExercise1RM('bench-press', lifts)).toBe(100);
  });

  it('should return correct weight from multiple lifts', () => {
    const lifts = [
      mockMaxLift({ id: '1', exerciseId: 'squat', weight: 200 }),
      mockMaxLift({ id: '2', exerciseId: 'bench-press', weight: 100 }),
      mockMaxLift({ id: '3', exerciseId: 'deadlift', weight: 250 }),
    ];
    expect(getExercise1RM('bench-press', lifts)).toBe(100);
    expect(getExercise1RM('deadlift', lifts)).toBe(250);
  });
});

// ============================================
// formatWeight
// ============================================
describe('formatWeight', () => {
  it('should format integer weights', () => {
    expect(formatWeight(100)).toBe('100kg');
  });

  it('should format decimal weights rounded to 1 decimal', () => {
    expect(formatWeight(82.5)).toBe('82.5kg');
  });

  it('should round to 1 decimal place', () => {
    expect(formatWeight(82.567)).toBe('82.6kg');
  });
});

// ============================================
// calculateWeightForPrescription
// ============================================
describe('calculateWeightForPrescription', () => {
  it('should return null when no percentage', () => {
    expect(calculateWeightForPrescription(undefined, 100)).toBeNull();
  });

  it('should return null when no 1RM', () => {
    expect(calculateWeightForPrescription(75, null)).toBeNull();
  });

  it('should calculate weight correctly', () => {
    // 100kg * 75% = 75kg, rounded to nearest 2.5 = 75kg
    expect(calculateWeightForPrescription(75, 100, 2.5)).toBe(75);
  });

  it('should round to specified increment', () => {
    // 100kg * 73% = 73kg, rounded to nearest 2.5 = 72.5kg
    expect(calculateWeightForPrescription(73, 100, 2.5)).toBe(72.5);
  });

  it('should round to 5kg when specified', () => {
    // 100kg * 73% = 73kg, rounded to nearest 5 = 75kg
    expect(calculateWeightForPrescription(73, 100, 5)).toBe(75);
  });
});

// ============================================
// getDaySummary
// ============================================
describe('getDaySummary', () => {
  it('should return singular for 1 exercise', () => {
    expect(getDaySummary({ name: 'Push', exercises: [{ exerciseId: 'a', exerciseName: 'A', weekPrescriptions: new Map() }] })).toBe('1 exercise');
  });

  it('should return plural for multiple exercises', () => {
    expect(getDaySummary({
      name: 'Push',
      exercises: [
        { exerciseId: 'a', exerciseName: 'A', weekPrescriptions: new Map() },
        { exerciseId: 'b', exerciseName: 'B', weekPrescriptions: new Map() },
      ],
    })).toBe('2 exercises');
  });

  it('should return plural for 0 exercises', () => {
    expect(getDaySummary({ name: 'Rest', exercises: [] })).toBe('0 exercises');
  });
});
