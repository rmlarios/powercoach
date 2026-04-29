import apiClient from './client';
import { API_ENDPOINTS } from './endpoints';
import { CoachDashboard } from '@/types';

export const dashboardApi = {
  getCoachDashboard: async (coachId: string): Promise<CoachDashboard> => {
    const response = await apiClient.get(API_ENDPOINTS.dashboard.coach, {
      params: { coachId },
    });
    return response.data;
  },
};
