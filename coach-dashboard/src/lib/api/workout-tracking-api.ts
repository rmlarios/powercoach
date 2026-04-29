import apiClient from './client';
import { API_ENDPOINTS } from './endpoints';
import {
  PagedResult,
  TodayWorkout,
  WorkoutHistoryItem,
  WeekWorkouts,
  ExerciseLiftHistory,
  SaveSetRequest,
  UpdateSetRequest,
  SkipWorkoutRequest,
  CompleteWorkoutRequest,
  GetWorkoutHistoryParams,
} from '@/types';

// ── Workout Tracking API (F-015) ──

export const workoutTrackingApi = {
  /** Get today's workout (or next upcoming) for an athlete. Returns null on 204. */
  getToday: async (athleteId: string): Promise<TodayWorkout | null> => {
    const response = await apiClient.get(
      API_ENDPOINTS.workoutTracking.today(athleteId),
      { validateStatus: (s) => s === 200 || s === 204 }
    );
    return response.status === 204 ? null : response.data;
  },

  /** Get a specific workout with all exercises and sets. */
  getWorkout: async (athleteId: string, workoutId: string): Promise<TodayWorkout> => {
    const response = await apiClient.get(
      API_ENDPOINTS.workoutTracking.getById(athleteId, workoutId)
    );
    return response.data;
  },

  /** Get workout history (paginated). */
  getHistory: async (params: GetWorkoutHistoryParams): Promise<PagedResult<WorkoutHistoryItem>> => {
    const { athleteId, ...queryParams } = params;
    const response = await apiClient.get(
      API_ENDPOINTS.workoutTracking.history(athleteId),
      { params: queryParams }
    );
    return response.data;
  },

  /** Get all workouts for a week (week-strip navigation). */
  getWeekWorkouts: async (athleteId: string, weekNumber?: number): Promise<WeekWorkouts | null> => {
    const response = await apiClient.get(
      API_ENDPOINTS.workoutTracking.week(athleteId),
      {
        params: weekNumber != null ? { weekNumber } : undefined,
        validateStatus: (s) => s === 200 || s === 204,
      }
    );
    return response.status === 204 ? null : response.data;
  },

  /** Get lift history for a specific exercise (exercise detail sheet). */
  getExerciseLiftHistory: async (athleteId: string, exerciseId: string, limit = 20): Promise<ExerciseLiftHistory | null> => {
    const response = await apiClient.get(
      API_ENDPOINTS.workoutTracking.exerciseLiftHistory(athleteId, exerciseId),
      {
        params: { limit },
        validateStatus: (s) => s === 200 || s === 404,
      }
    );
    return response.status === 404 ? null : response.data;
  },

  /** Start a workout session. */
  startWorkout: async (athleteId: string, workoutId: string): Promise<void> => {
    await apiClient.post(API_ENDPOINTS.workoutTracking.start(athleteId, workoutId));
  },

  /** Save (create or update) a set — primary auto-save endpoint. */
  saveSet: async (athleteId: string, workoutId: string, data: SaveSetRequest): Promise<{ id: string }> => {
    const response = await apiClient.put(
      API_ENDPOINTS.workoutTracking.saveSet(athleteId, workoutId),
      data
    );
    return response.data;
  },

  /** Update an existing set's actual performance. */
  updateSet: async (athleteId: string, workoutId: string, exerciseLogId: string, data: UpdateSetRequest): Promise<void> => {
    await apiClient.put(
      API_ENDPOINTS.workoutTracking.updateSet(athleteId, workoutId, exerciseLogId),
      data
    );
  },

  /** Mark a specific set as completed. */
  completeSet: async (athleteId: string, workoutId: string, exerciseLogId: string): Promise<void> => {
    await apiClient.post(
      API_ENDPOINTS.workoutTracking.completeSet(athleteId, workoutId, exerciseLogId)
    );
  },

  /** Skip the entire workout with an optional reason. */
  skipWorkout: async (athleteId: string, workoutId: string, data?: SkipWorkoutRequest): Promise<void> => {
    await apiClient.post(
      API_ENDPOINTS.workoutTracking.skip(athleteId, workoutId),
      data ?? {}
    );
  },

  /** Complete the entire workout session. */
  completeWorkout: async (athleteId: string, workoutId: string, data?: CompleteWorkoutRequest): Promise<void> => {
    await apiClient.post(
      API_ENDPOINTS.workoutTracking.complete(athleteId, workoutId),
      data ?? {}
    );
  },
};

// Named exports for convenience
export const getTodayWorkout = workoutTrackingApi.getToday;
export const getWorkoutDetail = workoutTrackingApi.getWorkout;
export const getWorkoutHistory = workoutTrackingApi.getHistory;
export const getWeekWorkouts = workoutTrackingApi.getWeekWorkouts;
export const getExerciseLiftHistory = workoutTrackingApi.getExerciseLiftHistory;
export const startWorkout = workoutTrackingApi.startWorkout;
export const saveWorkoutSet = workoutTrackingApi.saveSet;
export const updateWorkoutSet = workoutTrackingApi.updateSet;
export const completeWorkoutSet = workoutTrackingApi.completeSet;
export const skipWorkout = workoutTrackingApi.skipWorkout;
export const completeWorkout = workoutTrackingApi.completeWorkout;
