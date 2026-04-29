import apiClient from './client';
import { API_ENDPOINTS } from './endpoints';
import { PlanListItem, CreatePlanRequest, UpdatePlanRequest } from '@/types';

export interface GetPlansParams {
  coachId: string;
  isActive?: boolean;
}

export const plansApi = {
  getAll: async (params: GetPlansParams): Promise<PlanListItem[]> => {
    const response = await apiClient.get(API_ENDPOINTS.plans.list, { params });
    return response.data;
  },

  create: async (data: CreatePlanRequest): Promise<string> => {
    const response = await apiClient.post(API_ENDPOINTS.plans.create, data);
    return response.data;
  },

  update: async (id: string, data: UpdatePlanRequest): Promise<void> => {
    await apiClient.put(API_ENDPOINTS.plans.update(id), data);
  },

  deactivate: async (id: string): Promise<void> => {
    await apiClient.post(API_ENDPOINTS.plans.deactivate(id));
  },
};
