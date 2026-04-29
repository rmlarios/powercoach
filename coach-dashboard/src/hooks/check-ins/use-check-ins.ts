import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { apiClient } from '@/lib/api/client';
import { API_ENDPOINTS } from '@/lib/api/endpoints';
import { CheckInDto, PagedResult, ReviewCheckInRequest } from '@/types/checkIn';

export interface UseCoachCheckInsOptions {
  coachId: string;
  isReviewed?: boolean;
  page?: number;
  size?: number;
  enabled?: boolean;
}

export function useCoachCheckIns({ coachId, isReviewed, page = 1, size = 10, enabled = true }: UseCoachCheckInsOptions) {
  return useQuery<PagedResult<CheckInDto>>({
    queryKey: ['coach-checkins', coachId, { isReviewed, page, size }],
    queryFn: async () => {
      const params = new URLSearchParams({
        coachId,
        page: page.toString(),
        size: size.toString(),
      });
      if (isReviewed !== undefined) {
        params.append('isReviewed', isReviewed.toString());
      }
      const response = await apiClient.get<PagedResult<CheckInDto>>(`${API_ENDPOINTS.checkIns.coachList}?${params.toString()}`);
      return response.data;
    },
    enabled: !!coachId && enabled,
  });
}

export function useCheckInById(id: string, coachId: string) {
  return useQuery<CheckInDto>({
    queryKey: ['checkin', id, coachId],
    queryFn: async () => {
      const response = await apiClient.get<CheckInDto>(`${API_ENDPOINTS.checkIns.getById(id)}?coachId=${coachId}`);
      return response.data;
    },
    enabled: !!id && !!coachId,
  });
}

export function useReviewCheckIn() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async ({ checkInId, data }: { checkInId: string; data: ReviewCheckInRequest }) => {
      const response = await apiClient.put(API_ENDPOINTS.checkIns.review(checkInId), data);
      return response.data;
    },
    onSuccess: () => {
      // Invalidate the check-ins query
      queryClient.invalidateQueries({ queryKey: ['coach-checkins'] });
      queryClient.invalidateQueries({ queryKey: ['checkin'] });
    },
  });
}

// ============================================
// Athlete-side hooks
// ============================================

export interface CreateCheckInRequest {
  athleteId: string;
  weight?: number | null;
  weightUnit?: string;
  notes?: string;
  energyLevel?: number | null;
  sleepQuality?: number | null;
  sleepHours?: number | null;
  stressLevel?: number | null;
  nutritionAdherence?: number | null;
  trainingAdherence?: number | null;
}

/** Fetch the athlete's own check-in history */
export function useAthleteCheckIns(athleteId: string, page = 1, size = 10) {
  return useQuery<PagedResult<CheckInDto>>({
    queryKey: ['athlete-checkins', athleteId, { page, size }],
    queryFn: async () => {
      const params = new URLSearchParams({
        page: page.toString(),
        size: size.toString(),
      });
      const response = await apiClient.get<PagedResult<CheckInDto>>(
        `${API_ENDPOINTS.checkIns.athleteList(athleteId)}?${params.toString()}`
      );
      return response.data;
    },
    enabled: !!athleteId,
  });
}

/** Submit a new check-in */
export function useCreateCheckIn() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async (data: CreateCheckInRequest) => {
      const response = await apiClient.post<CheckInDto>(API_ENDPOINTS.checkIns.create, data);
      return response.data;
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['athlete-checkins'] });
    },
  });
}
