import apiClient from './client';
import { API_ENDPOINTS } from './endpoints';
import { PaymentListItem, CreatePaymentRequest } from '@/types';

export interface GetPaymentsParams {
  athleteId?: string;
  fromDate?: string;
  toDate?: string;
  paymentMethod?: string;
}

export const paymentsApi = {
  getAll: async (params?: GetPaymentsParams): Promise<PaymentListItem[]> => {
    const response = await apiClient.get(API_ENDPOINTS.payments.list, { params });
    return response.data;
  },

  getByAthlete: async (athleteId: string): Promise<PaymentListItem[]> => {
    const response = await apiClient.get(API_ENDPOINTS.payments.byAthlete(athleteId));
    return response.data;
  },

  create: async (data: CreatePaymentRequest): Promise<string> => {
    const response = await apiClient.post(API_ENDPOINTS.payments.create, data);
    return response.data;
  },
};
