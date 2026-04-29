/**
 * Tests for Excel generation utilities (excel-utils.ts).
 * PB-007-08: Tests de estructura Excel
 */
import {
  transformToAllDays,
  getExercise1RM,
  formatExerciseNameForExport,
  createOverviewData,
  createDaySheetData,
  DayDataForExcel,
} from '@/components/excel/excel-utils';
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
describe('transformToAllDays (Excel)', () => {
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
            exercises: [mockExercise({ sets: 4, repsMin: 6, repsMax: 6 })],
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
            exercises: [mockExercise({ id: 'ex-2', sets: 4, repsMin: 5, repsMax: 5 })],
          }),
        ],
      }),
    ];

    const result = transformToAllDays(weeks);
    expect(result.size).toBe(1);

    const day1 = result.get(1)!;
    expect(day1.exercises).toHaveLength(1);
    expect(day1.exercises[0].weekPrescriptions.size).toBe(2);
    expect(day1.exercises[0].weekPrescriptions.get(1)!.reps).toBe('4x6');
    expect(day1.exercises[0].weekPrescriptions.get(2)!.reps).toBe('4x5');
  });

  it('should use rawNotation when present', () => {
    const weeks: BuilderWeek[] = [
      mockWeek({
        days: [
          mockDay({
            exercises: [mockExercise({ rawNotation: '1x3 2x5' })],
          }),
        ],
      }),
    ];

    const result = transformToAllDays(weeks);
    const prescription = result.get(1)!.exercises[0].weekPrescriptions.get(1)!;
    expect(prescription.reps).toBe('1x3 2x5');
  });

  it('should handle rep ranges', () => {
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

  it('should store exerciseType', () => {
    const weeks: BuilderWeek[] = [
      mockWeek({
        days: [
          mockDay({
            exercises: [mockExercise({ exerciseType: 'emom' })],
          }),
        ],
      }),
    ];

    const result = transformToAllDays(weeks);
    expect(result.get(1)!.exercises[0].exerciseType).toBe('emom');
  });

  it('should use fallback name when day name is empty', () => {
    const weeks: BuilderWeek[] = [
      mockWeek({
        days: [mockDay({ dayNumber: 5, name: '' })],
      }),
    ];

    const result = transformToAllDays(weeks);
    expect(result.get(5)!.name).toBe('Day 5');
  });
});

// ============================================
// getExercise1RM
// ============================================
describe('getExercise1RM (Excel)', () => {
  it('should return null when no lifts', () => {
    expect(getExercise1RM('bench-press')).toBeNull();
    expect(getExercise1RM('bench-press', undefined)).toBeNull();
  });

  it('should return null when exercise not found', () => {
    const lifts = [mockMaxLift({ exerciseId: 'squat' })];
    expect(getExercise1RM('bench-press', lifts)).toBeNull();
  });

  it('should return correct weight', () => {
    const lifts = [
      mockMaxLift({ exerciseId: 'bench-press', weight: 100 }),
      mockMaxLift({ id: '2', exerciseId: 'squat', weight: 180 }),
    ];
    expect(getExercise1RM('bench-press', lifts)).toBe(100);
    expect(getExercise1RM('squat', lifts)).toBe(180);
  });
});

// ============================================
// formatExerciseNameForExport
// ============================================
describe('formatExerciseNameForExport', () => {
  it('should return plain name for standard type', () => {
    expect(formatExerciseNameForExport('Bench Press', 'standard')).toBe('Bench Press');
  });

  it('should return plain name when no type', () => {
    expect(formatExerciseNameForExport('Bench Press')).toBe('Bench Press');
    expect(formatExerciseNameForExport('Bench Press', undefined)).toBe('Bench Press');
  });

  it('should add uppercase type suffix for tempo', () => {
    expect(formatExerciseNameForExport('Squat', 'tempo')).toBe('Squat [TEMPO]');
  });

  it('should add uppercase type suffix for emom', () => {
    expect(formatExerciseNameForExport('Deadlift', 'emom')).toBe('Deadlift [EMOM]');
  });

  it('should add uppercase type suffix for superset', () => {
    expect(formatExerciseNameForExport('Curl', 'superset')).toBe('Curl [SUPERSET]');
  });
});

// ============================================
// createOverviewData
// ============================================
describe('createOverviewData', () => {
  it('should include program name and duration', () => {
    const rows = createOverviewData({
      programName: 'Hypertrophy 12',
      durationWeeks: 12,
    });

    const headerRow = rows[0];
    expect(headerRow[0]).toBe('PROGRAM OVERVIEW');

    const nameRow = rows.find((r) => r[0] === 'Program Name');
    expect(nameRow).toBeDefined();
    expect(nameRow![1]).toBe('Hypertrophy 12');

    const durationRow = rows.find((r) => r[0] === 'Duration');
    expect(durationRow).toBeDefined();
    expect(durationRow![1]).toBe('12 weeks');
  });

  it('should include optional fields when provided', () => {
    const rows = createOverviewData({
      programName: 'Test',
      description: 'A test program',
      athleteName: 'John',
      coachName: 'Coach Smith',
      durationWeeks: 4,
      startDate: '2026-03-01',
    });

    expect(rows.find((r) => r[0] === 'Description')![1]).toBe('A test program');
    expect(rows.find((r) => r[0] === 'Athlete')![1]).toBe('John');
    expect(rows.find((r) => r[0] === 'Coach')![1]).toBe('Coach Smith');
    expect(rows.find((r) => r[0] === 'Start Date')![1]).toBe('2026-03-01');
  });

  it('should omit optional fields when not provided', () => {
    const rows = createOverviewData({
      programName: 'Test',
      durationWeeks: 4,
    });

    expect(rows.find((r) => r[0] === 'Description')).toBeUndefined();
    expect(rows.find((r) => r[0] === 'Athlete')).toBeUndefined();
    expect(rows.find((r) => r[0] === 'Coach')).toBeUndefined();
    expect(rows.find((r) => r[0] === 'Start Date')).toBeUndefined();
  });

  it('should include max lifts section when provided', () => {
    const rows = createOverviewData({
      programName: 'Test',
      durationWeeks: 4,
      athleteMaxLifts: [
        mockMaxLift({ exerciseName: 'Bench Press', weight: 100 }),
        mockMaxLift({ id: '2', exerciseId: 'squat', exerciseName: 'Squat', weight: 180 }),
      ],
    });

    const maxLiftsHeader = rows.find((r) => r[0] === 'ATHLETE MAX LIFTS');
    expect(maxLiftsHeader).toBeDefined();

    const benchRow = rows.find((r) => r[0] === 'Bench Press' && r[1] === 100);
    expect(benchRow).toBeDefined();

    const squatRow = rows.find((r) => r[0] === 'Squat' && r[1] === 180);
    expect(squatRow).toBeDefined();
  });

  it('should not include max lifts section when empty array', () => {
    const rows = createOverviewData({
      programName: 'Test',
      durationWeeks: 4,
      athleteMaxLifts: [],
    });

    expect(rows.find((r) => r[0] === 'ATHLETE MAX LIFTS')).toBeUndefined();
  });

  it('should include Generated row', () => {
    const rows = createOverviewData({
      programName: 'Test',
      durationWeeks: 4,
    });

    const generatedRow = rows.find((r) => r[0] === 'Generated');
    expect(generatedRow).toBeDefined();
    expect(typeof generatedRow![1]).toBe('string');
  });
});

// ============================================
// createDaySheetData
// ============================================
describe('createDaySheetData', () => {
  const weekNumbers = [1, 2, 3];

  function makeDayData(exercises: DayDataForExcel['exercises'] = []): DayDataForExcel {
    return { name: 'Push', exercises };
  }

  function makeExercise(overrides: Partial<DayDataForExcel['exercises'][0]> = {}): DayDataForExcel['exercises'][0] {
    return {
      exerciseId: 'bench-press',
      exerciseName: 'Bench Press',
      weekPrescriptions: new Map([
        [1, { reps: '3x8', percentageRM: 70 }],
        [2, { reps: '3x6', percentageRM: 75 }],
        [3, { reps: '3x4', percentageRM: 80 }],
      ]),
      ...overrides,
    };
  }

  it('should have header row with week columns', () => {
    const rows = createDaySheetData(makeDayData(), weekNumbers);
    expect(rows[0]).toEqual(['Exercise', 'S1', 'S2', 'S3']);
  });

  it('should include exercise prescription rows', () => {
    const dayData = makeDayData([makeExercise()]);
    const rows = createDaySheetData(dayData, weekNumbers, undefined, false);

    // Row 0 = header, row 1 = exercise, row 2 = spacer
    expect(rows[1][0]).toBe('Bench Press');
    expect(rows[1][1]).toBe('3x8 70%');
    expect(rows[1][2]).toBe('3x6 75%');
    expect(rows[1][3]).toBe('3x4 80%');
  });

  it('should include RPE in prescription', () => {
    const dayData = makeDayData([
      makeExercise({
        weekPrescriptions: new Map([
          [1, { reps: '3x5', rpeTarget: 8 }],
        ]),
      }),
    ]);
    const rows = createDaySheetData(dayData, [1], undefined, false);
    expect(rows[1][1]).toBe('3x5 @8');
  });

  it('should include percentage AND RPE together', () => {
    const dayData = makeDayData([
      makeExercise({
        weekPrescriptions: new Map([
          [1, { reps: '3x5', percentageRM: 80, rpeTarget: 8 }],
        ]),
      }),
    ]);
    const rows = createDaySheetData(dayData, [1], undefined, false);
    expect(rows[1][1]).toBe('3x5 80% @8');
  });

  it('should add weight row when includeWeights is true and 1RM available', () => {
    const lifts = [mockMaxLift({ exerciseId: 'bench-press', weight: 100 })];
    const dayData = makeDayData([makeExercise()]);
    const rows = createDaySheetData(dayData, weekNumbers, lifts, true, 2.5);

    // Row 0: header
    // Row 1: exercise prescription
    // Row 2: weight suggestion
    // Row 3: spacer
    expect(rows[2][0]).toBe('');
    // 100 * 70% = 70kg
    expect(rows[2][1]).toBe('70kg');
    // 100 * 75% = 75kg
    expect(rows[2][2]).toBe('75kg');
    // 100 * 80% = 80kg
    expect(rows[2][3]).toBe('80kg');
  });

  it('should not add weight row when includeWeights is false', () => {
    const lifts = [mockMaxLift({ exerciseId: 'bench-press', weight: 100 })];
    const dayData = makeDayData([makeExercise()]);
    const rows = createDaySheetData(dayData, weekNumbers, lifts, false);

    // 1:header + 1:exercise + 1:spacer = 3 rows
    expect(rows).toHaveLength(3);
    // Row 2 should be spacer (all empty)
    expect(rows[2].every((c) => c === '')).toBe(true);
  });

  it('should not add weight row when 1RM not available', () => {
    const dayData = makeDayData([makeExercise()]);
    const rows = createDaySheetData(dayData, weekNumbers, undefined, true);

    // No 1RM → no weight row: 1:header + 1:exercise + 1:spacer = 3
    expect(rows).toHaveLength(3);
  });

  it('should show empty cell when week has no prescription', () => {
    const dayData = makeDayData([
      makeExercise({
        weekPrescriptions: new Map([
          [1, { reps: '3x8' }],
          // weeks 2 and 3 missing
        ]),
      }),
    ]);
    const rows = createDaySheetData(dayData, weekNumbers, undefined, false);

    expect(rows[1][1]).toBe('3x8');
    expect(rows[1][2]).toBe('');
    expect(rows[1][3]).toBe('');
  });

  it('should use formatExerciseNameForExport for display name', () => {
    const dayData = makeDayData([
      makeExercise({ exerciseType: 'tempo' }),
    ]);
    const rows = createDaySheetData(dayData, [1], undefined, false);
    expect(rows[1][0]).toBe('Bench Press [TEMPO]');
  });

  it('should handle explicit weight (no %RM) in weight row', () => {
    const lifts = [mockMaxLift({ exerciseId: 'bench-press', weight: 100 })];
    const dayData = makeDayData([
      makeExercise({
        weekPrescriptions: new Map([
          [1, { reps: '3x8', weight: 60 }],
        ]),
      }),
    ]);
    const rows = createDaySheetData(dayData, [1], lifts, true, 2.5);
    // No %RM but has explicit weight → should show "60kg"
    expect(rows[2][1]).toBe('60kg');
  });

  it('should round weights to specified increment', () => {
    const lifts = [mockMaxLift({ exerciseId: 'bench-press', weight: 100 })];
    const dayData = makeDayData([
      makeExercise({
        weekPrescriptions: new Map([
          [1, { reps: '3x5', percentageRM: 73 }],
        ]),
      }),
    ]);
    // 100 * 73% = 73, rounded to 5 = 75
    const rows = createDaySheetData(dayData, [1], lifts, true, 5);
    expect(rows[2][1]).toBe('75kg');

    // 100 * 73% = 73, rounded to 2.5 = 72.5
    const rows2 = createDaySheetData(dayData, [1], lifts, true, 2.5);
    expect(rows2[2][1]).toBe('72.5kg');
  });

  it('should handle multiple exercises producing multiple row chunks', () => {
    const dayData = makeDayData([
      makeExercise({ exerciseId: 'bench', exerciseName: 'Bench Press' }),
      makeExercise({
        exerciseId: 'squat',
        exerciseName: 'Squat',
        weekPrescriptions: new Map([
          [1, { reps: '4x5' }],
          [2, { reps: '4x4' }],
          [3, { reps: '4x3' }],
        ]),
      }),
    ]);

    const rows = createDaySheetData(dayData, weekNumbers, undefined, false);
    // header(1) + exercise1(1) + spacer(1) + exercise2(1) + spacer(1) = 5
    expect(rows).toHaveLength(5);
    expect(rows[1][0]).toBe('Bench Press');
    expect(rows[3][0]).toBe('Squat');
    expect(rows[3][1]).toBe('4x5');
  });
});
