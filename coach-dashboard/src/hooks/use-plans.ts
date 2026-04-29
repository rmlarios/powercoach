import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { plansApi, GetPlansParams } from '@/lib/api/plans-api';
import { CreatePlanRequest, UpdatePlanRequest } from '@/types';

export const planKeys = {
  all: ['plans'] as const,
  lists: () => [...planKeys.all, 'list'] as const,
  list: (params: Record<string, unknown>) => [...planKeys.lists(), params] as const,
};

export function usePlans(params: GetPlansParams) {
  return useQuery({
    queryKey: planKeys.list(params as unknown as Record<string, unknown>),
    queryFn: () => plansApi.getAll(params),
    enabled: !!params.coachId,
  });
}

export function useCreatePlan() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (data: CreatePlanRequest) => plansApi.create(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: planKeys.lists() });
    },
  });
}

export function useUpdatePlan() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ id, data }: { id: string; data: UpdatePlanRequest }) =>
      plansApi.update(id, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: planKeys.lists() });
    },
  });
}

export function useDeactivatePlan() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (id: string) => plansApi.deactivate(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: planKeys.lists() });
    },
  });
}
