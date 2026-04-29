import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { paymentsApi, GetPaymentsParams } from '@/lib/api/payments-api';
import { CreatePaymentRequest } from '@/types';

export const paymentKeys = {
  all: ['payments'] as const,
  lists: () => [...paymentKeys.all, 'list'] as const,
  list: (params: Record<string, unknown>) => [...paymentKeys.lists(), params] as const,
  byAthlete: (athleteId: string) => [...paymentKeys.all, 'athlete', athleteId] as const,
};

export function usePayments(params?: GetPaymentsParams) {
  return useQuery({
    queryKey: paymentKeys.list((params ?? {}) as Record<string, unknown>),
    queryFn: () => paymentsApi.getAll(params),
  });
}

export function useAthletePayments(athleteId: string) {
  return useQuery({
    queryKey: paymentKeys.byAthlete(athleteId),
    queryFn: () => paymentsApi.getByAthlete(athleteId),
    enabled: !!athleteId,
  });
}

export function useRegisterPayment() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (data: CreatePaymentRequest) => paymentsApi.create(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: paymentKeys.all });
    },
  });
}
