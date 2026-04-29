import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { workoutTrackingApi } from '@/lib/api/workout-tracking-api';
import {
  SaveSetRequest,
  UpdateSetRequest,
  SkipWorkoutRequest,
  CompleteWorkoutRequest,
  GetWorkoutHistoryParams,
} from '@/types';
import { athleteProgramKeys } from '../programs/use-programs';

// ========================
// Query Keys
// ========================

export const workoutTrackingKeys = {
  all: ['workout-tracking'] as const,
  today: (athleteId: string) => [...workoutTrackingKeys.all, 'today', athleteId] as const,
  detail: (athleteId: string, workoutId: string) =>
    [...workoutTrackingKeys.all, 'detail', athleteId, workoutId] as const,
  histories: () => [...workoutTrackingKeys.all, 'history'] as const,
  history: (params: Record<string, unknown>) =>
    [...workoutTrackingKeys.histories(), params] as const,
};

// ========================
// Query Hooks
// ========================

/** Fetch today's workout (or next upcoming) for an athlete. */
export function useTodayWorkout(athleteId: string) {
  return useQuery({
    queryKey: workoutTrackingKeys.today(athleteId),
    queryFn: () => workoutTrackingApi.getToday(athleteId),
    enabled: !!athleteId,
  });
}

/** Fetch a specific workout with all exercises and sets. */
export function useWorkoutDetail(athleteId: string, workoutId: string) {
  return useQuery({
    queryKey: workoutTrackingKeys.detail(athleteId, workoutId),
    queryFn: () => workoutTrackingApi.getWorkout(athleteId, workoutId),
    enabled: !!athleteId && !!workoutId,
  });
}

/** Fetch paginated workout history. */
export function useWorkoutHistory(params: GetWorkoutHistoryParams) {
  return useQuery({
    queryKey: workoutTrackingKeys.history(params as unknown as Record<string, unknown>),
    queryFn: () => workoutTrackingApi.getHistory(params),
    enabled: !!params.athleteId,
  });
}

/** Fetch all workouts for a specific week (week-strip navigation). */
export function useWeekWorkouts(athleteId: string, weekNumber?: number) {
  return useQuery({
    queryKey: [...workoutTrackingKeys.all, 'week', athleteId, weekNumber ?? 'current'] as const,
    queryFn: () => workoutTrackingApi.getWeekWorkouts(athleteId, weekNumber),
    enabled: !!athleteId,
  });
}

/** Fetch lift history for a specific exercise (exercise detail sheet). */
export function useExerciseLiftHistory(athleteId: string, exerciseId: string | null) {
  return useQuery({
    queryKey: [...workoutTrackingKeys.all, 'lift-history', athleteId, exerciseId] as const,
    queryFn: () => workoutTrackingApi.getExerciseLiftHistory(athleteId, exerciseId!),
    enabled: !!athleteId && !!exerciseId,
  });
}

// ========================
// Mutation Hooks
// ========================

/** Start a workout session. */
export function useStartWorkout() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ athleteId, workoutId }: { athleteId: string; workoutId: string }) =>
      workoutTrackingApi.startWorkout(athleteId, workoutId),
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({ queryKey: workoutTrackingKeys.today(variables.athleteId) });
      queryClient.invalidateQueries({
        queryKey: workoutTrackingKeys.detail(variables.athleteId, variables.workoutId),
      });
    },
  });
}

/** Save (upsert) a set — primary auto-save mutation. */
export function useSaveSet() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({
      athleteId,
      workoutId,
      data,
    }: {
      athleteId: string;
      workoutId: string;
      data: SaveSetRequest;
    }) => workoutTrackingApi.saveSet(athleteId, workoutId, data),
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({
        queryKey: workoutTrackingKeys.detail(variables.athleteId, variables.workoutId),
      });
      // Also refresh today view if it's the same workout
      queryClient.invalidateQueries({ queryKey: workoutTrackingKeys.today(variables.athleteId) });
    },
  });
}

/** Update an existing set's actual performance. */
export function useUpdateSet() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({
      athleteId,
      workoutId,
      exerciseLogId,
      data,
    }: {
      athleteId: string;
      workoutId: string;
      exerciseLogId: string;
      data: UpdateSetRequest;
    }) => workoutTrackingApi.updateSet(athleteId, workoutId, exerciseLogId, data),
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({
        queryKey: workoutTrackingKeys.detail(variables.athleteId, variables.workoutId),
      });
    },
  });
}

/** Mark a specific set as completed. */
export function useCompleteSet() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({
      athleteId,
      workoutId,
      exerciseLogId,
    }: {
      athleteId: string;
      workoutId: string;
      exerciseLogId: string;
    }) => workoutTrackingApi.completeSet(athleteId, workoutId, exerciseLogId),
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({
        queryKey: workoutTrackingKeys.detail(variables.athleteId, variables.workoutId),
      });
      queryClient.invalidateQueries({ queryKey: workoutTrackingKeys.today(variables.athleteId) });
    },
  });
}

/** Skip the entire workout. */
export function useSkipWorkout() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({
      athleteId,
      workoutId,
      data,
    }: {
      athleteId: string;
      workoutId: string;
      data?: SkipWorkoutRequest;
    }) => workoutTrackingApi.skipWorkout(athleteId, workoutId, data),
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({ queryKey: workoutTrackingKeys.today(variables.athleteId) });
      queryClient.invalidateQueries({
        queryKey: workoutTrackingKeys.detail(variables.athleteId, variables.workoutId),
      });
      queryClient.invalidateQueries({ queryKey: workoutTrackingKeys.histories() });
      // Invalidate parent program view
      queryClient.invalidateQueries({ queryKey: athleteProgramKeys.detail(variables.athleteId) });
    },
  });
}

/** Complete the entire workout session. */
export function useCompleteWorkout() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({
      athleteId,
      workoutId,
      data,
    }: {
      athleteId: string;
      workoutId: string;
      data?: CompleteWorkoutRequest;
    }) => workoutTrackingApi.completeWorkout(athleteId, workoutId, data),
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({ queryKey: workoutTrackingKeys.today(variables.athleteId) });
      queryClient.invalidateQueries({
        queryKey: workoutTrackingKeys.detail(variables.athleteId, variables.workoutId),
      });
      queryClient.invalidateQueries({ queryKey: workoutTrackingKeys.histories() });
      // Invalidate parent program view
      queryClient.invalidateQueries({ queryKey: athleteProgramKeys.detail(variables.athleteId) });
    },
  });
}
