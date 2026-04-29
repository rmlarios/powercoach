// Training Program types

export type ProgramStatus = 'Active' | 'Completed' | 'Paused' | 'Cancelled';
export type DayFocus = 'Squat' | 'Bench' | 'Deadlift' | 'Hypertrophy' | 'Push' | 'Pull' | 'Legs' | 'UpperBody' | 'LowerBody' | 'FullBody' | 'Accessories' | 'Recovery' | 'Rest' | 'Cardio' | 'Custom';

export interface ProgramTemplateListItem {
  id: string;
  name: string;
  description?: string;
  durationWeeks: number;
  isActive: boolean;
  createdAt: string;
}

export interface ProgramTemplate {
  id: string;
  coachId: string;
  name: string;
  description?: string;
  durationWeeks: number;
  isActive: boolean;
  weeks: ProgramWeek[];
  createdAt: string;
  updatedAt?: string;
}

export interface ProgramWeek {
  id: string;
  weekNumber: number;
  name: string;
  notes?: string;
  days: ProgramDay[];
}

export interface ProgramDay {
  id: string;
  dayNumber: number;
  name: string;
  focus: DayFocus | null;
  notes?: string;
  exercises: ProgramExercise[];
}

export interface ProgramExercise {
  id: string;
  exerciseId: string;
  exerciseName: string;
  order: number;
  sets: number;
  reps: string;
  restSeconds?: number;
  targetRpe?: number;
  notes?: string;
  exerciseType?: string;
  percentageRM?: number;
  rawNotation?: string;
  weight?: number;
  emomConfigJson?: string;
  tempoConfigJson?: string;
  supersetConfigJson?: string;
}

export interface CreateProgramTemplateRequest {
  coachId: string;
  name: string;
  description?: string;
  durationWeeks: number;
  daysPerWeek?: number;
}

// Athlete Program types
export interface AthleteProgram {
  id: string;
  athleteId: string;
  programTemplateId: string;
  programName: string;
  status: ProgramStatus;
  startDate: string;
  endDate?: string;
  currentWeek: number;
  currentDay: number;
  workouts: AthleteWorkoutSummary[];
}

export interface AthleteWorkoutSummary {
  id: string;
  weekNumber: number;
  dayNumber: number;
  scheduledDate: string;
  completedDate?: string;
  isCompleted: boolean;
}

export interface AthleteWorkout {
  id: string;
  athleteProgramId: string;
  weekNumber: number;
  dayNumber: number;
  dayName: string;
  focus: DayFocus | null;
  scheduledDate: string;
  completedDate?: string;
  isCompleted: boolean;
  exercises: AthleteWorkoutExercise[];
}

export interface AthleteWorkoutExercise {
  id: string;
  exerciseId: string;
  exerciseName: string;
  order: number;
  targetSets: number;
  targetRepsMin: number;
  targetRepsMax: number;
  restSeconds: number;
  rpeTarget?: number;
  notes?: string;
  logs: ExerciseLog[];
}

export interface ExerciseLog {
  setNumber: number;
  weight: number;
  reps: number;
  rpe?: number;
  notes?: string;
}

export interface LogWorkoutRequest {
  exerciseLogs: {
    exerciseId: string;
    logs: ExerciseLog[];
  }[];
}
