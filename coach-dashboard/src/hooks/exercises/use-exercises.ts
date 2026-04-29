import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { exercisesApi } from '@/lib/api/exercises-api';
import { CreateExerciseRequest, UpdateExerciseRequest } from '@/types';

// Query keys
export const exerciseKeys = {
  all: ['exercises'] as const,
  lists: () => [...exerciseKeys.all, 'list'] as const,
  list: (params?: Record<string, unknown>) => 
    [...exerciseKeys.lists(), params] as const,
  details: () => [...exerciseKeys.all, 'detail'] as const,
  detail: (exerciseId: string) => 
    [...exerciseKeys.details(), exerciseId] as const,
};

// Hooks
export function useExercises(params?: {
  category?: string;
  muscleGroup?: string;
  isActive?: boolean;
}) {
  return useQuery({
    queryKey: exerciseKeys.list(params),
    queryFn: () => exercisesApi.getAll(params),
  });
}

export function useExercise(exerciseId: string) {
  return useQuery({
    queryKey: exerciseKeys.detail(exerciseId),
    queryFn: () => exercisesApi.getById(exerciseId),
    enabled: !!exerciseId,
  });
}

export function useCreateExercise() {
  const queryClient = useQueryClient();
  
  return useMutation({
    mutationFn: (data: CreateExerciseRequest) => 
      exercisesApi.create(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: exerciseKeys.lists() });
    },
  });
}

export function useUpdateExercise() {
  const queryClient = useQueryClient();
  
  return useMutation({
    mutationFn: ({ exerciseId, data }: { exerciseId: string; data: UpdateExerciseRequest }) => 
      exercisesApi.update(exerciseId, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: exerciseKeys.lists() });
    },
  });
}

export function useDeleteExercise() {
  const queryClient = useQueryClient();
  
  return useMutation({
    mutationFn: (exerciseId: string) => 
      exercisesApi.delete(exerciseId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: exerciseKeys.lists() });
    },
  });
}
