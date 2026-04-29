import apiClient from './client';
import { API_ENDPOINTS } from './endpoints';
import { SubscriptionListItem, CreateSubscriptionRequest } from '@/types';

export const subscriptionsApi = {
  getByAthlete: async (athleteId: string): Promise<SubscriptionListItem[]> => {
    const response = await apiClient.get(API_ENDPOINTS.subscriptions.byAthlete(athleteId));
    return response.data;
  },

  create: async (data: CreateSubscriptionRequest): Promise<string> => {
    const response = await apiClient.post(API_ENDPOINTS.subscriptions.create, data);
    return response.data;
  },

  cancel: async (id: string, reason?: string): Promise<void> => {
    await apiClient.post(API_ENDPOINTS.subscriptions.cancel(id), { reason });
  },
};
