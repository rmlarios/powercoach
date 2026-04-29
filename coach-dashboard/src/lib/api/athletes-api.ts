import apiClient from './client';
import { API_ENDPOINTS } from './endpoints';
import { 
  PagedResult, 
  AthleteListItem, 
  Athlete, 
  CreateAthleteRequest, 
  UpdateAthleteRequest,
  AthleteMaxLifts,
  RegisterMaxLiftRequest,
  ExerciseHistory
} from '@/types';

export interface GetAthletesParams {
  coachId: string;
  status?: string;
  searchTerm?: string;
  pageNumber?: number;
  pageSize?: number;
}

export const athletesApi = {
  getAll: async (params: GetAthletesParams): Promise<PagedResult<AthleteListItem>> => {
    const response = await apiClient.get(API_ENDPOINTS.athletes.list, { params });
    return response.data;
  },

  getById: async (id: string): Promise<Athlete> => {
    const response = await apiClient.get(API_ENDPOINTS.athletes.getById(id));
    return response.data;
  },

  create: async (data: CreateAthleteRequest): Promise<string> => {
    const response = await apiClient.post(API_ENDPOINTS.athletes.create, data);
    return response.data;
  },

  update: async (id: string, data: UpdateAthleteRequest): Promise<void> => {
    await apiClient.put(API_ENDPOINTS.athletes.update(id), data);
  },

  deactivate: async (id: string): Promise<void> => {
    await apiClient.delete(API_ENDPOINTS.athletes.deactivate(id));
  },

  // Max Lifts (1RM) methods
  getMaxLifts: async (athleteId: string, exerciseId?: string): Promise<AthleteMaxLifts> => {
    const params = exerciseId ? { exerciseId } : {};
    const response = await apiClient.get(
      API_ENDPOINTS.athletes.maxLifts(athleteId),
      { params }
    );
    return response.data;
  },

  registerMaxLift: async (athleteId: string, data: RegisterMaxLiftRequest): Promise<string> => {
    const response = await apiClient.post(
      API_ENDPOINTS.athletes.maxLifts(athleteId),
      data
    );
    return response.data;
  },

  // Exercise History methods
  getExerciseHistory: async (athleteId: string, exerciseId: string): Promise<ExerciseHistory> => {
    const response = await apiClient.get(
      API_ENDPOINTS.athletes.exerciseHistory(athleteId, exerciseId)
    );
    return response.data;
  },
};

// Named exports for convenience
export const getAthletes = athletesApi.getAll;
export const getAthleteById = athletesApi.getById;
export const createAthlete = athletesApi.create;
export const updateAthlete = athletesApi.update;
export const deactivateAthlete = athletesApi.deactivate;
