import type { TodayWorkout, WeekWorkouts, ExerciseLiftHistory } from '@/types';

/**
 * Mock workout data for visual testing without backend.
 * Showcases ALL exercise types: Standard, Tempo, Superset, EMOM, Dropset, and Compound Notation.
 */
export const mockTodayWorkout: TodayWorkout = {
  workoutId: 'mock-workout-1',
  athleteProgramId: 'mock-program-1',
  programName: 'Hypertrophy Block A',
  weekNumber: 3,
  dayNumber: 1,
  dayName: 'Push Day',
  focus: 'UpperBody',
  scheduledDate: new Date().toISOString(),
  status: 'NotStarted',
  statusName: 'NotStarted',
  startedAt: undefined,
  completedDate: undefined,
  durationMinutes: undefined,
  fatigueRating: undefined,
  notes: undefined,
  exercises: [
    // 1. Standard with compound notation (top set + backoff) and %1RM per segment
    {
      exerciseId: 'ex-bench',
      exerciseName: 'Bench Press',
      order: 1,
      prescribedSets: 4,
      prescribedReps: '6-8',
      prescribedRpe: 8,
      restSeconds: 180,
      exerciseNotes: 'Pausa de 1s en el pecho',
      exerciseType: 'Standard',
      percentageRM: undefined, // per-set %RM instead of global
      rawNotation: '1x1 91% + 3x4 80%',
      prescribedWeight: 100,
      sets: [
        { id: 'set-1', setNumber: 1, targetReps: 1, targetWeight: 100, targetRpe: undefined, targetPercentageRM: 91, suggestedWeight: 115, setLabel: 'Top Set', actualReps: 0, actualWeight: 0, isCompleted: false },
        { id: 'set-2', setNumber: 2, targetReps: 4, targetWeight: 100, targetRpe: undefined, targetPercentageRM: 80, suggestedWeight: 102.5, setLabel: 'Backoff 1', actualReps: 0, actualWeight: 0, isCompleted: false },
        { id: 'set-3', setNumber: 3, targetReps: 4, targetWeight: 100, targetRpe: undefined, targetPercentageRM: 80, suggestedWeight: 102.5, setLabel: 'Backoff 2', actualReps: 0, actualWeight: 0, isCompleted: false },
        { id: 'set-4', setNumber: 4, targetReps: 4, targetWeight: 100, targetRpe: undefined, targetPercentageRM: 80, suggestedWeight: 102.5, setLabel: 'Backoff 3', actualReps: 0, actualWeight: 0, isCompleted: false },
      ],
      previousPerformance: {
        date: new Date(Date.now() - 7 * 24 * 60 * 60 * 1000).toISOString(),
        maxWeight: 100,
        maxReps: 8,
        bestRpe: 8.5,
        estimatedOneRM: 126.7,
        totalSets: 4,
      },
    },
    // 2. Tempo exercise
    {
      exerciseId: 'ex-ohp-tempo',
      exerciseName: 'Overhead Press (Tempo)',
      order: 2,
      prescribedSets: 3,
      prescribedReps: '8-10',
      prescribedRpe: 7.5,
      restSeconds: 120,
      exerciseType: 'Tempo',
      tempoConfig: { eccentric: 3, pauseBottom: 1, concentric: 0, pauseTop: 0 },
      prescribedWeight: 55,
      sets: [
        { id: 'set-5', setNumber: 1, targetReps: 10, targetWeight: 55, targetRpe: 7.5, actualReps: 0, actualWeight: 0, isCompleted: false },
        { id: 'set-6', setNumber: 2, targetReps: 10, targetWeight: 55, targetRpe: 7.5, actualReps: 0, actualWeight: 0, isCompleted: false },
        { id: 'set-7', setNumber: 3, targetReps: 8, targetWeight: 57.5, targetRpe: 7.5, actualReps: 0, actualWeight: 0, isCompleted: false },
      ],
      previousPerformance: {
        date: new Date(Date.now() - 7 * 24 * 60 * 60 * 1000).toISOString(),
        maxWeight: 55,
        maxReps: 10,
        bestRpe: 7,
        estimatedOneRM: 73.3,
        totalSets: 3,
      },
    },
    // 3a. Superset A1 — Incline Dumbbell Press
    {
      exerciseId: 'ex-incline-db',
      exerciseName: 'Incline Dumbbell Press',
      order: 3,
      prescribedSets: 3,
      prescribedReps: '10-12',
      prescribedRpe: 8,
      restSeconds: 0, // no rest between superset pairs
      exerciseType: 'Superset',
      supersetGroupId: 'A',
      supersetPosition: 1,
      prescribedWeight: 32,
      sets: [
        { id: 'set-8', setNumber: 1, targetReps: 12, targetWeight: 32, actualReps: 0, actualWeight: 0, isCompleted: false },
        { id: 'set-9', setNumber: 2, targetReps: 12, targetWeight: 32, actualReps: 0, actualWeight: 0, isCompleted: false },
        { id: 'set-10', setNumber: 3, targetReps: 10, targetWeight: 34, actualReps: 0, actualWeight: 0, isCompleted: false },
      ],
    },
    // 3b. Superset A2 — Cable Fly
    {
      exerciseId: 'ex-cable-fly',
      exerciseName: 'Cable Fly',
      order: 4,
      prescribedSets: 3,
      prescribedReps: '12-15',
      prescribedRpe: 7,
      restSeconds: 90, // rest after full superset round
      exerciseType: 'Superset',
      supersetGroupId: 'A',
      supersetPosition: 2,
      prescribedWeight: 15,
      sets: [
        { id: 'set-11', setNumber: 1, targetReps: 15, targetWeight: 15, actualReps: 0, actualWeight: 0, isCompleted: false },
        { id: 'set-12', setNumber: 2, targetReps: 15, targetWeight: 15, actualReps: 0, actualWeight: 0, isCompleted: false },
        { id: 'set-13', setNumber: 3, targetReps: 12, targetWeight: 15, actualReps: 0, actualWeight: 0, isCompleted: false },
      ],
    },
    // 4. EMOM exercise
    {
      exerciseId: 'ex-kb-press',
      exerciseName: 'Kettlebell Push Press (EMOM)',
      order: 5,
      prescribedSets: 10,
      prescribedReps: '5',
      restSeconds: undefined,
      exerciseType: 'Emom',
      emomConfig: { totalMinutes: 10, workSeconds: 30, restSeconds: 30 },
      prescribedWeight: 24,
      sets: Array.from({ length: 10 }, (_, i) => ({
        id: `set-emom-${i + 1}`,
        setNumber: i + 1,
        targetReps: 5,
        targetWeight: 24,
        actualReps: 0,
        actualWeight: 0,
        isCompleted: false,
      })),
    },
    // 5. Dropset exercise
    {
      exerciseId: 'ex-tricep-pushdown',
      exerciseName: 'Tricep Pushdown (Dropset)',
      order: 6,
      prescribedSets: 3,
      prescribedReps: '12-15',
      prescribedRpe: 9,
      restSeconds: 60,
      exerciseType: 'Dropset',
      rawNotation: '1x12 1x10 1x8',
      prescribedWeight: 27.5,
      sets: [
        { id: 'set-14', setNumber: 1, targetReps: 12, targetWeight: 27.5, setLabel: 'Top Set', actualReps: 0, actualWeight: 0, isCompleted: false },
        { id: 'set-15', setNumber: 2, targetReps: 10, targetWeight: 22, setLabel: 'Drop 1', actualReps: 0, actualWeight: 0, isCompleted: false },
        { id: 'set-16', setNumber: 3, targetReps: 8, targetWeight: 17.5, setLabel: 'Drop 2', actualReps: 0, actualWeight: 0, isCompleted: false },
      ],
    },
  ],
};

/**
 * Mock workout in "InProgress" state — some sets already completed.
 */
export const mockInProgressWorkout: TodayWorkout = {
  ...mockTodayWorkout,
  status: 'InProgress',
  statusName: 'InProgress',
  startedAt: new Date(Date.now() - 25 * 60 * 1000).toISOString(), // 25 min ago
  exercises: mockTodayWorkout.exercises.map((ex, i) => {
    if (i === 0) {
      // Bench Press — 2 sets done (top set at 91% + first backoff at 80%)
      return {
        ...ex,
        sets: ex.sets.map((s, j) =>
          j < 2
            ? { ...s, actualWeight: j === 0 ? 115 : 100, actualReps: j === 0 ? 1 : 4, actualRpe: j === 0 ? 9.5 : 7.5, isCompleted: true }
            : s,
        ),
      };
    }
    return ex;
  }),
};

// ── Mock workouts for other days (used by day navigation) ──
const today = new Date();
const dayMs = 24 * 60 * 60 * 1000;

/** Day 1 — Completed Push Day (2 days ago) */
export const mockDay1Workout: TodayWorkout = {
  workoutId: 'mock-w3d1',
  athleteProgramId: 'mock-program-1',
  programName: 'Hypertrophy Block A',
  weekNumber: 3,
  dayNumber: 1,
  dayName: 'Push Day',
  focus: 'UpperBody',
  scheduledDate: new Date(today.getTime() - 2 * dayMs).toISOString(),
  status: 'Completed',
  statusName: 'Completed',
  startedAt: new Date(today.getTime() - 2 * dayMs - 68 * 60000).toISOString(),
  completedDate: new Date(today.getTime() - 2 * dayMs).toISOString(),
  durationMinutes: 68,
  fatigueRating: 3,
  notes: 'Buen día, buenas sensaciones.',
  exercises: [
    {
      exerciseId: 'ex-bench',
      exerciseName: 'Bench Press',
      order: 1,
      prescribedSets: 4,
      prescribedReps: '6-8',
      prescribedRpe: 8,
      restSeconds: 180,
      exerciseNotes: 'Control la bajada',
      exerciseType: 'Standard',
      rawNotation: '4x6 @8',
      prescribedWeight: 95,
      sets: [
        { id: 'd1-s1', setNumber: 1, targetReps: 6, targetWeight: 95, actualReps: 6, actualWeight: 95, actualRpe: 7.5, isCompleted: true },
        { id: 'd1-s2', setNumber: 2, targetReps: 6, targetWeight: 95, actualReps: 6, actualWeight: 95, actualRpe: 8, isCompleted: true },
        { id: 'd1-s3', setNumber: 3, targetReps: 6, targetWeight: 95, actualReps: 5, actualWeight: 95, actualRpe: 8.5, isCompleted: true },
        { id: 'd1-s4', setNumber: 4, targetReps: 6, targetWeight: 95, actualReps: 5, actualWeight: 92.5, actualRpe: 8.5, isCompleted: true },
      ],
      previousPerformance: { date: new Date(today.getTime() - 9 * dayMs).toISOString(), maxWeight: 92.5, maxReps: 6, estimatedOneRM: 110.8, totalSets: 4 },
    },
    {
      exerciseId: 'ex-ohp',
      exerciseName: 'Overhead Press',
      order: 2,
      prescribedSets: 3,
      prescribedReps: '8-10',
      prescribedRpe: 7.5,
      restSeconds: 120,
      exerciseType: 'Standard',
      prescribedWeight: 50,
      sets: [
        { id: 'd1-s5', setNumber: 1, targetReps: 10, targetWeight: 50, actualReps: 10, actualWeight: 50, actualRpe: 7, isCompleted: true },
        { id: 'd1-s6', setNumber: 2, targetReps: 10, targetWeight: 50, actualReps: 9, actualWeight: 50, actualRpe: 7.5, isCompleted: true },
        { id: 'd1-s7', setNumber: 3, targetReps: 8, targetWeight: 50, actualReps: 8, actualWeight: 50, actualRpe: 8, isCompleted: true },
      ],
    },
  ],
};

/** Day 2 — Completed Pull Day (yesterday) */
export const mockDay2Workout: TodayWorkout = {
  workoutId: 'mock-w3d2',
  athleteProgramId: 'mock-program-1',
  programName: 'Hypertrophy Block A',
  weekNumber: 3,
  dayNumber: 2,
  dayName: 'Pull Day',
  focus: 'UpperBody',
  scheduledDate: new Date(today.getTime() - 1 * dayMs).toISOString(),
  status: 'Completed',
  statusName: 'Completed',
  startedAt: new Date(today.getTime() - 1 * dayMs - 72 * 60000).toISOString(),
  completedDate: new Date(today.getTime() - 1 * dayMs).toISOString(),
  durationMinutes: 72,
  fatigueRating: 4,
  notes: 'Peso muerto se sintió pesado.',
  exercises: [
    {
      exerciseId: 'ex-deadlift',
      exerciseName: 'Deadlift',
      order: 1,
      prescribedSets: 3,
      prescribedReps: '5',
      prescribedRpe: 8.5,
      restSeconds: 180,
      exerciseNotes: 'Mantener espalda neutra',
      exerciseType: 'Standard',
      prescribedWeight: 140,
      sets: [
        { id: 'd2-s1', setNumber: 1, targetReps: 5, targetWeight: 140, actualReps: 5, actualWeight: 140, actualRpe: 8, isCompleted: true },
        { id: 'd2-s2', setNumber: 2, targetReps: 5, targetWeight: 140, actualReps: 5, actualWeight: 140, actualRpe: 8.5, isCompleted: true },
        { id: 'd2-s3', setNumber: 3, targetReps: 5, targetWeight: 140, actualReps: 4, actualWeight: 140, actualRpe: 9, isCompleted: true },
      ],
      previousPerformance: { date: new Date(today.getTime() - 8 * dayMs).toISOString(), maxWeight: 135, maxReps: 5, estimatedOneRM: 157.5, totalSets: 3 },
    },
    {
      exerciseId: 'ex-row',
      exerciseName: 'Barbell Row',
      order: 2,
      prescribedSets: 4,
      prescribedReps: '8-10',
      prescribedRpe: 7.5,
      restSeconds: 90,
      exerciseType: 'Standard',
      prescribedWeight: 70,
      sets: [
        { id: 'd2-s4', setNumber: 1, targetReps: 10, targetWeight: 70, actualReps: 10, actualWeight: 70, actualRpe: 7, isCompleted: true },
        { id: 'd2-s5', setNumber: 2, targetReps: 10, targetWeight: 70, actualReps: 10, actualWeight: 70, actualRpe: 7.5, isCompleted: true },
        { id: 'd2-s6', setNumber: 3, targetReps: 10, targetWeight: 70, actualReps: 9, actualWeight: 70, actualRpe: 8, isCompleted: true },
        { id: 'd2-s7', setNumber: 4, targetReps: 8, targetWeight: 70, actualReps: 8, actualWeight: 70, actualRpe: 8, isCompleted: true },
      ],
    },
  ],
};

/** Day 4 — NotStarted Leg Day (tomorrow) */
export const mockDay4Workout: TodayWorkout = {
  workoutId: 'mock-w3d4',
  athleteProgramId: 'mock-program-1',
  programName: 'Hypertrophy Block A',
  weekNumber: 3,
  dayNumber: 4,
  dayName: 'Legs',
  focus: 'Legs',
  scheduledDate: new Date(today.getTime() + 1 * dayMs).toISOString(),
  status: 'NotStarted',
  statusName: 'NotStarted',
  exercises: [
    {
      exerciseId: 'ex-squat',
      exerciseName: 'Back Squat',
      order: 1,
      prescribedSets: 4,
      prescribedReps: '6-8',
      prescribedRpe: 8,
      restSeconds: 180,
      exerciseNotes: 'Profundidad completa. ATG si la movilidad lo permite.',
      exerciseType: 'Standard',
      prescribedWeight: 120,
      sets: [
        { id: 'd4-s1', setNumber: 1, targetReps: 8, targetWeight: 120, actualReps: 0, actualWeight: 0, isCompleted: false },
        { id: 'd4-s2', setNumber: 2, targetReps: 8, targetWeight: 120, actualReps: 0, actualWeight: 0, isCompleted: false },
        { id: 'd4-s3', setNumber: 3, targetReps: 6, targetWeight: 125, actualReps: 0, actualWeight: 0, isCompleted: false },
        { id: 'd4-s4', setNumber: 4, targetReps: 6, targetWeight: 125, actualReps: 0, actualWeight: 0, isCompleted: false },
      ],
    },
    {
      exerciseId: 'ex-rdl',
      exerciseName: 'Romanian Deadlift',
      order: 2,
      prescribedSets: 3,
      prescribedReps: '10-12',
      prescribedRpe: 7,
      restSeconds: 90,
      exerciseType: 'Standard',
      prescribedWeight: 80,
      sets: [
        { id: 'd4-s5', setNumber: 1, targetReps: 12, targetWeight: 80, actualReps: 0, actualWeight: 0, isCompleted: false },
        { id: 'd4-s6', setNumber: 2, targetReps: 12, targetWeight: 80, actualReps: 0, actualWeight: 0, isCompleted: false },
        { id: 'd4-s7', setNumber: 3, targetReps: 10, targetWeight: 80, actualReps: 0, actualWeight: 0, isCompleted: false },
      ],
    },
  ],
};

/** Day 5 — NotStarted Upper Pull (3 days from now) */
export const mockDay5Workout: TodayWorkout = {
  workoutId: 'mock-w3d5',
  athleteProgramId: 'mock-program-1',
  programName: 'Hypertrophy Block A',
  weekNumber: 3,
  dayNumber: 5,
  dayName: 'Upper Pull',
  focus: 'UpperBody',
  scheduledDate: new Date(today.getTime() + 3 * dayMs).toISOString(),
  status: 'NotStarted',
  statusName: 'NotStarted',
  exercises: [
    {
      exerciseId: 'ex-pullup',
      exerciseName: 'Pull-ups',
      order: 1,
      prescribedSets: 4,
      prescribedReps: '6-10',
      prescribedRpe: 8,
      restSeconds: 120,
      exerciseNotes: 'Agarre prono, rango completo',
      exerciseType: 'Standard',
      sets: [
        { id: 'd5-s1', setNumber: 1, targetReps: 10, actualReps: 0, actualWeight: 0, isCompleted: false },
        { id: 'd5-s2', setNumber: 2, targetReps: 10, actualReps: 0, actualWeight: 0, isCompleted: false },
        { id: 'd5-s3', setNumber: 3, targetReps: 8, actualReps: 0, actualWeight: 0, isCompleted: false },
        { id: 'd5-s4', setNumber: 4, targetReps: 6, actualReps: 0, actualWeight: 0, isCompleted: false },
      ],
    },
    {
      exerciseId: 'ex-face-pull',
      exerciseName: 'Face Pull',
      order: 2,
      prescribedSets: 3,
      prescribedReps: '15-20',
      prescribedRpe: 7,
      restSeconds: 60,
      exerciseType: 'Standard',
      prescribedWeight: 15,
      sets: [
        { id: 'd5-s5', setNumber: 1, targetReps: 20, targetWeight: 15, actualReps: 0, actualWeight: 0, isCompleted: false },
        { id: 'd5-s6', setNumber: 2, targetReps: 20, targetWeight: 15, actualReps: 0, actualWeight: 0, isCompleted: false },
        { id: 'd5-s7', setNumber: 3, targetReps: 15, targetWeight: 15, actualReps: 0, actualWeight: 0, isCompleted: false },
      ],
    },
  ],
};

/**
 * Map of workoutId → TodayWorkout for day navigation in mock mode.
 */
export const mockWorkoutsByDay: Record<string, TodayWorkout> = {
  'mock-w3d1': mockDay1Workout,
  'mock-w3d2': mockDay2Workout,
  'mock-workout-1': mockTodayWorkout,  // day 3 = today
  'mock-w3d4': mockDay4Workout,
  'mock-w3d5': mockDay5Workout,
};

// ── Week Strip Mock Data ───────────────────────────────────

export const mockWeekWorkouts: WeekWorkouts = {
  programName: 'Hypertrophy Block A',
  weekNumber: 3,
  totalWeeks: 8,
  days: [
    {
      workoutId: 'mock-w3d1',
      dayNumber: 1,
      dayName: 'Push Day',
      focus: 'UpperBody',
      scheduledDate: new Date(today.getTime() - 2 * dayMs).toISOString(),
      status: 'Completed',
      statusName: 'Completed',
      isToday: false,
      durationMinutes: 68,
      fatigueRating: 7,
      exerciseCount: 5,
      completedSets: 18,
      totalSets: 18,
    },
    {
      workoutId: 'mock-w3d2',
      dayNumber: 2,
      dayName: 'Pull Day',
      focus: 'UpperBody',
      scheduledDate: new Date(today.getTime() - 1 * dayMs).toISOString(),
      status: 'Completed',
      statusName: 'Completed',
      isToday: false,
      durationMinutes: 72,
      fatigueRating: 8,
      exerciseCount: 6,
      completedSets: 20,
      totalSets: 20,
    },
    {
      workoutId: 'mock-workout-1',
      dayNumber: 3,
      dayName: 'Push Day',
      focus: 'UpperBody',
      scheduledDate: today.toISOString(),
      status: 'InProgress',
      statusName: 'InProgress',
      isToday: true,
      durationMinutes: undefined,
      fatigueRating: undefined,
      exerciseCount: 6,
      completedSets: 3,
      totalSets: 18,
    },
    {
      workoutId: 'mock-w3d4',
      dayNumber: 4,
      dayName: 'Legs',
      focus: 'Legs',
      scheduledDate: new Date(today.getTime() + 1 * dayMs).toISOString(),
      status: 'NotStarted',
      statusName: 'NotStarted',
      isToday: false,
      durationMinutes: undefined,
      fatigueRating: undefined,
      exerciseCount: 5,
      completedSets: 0,
      totalSets: 0,
    },
    {
      workoutId: 'mock-w3d5',
      dayNumber: 5,
      dayName: 'Upper Pull',
      focus: 'UpperBody',
      scheduledDate: new Date(today.getTime() + 3 * dayMs).toISOString(),
      status: 'NotStarted',
      statusName: 'NotStarted',
      isToday: false,
      durationMinutes: undefined,
      fatigueRating: undefined,
      exerciseCount: 5,
      completedSets: 0,
      totalSets: 0,
    },
  ],
};

// ── Exercise Lift History Mock Data ─────────────────────────
export const mockLiftHistory: ExerciseLiftHistory = {
  exerciseId: 'ex-bench',
  exerciseName: 'Bench Press',
  category: 'Bench',
  primaryMuscleGroup: 'Chest',
  equipment: 'Barbell',
  videoUrl: 'https://www.youtube.com/embed/rT7DgCr-3pg',
  imageUrl: undefined,
  description: 'Compound barbell pressing movement. Primary chest builder.',
  isCompound: true,
  instructions: [
    'Lie flat on the bench with feet firmly on the ground.',
    'Grip the bar slightly wider than shoulder width.',
    'Unrack and lower the bar to mid-chest, elbows at ~45°.',
    'Press the bar back up to lockout, driving through the chest.',
    'Maintain upper back tightness throughout the lift.',
  ],
  coachingCues: [
    'Arch back',
    'Drive feet into floor',
    'Squeeze shoulder blades',
    'Elbows 45°',
    'Pause at chest',
    'Explode up',
  ],
  currentEstimated1RM: 128.3,
  personalRecords: {
    maxWeight: 120,
    maxWeightDate: new Date(today.getTime() - 7 * dayMs).toISOString(),
    maxReps: 12,
    maxRepsDate: new Date(today.getTime() - 28 * dayMs).toISOString(),
    maxEstimated1RM: 128.3,
    maxEstimated1RMDate: new Date(today.getTime() - 7 * dayMs).toISOString(),
    maxVolume: 4800,
    maxVolumeDate: new Date(today.getTime() - 14 * dayMs).toISOString(),
  },
  entries: [
    { date: new Date(today.getTime() - 7 * dayMs).toISOString(), weekNumber: 2, dayNumber: 1, maxWeight: 120, bestReps: 3, rpe: 9, estimated1RM: 128.3, totalVolume: 3560, totalSets: 4 },
    { date: new Date(today.getTime() - 14 * dayMs).toISOString(), weekNumber: 1, dayNumber: 1, maxWeight: 115, bestReps: 5, rpe: 8.5, estimated1RM: 126.7, totalVolume: 4800, totalSets: 5 },
    { date: new Date(today.getTime() - 21 * dayMs).toISOString(), weekNumber: 4, dayNumber: 1, maxWeight: 112.5, bestReps: 5, rpe: 8, estimated1RM: 123.9, totalVolume: 4500, totalSets: 5 },
    { date: new Date(today.getTime() - 28 * dayMs).toISOString(), weekNumber: 3, dayNumber: 1, maxWeight: 100, bestReps: 12, rpe: 8, estimated1RM: 120.0, totalVolume: 4400, totalSets: 4 },
    { date: new Date(today.getTime() - 35 * dayMs).toISOString(), weekNumber: 2, dayNumber: 1, maxWeight: 107.5, bestReps: 6, rpe: 8, estimated1RM: 119.2, totalVolume: 4200, totalSets: 5 },
    { date: new Date(today.getTime() - 42 * dayMs).toISOString(), weekNumber: 1, dayNumber: 1, maxWeight: 105, bestReps: 6, rpe: 7.5, estimated1RM: 116.7, totalVolume: 4100, totalSets: 5 },
    { date: new Date(today.getTime() - 49 * dayMs).toISOString(), weekNumber: 4, dayNumber: 1, maxWeight: 100, bestReps: 8, rpe: 8, estimated1RM: 113.3, totalVolume: 3900, totalSets: 4 },
    { date: new Date(today.getTime() - 56 * dayMs).toISOString(), weekNumber: 3, dayNumber: 1, maxWeight: 97.5, bestReps: 8, rpe: 7, estimated1RM: 110.0, totalVolume: 3600, totalSets: 4 },
  ],
};