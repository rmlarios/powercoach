import { useQuery } from '@tanstack/react-query';
import { dashboardApi } from '@/lib/api/dashboard-api';
import { CoachDashboard } from '@/types';

export const dashboardKeys = {
  all: ['dashboard'] as const,
  coach: (coachId: string) => [...dashboardKeys.all, 'coach', coachId] as const,
};

export function useCoachDashboard(coachId: string) {
  return useQuery<CoachDashboard>({
    queryKey: dashboardKeys.coach(coachId),
    queryFn: () => dashboardApi.getCoachDashboard(coachId),
    enabled: !!coachId,
    staleTime: 30_000,
    refetchInterval: 60_000,
  });
}
