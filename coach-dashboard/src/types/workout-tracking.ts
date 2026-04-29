// Workout Tracking types (F-015)

export type WorkoutStatus = 'NotStarted' | 'InProgress' | 'Completed' | 'Skipped' | 'PartiallyCompleted';

export type ExerciseType = 'Standard' | 'Emom' | 'Tempo' | 'Superset' | 'Circuit' | 'Dropset';

// ==========================================
// Config sub-types
// ==========================================

export interface EmomConfig {
  totalMinutes: number;
  workSeconds?: number;
  restSeconds?: number;
}

export interface TempoConfig {
  eccentric: number;
  pauseBottom: number;
  concentric: number;
  pauseTop?: number;
}

// ==========================================
// Response DTOs
// ==========================================

export interface TodayWorkout {
  workoutId: string;
  athleteProgramId: string;
  programName: string;
  weekNumber: number;
  dayNumber: number;
  dayName?: string;
  focus?: string;
  scheduledDate: string;
  status: WorkoutStatus;
  statusName: string;
  startedAt?: string;
  completedDate?: string;
  durationMinutes?: number;
  fatigueRating?: number;
  notes?: string;
  weekNotes?: string;
  exercises: WorkoutExerciseGroup[];
}

export interface WorkoutExerciseGroup {
  exerciseId: string;
  exerciseName: string;
  order: number;
  prescribedSets: number;
  prescribedReps?: string;
  prescribedRpe?: number;
  restSeconds?: number;
  exerciseNotes?: string;
  // Exercise type support
  exerciseType: ExerciseType;
  percentageRM?: number;
  rawNotation?: string;
  prescribedWeight?: number;
  emomConfig?: EmomConfig;
  tempoConfig?: TempoConfig;
  supersetGroupId?: string;
  supersetPosition?: number;
  // Data
  sets: WorkoutSet[];
  previousPerformance?: PreviousPerformance;
}

export interface WorkoutSet {
  id: string;
  setNumber: number;
  targetReps?: number;
  targetWeight?: number;
  targetRpe?: number;
  targetPercentageRM?: number;
  /** Suggested weight from estimated1RM × %RM, rounded to 2.5kg. Null when no data. */
  suggestedWeight?: number;
  setLabel?: string;
  actualReps: number;
  actualWeight: number;
  actualRpe?: number;
  isCompleted: boolean;
  skippedReason?: string;
  notes?: string;
}

export interface PreviousPerformance {
  date: string;
  maxWeight: number;
  maxReps: number;
  bestRpe?: number;
  estimatedOneRM?: number;
  totalSets: number;
}

export interface WorkoutHistoryItem {
  id: string;
  weekNumber: number;
  dayNumber: number;
  dayName?: string;
  focus?: string;
  scheduledDate: string;
  status: WorkoutStatus;
  statusName: string;
  completedDate?: string;
  durationMinutes?: number;
  fatigueRating?: number;
  exerciseCount: number;
  completedSets: number;
  totalSets: number;
}

// ==========================================
// Request DTOs
// ==========================================

export interface SaveSetRequest {
  exerciseLogId?: string;
  exerciseId: string;
  setNumber: number;
  reps: number;
  weight: number;
  rpe?: number;
  targetReps?: number;
  targetWeight?: number;
  isCompleted: boolean;
  notes?: string;
}

export interface UpdateSetRequest {
  reps: number;
  weight: number;
  rpe?: number;
  notes?: string;
}

export interface SkipWorkoutRequest {
  reason?: string;
}

export interface CompleteWorkoutRequest {
  durationMinutes?: number;
  fatigueRating?: number;
  notes?: string;
}

// ==========================================
// Week Navigation
// ==========================================

export interface WeekWorkouts {
  programName: string;
  weekNumber: number;
  totalWeeks: number;
  days: WeekDay[];
}

export interface WeekDay {
  workoutId: string;
  dayNumber: number;
  dayName?: string;
  focus?: string;
  scheduledDate: string;
  status: WorkoutStatus;
  statusName: string;
  isToday: boolean;
  durationMinutes?: number;
  fatigueRating?: number;
  exerciseCount: number;
  completedSets: number;
  totalSets: number;
}

// ==========================================
// Exercise Lift History
// ==========================================

export interface ExerciseLiftHistory {
  exerciseId: string;
  exerciseName: string;
  category?: string;
  primaryMuscleGroup?: string;
  equipment?: string;
  videoUrl?: string;
  imageUrl?: string;
  description?: string;
  isCompound: boolean;
  instructions: string[];
  coachingCues: string[];
  currentEstimated1RM?: number;
  personalRecords?: ExerciseLiftPR;
  entries: LiftEntry[];
}

export interface ExerciseLiftPR {
  maxWeight: number;
  maxWeightDate: string;
  maxReps: number;
  maxRepsDate: string;
  maxEstimated1RM: number;
  maxEstimated1RMDate: string;
  maxVolume: number;
  maxVolumeDate: string;
}

export interface LiftEntry {
  date: string;
  weekNumber: number;
  dayNumber: number;
  maxWeight: number;
  bestReps: number;
  rpe?: number;
  estimated1RM?: number;
  totalVolume: number;
  totalSets: number;
}

// ==========================================
// Query params
// ==========================================

export interface GetWorkoutHistoryParams {
  athleteId: string;
  programId?: string;
  page?: number;
  pageSize?: number;
}
