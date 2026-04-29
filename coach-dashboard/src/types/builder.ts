// Program Builder types for local state management

import { DayFocus } from './program';

/**
 * Exercise execution type
 */
export type ExerciseType = 'standard' | 'emom' | 'tempo' | 'superset' | 'circuit' | 'dropset';

/**
 * EMOM configuration
 */
export interface EMOMConfig {
  totalMinutes: number;      // Total duration in minutes
  workSeconds?: number;      // Work period (if interval-based)
  restSeconds?: number;      // Rest period within EMOM (if interval-based)
}

/**
 * Tempo configuration (eccentric:pause:concentric:pause)
 */
export interface TempoConfig {
  eccentric: number;         // Lowering phase (e.g., 3 seconds)
  pauseBottom: number;       // Pause at bottom (e.g., 1 second)
  concentric: number;        // Lifting phase (e.g., 0 = explosive)
  pauseTop?: number;         // Pause at top (optional)
}

/**
 * Superset configuration
 */
export interface SupersetConfig {
  groupId: string;           // Links exercises in the same superset
  position: number;          // Order within the superset (1, 2, 3...)
}

/**
 * Exercise type labels and icons
 */
export const EXERCISE_TYPE_CONFIG: Record<ExerciseType, { 
  label: string; 
  shortLabel: string;
  icon: string; 
  color: string;
  description: string;
}> = {
  standard: {
    label: 'Standard',
    shortLabel: 'STD',
    icon: 'Dumbbell',
    color: 'slate',
    description: 'Regular sets and reps',
  },
  emom: {
    label: 'EMOM',
    shortLabel: 'EMOM',
    icon: 'Timer',
    color: 'blue',
    description: 'Every Minute On the Minute',
  },
  tempo: {
    label: 'Tempo',
    shortLabel: 'TEMPO',
    icon: 'Clock',
    color: 'purple',
    description: 'Controlled tempo execution',
  },
  superset: {
    label: 'Superset',
    shortLabel: 'SS',
    icon: 'Layers',
    color: 'orange',
    description: 'Paired exercises back-to-back',
  },
  circuit: {
    label: 'Circuit',
    shortLabel: 'CIR',
    icon: 'RotateCw',
    color: 'green',
    description: 'Multiple exercises in sequence',
  },
  dropset: {
    label: 'Drop Set',
    shortLabel: 'DROP',
    icon: 'TrendingDown',
    color: 'red',
    description: 'Reduce weight each set without rest',
  },
};

/**
 * Local exercise state for the builder (includes temporary IDs for new items)
 */
export interface BuilderExercise {
  id: string;
  tempId?: string; // For optimistic updates before server response
  exerciseId: string;
  exerciseName: string;
  order: number;
  sets: number;
  repsMin: number;
  repsMax: number;
  restSeconds: number;
  rpeTarget?: number;
  notes?: string;
  isNew?: boolean;
  isDirty?: boolean;
  // New exercise type fields
  exerciseType?: ExerciseType;
  emomConfig?: EMOMConfig;
  tempoConfig?: TempoConfig;
  supersetConfig?: SupersetConfig;
  percentageRM?: number;
  /** Raw notation string for compound sets (e.g., "1x1 3x4") */
  rawNotation?: string;
  /** Manual weight override in kg */
  weight?: number;
}

/**
 * Local day state for the builder
 */
export interface BuilderDay {
  id: string;
  tempId?: string;
  dayNumber: number;
  name: string;
  focus: DayFocus | null;
  notes?: string;
  exercises: BuilderExercise[];
  isNew?: boolean;
  isDirty?: boolean;
  isCollapsed?: boolean;
}

/**
 * Local week state for the builder
 */
export interface BuilderWeek {
  id: string;
  tempId?: string;
  weekNumber: number;
  name: string;
  notes?: string;
  days: BuilderDay[];
  isNew?: boolean;
  isDirty?: boolean;
  isCollapsed?: boolean;
}

/**
 * Complete builder state
 */
export interface BuilderState {
  programId: string;
  programName: string;
  description?: string;
  durationWeeks: number;
  weeks: BuilderWeek[];
  isLoading: boolean;
  isSaving: boolean;
  hasUnsavedChanges: boolean;
  lastSavedAt?: string;
  activeWeekId?: string;
  activeDayId?: string;
  activeExerciseId?: string;
  /** History stack for undo functionality (max 10 snapshots) */
  history?: BuilderWeek[][];
  /** Exercise currently open in the progression panel */
  progressionPanel?: { exerciseId: string; exerciseName: string };
}

/**
 * Builder actions
 */
export type BuilderAction =
  | { type: 'SET_PROGRAM'; payload: { programId: string; programName: string; description?: string; durationWeeks: number; weeks: BuilderWeek[] } }
  | { type: 'SET_LOADING'; payload: boolean }
  | { type: 'SET_SAVING'; payload: boolean }
  | { type: 'ADD_WEEK'; payload: { week: BuilderWeek } }
  | { type: 'UPDATE_WEEK'; payload: { weekId: string; updates: Partial<BuilderWeek> } }
  | { type: 'DELETE_WEEK'; payload: { weekId: string } }
  | { type: 'DUPLICATE_WEEK'; payload: { weekId: string; newWeek: BuilderWeek } }
  | { type: 'REORDER_WEEKS'; payload: { weeks: BuilderWeek[] } }
  | { type: 'ADD_DAY'; payload: { weekId: string; day: BuilderDay } }
  | { type: 'UPDATE_DAY'; payload: { weekId: string; dayId: string; updates: Partial<BuilderDay> } }
  | { type: 'DELETE_DAY'; payload: { weekId: string; dayId: string } }
  | { type: 'DUPLICATE_DAY'; payload: { weekId: string; dayId: string; newDay: BuilderDay } }
  | { type: 'REORDER_DAYS'; payload: { weekId: string; days: BuilderDay[] } }
  | { type: 'ADD_EXERCISE'; payload: { weekId: string; dayId: string; exercise: BuilderExercise } }
  | { type: 'UPDATE_EXERCISE'; payload: { weekId: string; dayId: string; exerciseId: string; updates: Partial<BuilderExercise> } }
  | { type: 'DELETE_EXERCISE'; payload: { weekId: string; dayId: string; exerciseId: string } }
  | { type: 'DUPLICATE_EXERCISE'; payload: { weekId: string; dayId: string; exerciseId: string; newExercise: BuilderExercise } }
  | { type: 'REORDER_EXERCISES'; payload: { weekId: string; dayId: string; exercises: BuilderExercise[] } }
  | { type: 'TOGGLE_WEEK_COLLAPSE'; payload: { weekId: string } }
  | { type: 'TOGGLE_DAY_COLLAPSE'; payload: { weekId: string; dayId: string } }
  | { type: 'SET_ACTIVE_WEEK'; payload: { weekId: string | undefined } }
  | { type: 'SET_ACTIVE_DAY'; payload: { dayId: string | undefined } }
  | { type: 'SET_ACTIVE_EXERCISE'; payload: { exerciseId: string | undefined } }
  | { type: 'MARK_SAVED' }
  | { type: 'PUSH_UNDO' }
  | { type: 'UNDO' }
  | { type: 'APPLY_PRESET'; payload: { preset: 'pl3' | 'ul4' | 'ppl6' } }
  | { type: 'COPY_WEEK_STRUCTURE'; payload: { sourceWeekId: string; targetWeekIds: string[]; includeExercises: boolean; progressionPercent?: number } }
  | { type: 'GENERATE_DELOAD_WEEK'; payload: { afterWeekId: string; volumePercent: number } }
  | { type: 'OPEN_PROGRESSION_PANEL'; payload: { exerciseId: string; exerciseName: string } }
  | { type: 'CLOSE_PROGRESSION_PANEL' };

/**
 * Exercise option for autocomplete
 */
export interface ExerciseOption {
  id: string;
  name: string;
  category: string;
  muscleGroup: string;
}

/**
 * Day focus options with labels
 */
export const DAY_FOCUS_OPTIONS: { value: DayFocus; label: string }[] = [
  { value: 'Squat', label: 'Squat' },
  { value: 'Bench', label: 'Bench' },
  { value: 'Deadlift', label: 'Deadlift' },
  { value: 'Hypertrophy', label: 'Hypertrophy' },
  { value: 'Push', label: 'Push' },
  { value: 'Pull', label: 'Pull' },
  { value: 'Legs', label: 'Legs' },
  { value: 'UpperBody', label: 'Upper Body' },
  { value: 'LowerBody', label: 'Lower Body' },
  { value: 'FullBody', label: 'Full Body' },
  { value: 'Accessories', label: 'Accessories' },
  { value: 'Recovery', label: 'Recovery' },
  { value: 'Rest', label: 'Rest' },
  { value: 'Cardio', label: 'Cardio' },
  { value: 'Custom', label: 'Custom' },
];

/**
 * View mode for Program Builder
 */
export type BuilderViewMode = 'weekly' | 'progression' | 'day-centric' | 'calendar';

/**
 * Day-Centric View: Represents a unique "day slot" across all weeks
 * Example: "Día 1" appears in Week 1, Week 2, etc. with same exercises but different prescriptions
 */
export interface DaySlot {
  dayNumber: number;
  name: string;
  focus: string;
  /** Map of exerciseId/name to its instances across weeks */
  exercises: DaySlotExercise[];
}

/**
 * Exercise within a day slot, with prescriptions for each week
 */
export interface DaySlotExercise {
  exerciseId: string;
  exerciseName: string;
  order: number;
  /** Map of weekNumber to the prescription for that week */
  weekPrescriptions: Map<number, WeekPrescription>;
  /** Exercise type (standard, emom, tempo, etc.) */
  exerciseType?: ExerciseType;
  /** EMOM configuration if exerciseType is 'emom' */
  emomConfig?: EMOMConfig;
  /** Tempo configuration if exerciseType is 'tempo' */
  tempoConfig?: TempoConfig;
  /** Superset configuration if exerciseType is 'superset' */
  supersetConfig?: SupersetConfig;
}

/**
 * A single week's prescription for an exercise
 */
export interface WeekPrescription {
  weekId: string;
  dayId: string;
  exerciseEntryId: string;
  sets: number;
  reps: string;           // Flexible notation: "3x8", "1x1 3x4", "AMRAP"
  percentageRM?: number;
  rpeTarget?: number;
  weight?: number;        // Suggested or manual weight
  restSeconds?: number;
  tempo?: string;         // "3:1:0"
  notes?: string;
}

/**
 * Exercise progression data for a single exercise across all weeks
 */
export interface ExerciseProgression {
  exerciseId: string;
  exerciseName: string;
  instances: ExerciseProgressionInstance[];
}

/**
 * Single instance of an exercise in a specific week/day
 */
export interface ExerciseProgressionInstance {
  weekId: string;
  weekNumber: number;
  dayId: string;
  dayName: string;
  exerciseEntryId: string;
  sets: number;
  repsMin: number;
  repsMax: number;
  rpeTarget?: number;
  percentageRM?: number;
  restSeconds: number;
  notes?: string;
}

/**
 * Auto progression settings
 */
export interface AutoProgressionSettings {
  startPercentage: number;
  endPercentage: number;
  incrementType: 'linear' | 'wave';
}

/**
 * Progression type for generating exercise progressions
 */
export type ProgressionType = 'linear' | 'wave' | 'step' | 'peak';

/**
 * Progression template preset
 */
export type ProgressionTemplate = 'hypertrophy' | 'strength' | 'peaking' | 'deload' | 'custom';

/**
 * Progression generator settings
 */
export interface ProgressionGeneratorSettings {
  exerciseId: string;
  exerciseName: string;
  totalWeeks: number;
  sets: number;
  repsMin: number;
  repsMax: number;
  startPercentage: number;
  endPercentage: number;
  startRPE: number;
  endRPE: number;
  restSeconds: number;
  progressionType: ProgressionType;
  template?: ProgressionTemplate;
  useAthleteRM?: boolean;
  athleteRM?: number;
}

/**
 * Generated week data from progression generator
 */
export interface GeneratedWeekData {
  weekNumber: number;
  sets: number;
  repsMin: number;
  repsMax: number;
  percentageRM: number;
  rpeTarget: number;
  restSeconds: number;
  weight?: number;
  isDeload?: boolean;
}

/**
 * Progression template definitions with default values
 */
export const PROGRESSION_TEMPLATES: Record<ProgressionTemplate, {
  label: string;
  description: string;
  defaults: Partial<ProgressionGeneratorSettings>;
}> = {
  hypertrophy: {
    label: 'Hypertrophy Block',
    description: 'High volume, moderate intensity for muscle growth',
    defaults: {
      sets: 4,
      repsMin: 8,
      repsMax: 12,
      startPercentage: 60,
      endPercentage: 72,
      startRPE: 6,
      endRPE: 8,
      restSeconds: 90,
      progressionType: 'linear',
    },
  },
  strength: {
    label: 'Strength Block',
    description: 'Lower volume, higher intensity for strength gains',
    defaults: {
      sets: 5,
      repsMin: 3,
      repsMax: 5,
      startPercentage: 75,
      endPercentage: 88,
      startRPE: 7,
      endRPE: 9,
      restSeconds: 180,
      progressionType: 'step',
    },
  },
  peaking: {
    label: 'Peaking Block',
    description: 'Progressive overload to peak performance',
    defaults: {
      sets: 3,
      repsMin: 1,
      repsMax: 3,
      startPercentage: 80,
      endPercentage: 95,
      startRPE: 8,
      endRPE: 10,
      restSeconds: 240,
      progressionType: 'peak',
    },
  },
  deload: {
    label: 'Deload Block',
    description: 'Recovery week with reduced volume and intensity',
    defaults: {
      sets: 2,
      repsMin: 8,
      repsMax: 10,
      startPercentage: 50,
      endPercentage: 55,
      startRPE: 5,
      endRPE: 6,
      restSeconds: 90,
      progressionType: 'linear',
    },
  },
  custom: {
    label: 'Custom',
    description: 'Define your own progression parameters',
    defaults: {},
  },
};

/**
 * Progression type definitions
 */
export const PROGRESSION_TYPES: Record<ProgressionType, {
  label: string;
  description: string;
}> = {
  linear: {
    label: 'Linear',
    description: 'Constant increment from start to end',
  },
  wave: {
    label: 'Wave',
    description: 'Oscillating intensity pattern',
  },
  step: {
    label: 'Step',
    description: 'Block increases in intensity',
  },
  peak: {
    label: 'Peak',
    description: 'Build to maximum then deload',
  },
};
