// API module index
// Re-exports all API services for convenient imports

export { default as apiClient } from './client';
export { API_ENDPOINTS } from './endpoints';

// Auth API
export { authApi } from './auth-api';
export type { LoginRequest, LoginResponse, UserDto, CreateUserRequest, UpdateUserRequest, ResetPasswordRequest } from './auth-api';

// Re-export types from the types module
export * from '@/types';

// Applications API
export { 
  applicationsApi,
  getApplications,
  getApplicationById,
  createApplication,
  approveApplication,
  rejectApplication,
} from './applications-api';

// Athletes API
export {
  athletesApi,
  getAthletes,
  getAthleteById,
  createAthlete,
  updateAthlete,
  deactivateAthlete,
} from './athletes-api';

// Exercises API
export {
  exercisesApi,
  getExercises,
  getExerciseById,
  createExercise,
} from './exercises-api';

// Programs API
export {
  programsApi,
  athleteProgramsApi,
  getPrograms,
  getProgramById,
  createProgram,
  addProgramWeek,
  addProgramDay,
  addProgramExercise,
  getAthleteProgram,
  assignProgramToAthlete,
  getAthleteWorkout,
  logAthleteWorkout,
} from './programs-api';

// Workout Tracking API (F-015)
export {
  workoutTrackingApi,
  getTodayWorkout,
  getWorkoutDetail,
  getWorkoutHistory,
  startWorkout,
  saveWorkoutSet,
  updateWorkoutSet,
  completeWorkoutSet,
  skipWorkout,
  completeWorkout,
} from './workout-tracking-api';

// Legacy exports for backward compatibility
// These allow existing code using services.ts to work during migration
export { applicationsApi as applicationsService } from './applications-api';
export { athletesApi as athletesService } from './athletes-api';
export { exercisesApi as exercisesService } from './exercises-api';
export { programsApi as programsService } from './programs-api';
export { athleteProgramsApi as athleteProgramsService } from './programs-api';
