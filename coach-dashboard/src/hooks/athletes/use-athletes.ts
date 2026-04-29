import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { athletesApi } from '@/lib/api/athletes-api';
import { CreateAthleteRequest, UpdateAthleteRequest } from '@/types';

// Query keys
export const athleteKeys = {
  all: ['athletes'] as const,
  lists: () => [...athleteKeys.all, 'list'] as const,
  list: (params: Record<string, unknown>) => [...athleteKeys.lists(), params] as const,
  details: () => [...athleteKeys.all, 'detail'] as const,
  detail: (id: string) => [...athleteKeys.details(), id] as const,
};

// Hooks
export function useAthletes(params: {
  coachId: string;
  status?: string;
  searchTerm?: string;
  pageNumber?: number;
  pageSize?: number;
}) {
  return useQuery({
    queryKey: athleteKeys.list(params),
    queryFn: () => athletesApi.getAll(params),
    enabled: !!params.coachId,
  });
}

export function useAthlete(id: string) {
  return useQuery({
    queryKey: athleteKeys.detail(id),
    queryFn: () => athletesApi.getById(id),
    enabled: !!id,
  });
}

export function useCreateAthlete() {
  const queryClient = useQueryClient();
  
  return useMutation({
    mutationFn: (data: CreateAthleteRequest) => athletesApi.create(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: athleteKeys.lists() });
    },
  });
}

export function useUpdateAthlete() {
  const queryClient = useQueryClient();
  
  return useMutation({
    mutationFn: ({ id, data }: { id: string; data: UpdateAthleteRequest }) => 
      athletesApi.update(id, data),
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({ queryKey: athleteKeys.lists() });
      queryClient.invalidateQueries({ queryKey: athleteKeys.detail(variables.id) });
    },
  });
}

export function useDeactivateAthlete() {
  const queryClient = useQueryClient();
  
  return useMutation({
    mutationFn: (id: string) => athletesApi.deactivate(id),
    onSuccess: (_, id) => {
      queryClient.invalidateQueries({ queryKey: athleteKeys.lists() });
      queryClient.invalidateQueries({ queryKey: athleteKeys.detail(id) });
    },
  });
}

export function useMaxLifts(athleteId: string, exerciseId?: string) {
  return useQuery({
    queryKey: [...athleteKeys.detail(athleteId), 'max-lifts', exerciseId],
    queryFn: () => athletesApi.getMaxLifts(athleteId, exerciseId),
    enabled: !!athleteId,
  });
}

export function useExerciseHistory(athleteId: string, exerciseId: string) {
  return useQuery({
    queryKey: [...athleteKeys.detail(athleteId), 'exercise-history', exerciseId],
    queryFn: () => athletesApi.getExerciseHistory(athleteId, exerciseId),
    enabled: !!athleteId && !!exerciseId,
  });
}
