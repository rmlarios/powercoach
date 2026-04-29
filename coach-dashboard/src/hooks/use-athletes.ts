import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { athletesService } from '@/lib/api/services';
import { CreateAthleteRequest, UpdateAthleteRequest, RegisterMaxLiftRequest } from '@/lib/api/types';

// Query keys
export const athleteKeys = {
  all: ['athletes'] as const,
  lists: () => [...athleteKeys.all, 'list'] as const,
  list: (params: Record<string, unknown>) => [...athleteKeys.lists(), params] as const,
  details: () => [...athleteKeys.all, 'detail'] as const,
  detail: (id: string) => [...athleteKeys.details(), id] as const,
  maxLifts: (athleteId: string) => [...athleteKeys.all, 'maxLifts', athleteId] as const,
  maxLift: (athleteId: string, exerciseId: string) => [...athleteKeys.maxLifts(athleteId), exerciseId] as const,
  exerciseHistory: (athleteId: string, exerciseId: string) => 
    [...athleteKeys.all, 'exerciseHistory', athleteId, exerciseId] as const,
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
    queryFn: () => athletesService.getAll(params),
    enabled: !!params.coachId,
  });
}

export function useAthlete(id: string) {
  return useQuery({
    queryKey: athleteKeys.detail(id),
    queryFn: () => athletesService.getById(id),
    enabled: !!id,
  });
}

export function useCreateAthlete() {
  const queryClient = useQueryClient();
  
  return useMutation({
    mutationFn: (data: CreateAthleteRequest) => athletesService.create(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: athleteKeys.lists() });
    },
  });
}

export function useUpdateAthlete() {
  const queryClient = useQueryClient();
  
  return useMutation({
    mutationFn: ({ id, data }: { id: string; data: UpdateAthleteRequest }) => 
      athletesService.update(id, data),
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({ queryKey: athleteKeys.lists() });
      queryClient.invalidateQueries({ queryKey: athleteKeys.detail(variables.id) });
    },
  });
}

export function useDeactivateAthlete() {
  const queryClient = useQueryClient();
  
  return useMutation({
    mutationFn: (id: string) => athletesService.deactivate(id),
    onSuccess: (_, id) => {
      queryClient.invalidateQueries({ queryKey: athleteKeys.lists() });
      queryClient.invalidateQueries({ queryKey: athleteKeys.detail(id) });
    },
  });
}

// ==========================================
// Max Lifts (1RM) hooks
// ==========================================

/**
 * Fetch all max lifts for an athlete
 */
export function useAthleteMaxLifts(athleteId: string, exerciseId?: string) {
  return useQuery({
    queryKey: exerciseId 
      ? athleteKeys.maxLift(athleteId, exerciseId)
      : athleteKeys.maxLifts(athleteId),
    queryFn: () => athletesService.getMaxLifts(athleteId, exerciseId),
    enabled: !!athleteId,
  });
}

/**
 * Register a new max lift for an athlete
 */
export function useRegisterMaxLift() {
  const queryClient = useQueryClient();
  
  return useMutation({
    mutationFn: ({ athleteId, data }: { athleteId: string; data: RegisterMaxLiftRequest }) =>
      athletesService.registerMaxLift(athleteId, data),
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({ queryKey: athleteKeys.maxLifts(variables.athleteId) });
    },
  });
}

// ==========================================
// Exercise History hooks
// ==========================================

/**
 * Fetch exercise history for an athlete
 * Returns 1RM, PR, last programmed, trend, and suggestions
 */
export function useExerciseHistory(athleteId: string | null, exerciseId: string | null) {
  return useQuery({
    queryKey: athleteId && exerciseId 
      ? athleteKeys.exerciseHistory(athleteId, exerciseId)
      : ['exerciseHistory', 'disabled'],
    queryFn: () => athletesService.getExerciseHistory(athleteId!, exerciseId!),
    enabled: !!athleteId && !!exerciseId,
    staleTime: 1000 * 60 * 5, // 5 minutes - history doesn't change often
  });
}
