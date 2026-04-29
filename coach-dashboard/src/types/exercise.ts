// Exercise types — aligned with backend ExerciseDtos + ExerciseListItemDto

export type ExerciseCategory =
  | 'Squat' | 'Bench' | 'Deadlift' | 'OverheadPress'
  | 'Row' | 'Pull' | 'Accessory' | 'Cardio'
  | 'Mobility' | 'Core';

export type MuscleGroup =
  | 'Chest' | 'Back' | 'Shoulders' | 'Biceps' | 'Triceps'
  | 'Forearms' | 'Quads' | 'Hamstrings' | 'Glutes' | 'Calves'
  | 'Core' | 'Traps' | 'FullBody';

/** Full exercise detail (matches backend ExerciseDto) */
export interface Exercise {
  id: string;
  coachId?: string;
  name: string;
  description?: string;
  category: ExerciseCategory;
  primaryMuscleGroup: MuscleGroup;
  secondaryMuscleGroups: MuscleGroup[];
  videoUrl?: string;
  imageUrl?: string;
  equipment?: string;
  isCompound: boolean;
  isActive: boolean;
  createdAt: string;
  updatedAt?: string;
}

/** Lightweight list item (matches backend ExerciseListItemDto) */
export interface ExerciseListItem {
  id: string;
  name: string;
  category: ExerciseCategory;
  primaryMuscleGroup: MuscleGroup;
  equipment?: string;
  isCompound: boolean;
  isActive: boolean;
}

export interface CreateExerciseRequest {
  name: string;
  description?: string;
  category: ExerciseCategory;
  primaryMuscleGroup: MuscleGroup;
  secondaryMuscleGroups?: MuscleGroup[];
  videoUrl?: string;
  equipment?: string;
  isCompound?: boolean;
}

export interface UpdateExerciseRequest {
  name: string;
  description?: string;
  category: ExerciseCategory;
  primaryMuscleGroup: MuscleGroup;
  secondaryMuscleGroups?: MuscleGroup[];
  videoUrl?: string;
  equipment?: string;
  isCompound?: boolean;
}
