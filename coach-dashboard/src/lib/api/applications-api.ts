import apiClient from './client';
import { API_ENDPOINTS } from './endpoints';
import { 
  PagedResult, 
  ApplicationListItem, 
  Application, 
  CreateApplicationRequest 
} from '@/types';

export interface GetApplicationsParams {
  coachId: string;
  status?: string;
  searchTerm?: string;
  pageNumber?: number;
  pageSize?: number;
}

export const applicationsApi = {
  getAll: async (params: GetApplicationsParams): Promise<PagedResult<ApplicationListItem>> => {
    const response = await apiClient.get(API_ENDPOINTS.applications.list, { params });
    return response.data;
  },

  getById: async (id: string): Promise<Application> => {
    const response = await apiClient.get(API_ENDPOINTS.applications.getById(id));
    return response.data;
  },

  create: async (data: CreateApplicationRequest): Promise<string> => {
    const response = await apiClient.post(API_ENDPOINTS.applications.create, data);
    return response.data;
  },

  approve: async (id: string, notes?: string): Promise<void> => {
    await apiClient.post(API_ENDPOINTS.applications.approve(id), { notes });
  },

  reject: async (id: string, reason: string): Promise<void> => {
    await apiClient.post(API_ENDPOINTS.applications.reject(id), { reason });
  },
};

// Named exports for convenience
export const getApplications = applicationsApi.getAll;
export const getApplicationById = applicationsApi.getById;
export const createApplication = applicationsApi.create;
export const approveApplication = applicationsApi.approve;
export const rejectApplication = applicationsApi.reject;
