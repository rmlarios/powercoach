import apiClient from './client';
import { API_ENDPOINTS } from './endpoints';
import { Exercise, ExerciseListItem, CreateExerciseRequest, UpdateExerciseRequest } from '@/types';

export interface GetExercisesParams {
  category?: string;
  muscleGroup?: string;
  isActive?: boolean;
}

export const exercisesApi = {
  getAll: async (params?: GetExercisesParams): Promise<ExerciseListItem[]> => {
    const response = await apiClient.get(API_ENDPOINTS.exercises.list, { params });
    return response.data;
  },

  getById: async (exerciseId: string): Promise<Exercise> => {
    const response = await apiClient.get(API_ENDPOINTS.exercises.getById(exerciseId));
    return response.data;
  },

  create: async (data: CreateExerciseRequest): Promise<string> => {
    const response = await apiClient.post(API_ENDPOINTS.exercises.create, data);
    return response.data;
  },

  update: async (exerciseId: string, data: UpdateExerciseRequest): Promise<void> => {
    await apiClient.put(`${API_ENDPOINTS.exercises.list}/${exerciseId}`, data);
  },

  delete: async (exerciseId: string): Promise<void> => {
    await apiClient.delete(`${API_ENDPOINTS.exercises.list}/${exerciseId}`);
  },
};

// Named exports for convenience
export const getExercises = exercisesApi.getAll;
export const getExerciseById = exercisesApi.getById;
export const createExercise = exercisesApi.create;
export const updateExercise = exercisesApi.update;
export const deleteExercise = exercisesApi.delete;
