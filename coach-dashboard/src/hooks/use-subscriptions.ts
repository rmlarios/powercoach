import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { subscriptionsApi } from '@/lib/api/subscriptions-api';
import { CreateSubscriptionRequest } from '@/types';

export const subscriptionKeys = {
  all: ['subscriptions'] as const,
  byAthlete: (athleteId: string) => [...subscriptionKeys.all, 'athlete', athleteId] as const,
};

export function useAthleteSubscriptions(athleteId: string) {
  return useQuery({
    queryKey: subscriptionKeys.byAthlete(athleteId),
    queryFn: () => subscriptionsApi.getByAthlete(athleteId),
    enabled: !!athleteId,
  });
}

export function useCreateSubscription() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (data: CreateSubscriptionRequest) => subscriptionsApi.create(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: subscriptionKeys.all });
    },
  });
}

export function useCancelSubscription() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ id, reason }: { id: string; reason?: string }) =>
      subscriptionsApi.cancel(id, reason),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: subscriptionKeys.all });
    },
  });
}
