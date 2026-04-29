import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { exercisesService } from '@/lib/api/services';
import { CreateExerciseRequest } from '@/lib/api/types';

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
    queryFn: () => exercisesService.getAll(params),
  });
}

export function useExercise(exerciseId: string) {
  return useQuery({
    queryKey: exerciseKeys.detail(exerciseId),
    queryFn: () => exercisesService.getById(exerciseId),
    enabled: !!exerciseId,
  });
}

export function useCreateExercise() {
  const queryClient = useQueryClient();
  
  return useMutation({
    mutationFn: (data: CreateExerciseRequest) => 
      exercisesService.create(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: exerciseKeys.lists() });
    },
  });
}
