import { apiClient } from './client';
import { API_ENDPOINTS } from './endpoints';

export interface LoginRequest {
  email: string;
  password: string;
}

export interface UserDto {
  id: string;
  email: string;
  username: string;
  role: 'Admin' | 'Coach' | 'Athlete';
  coachId: string | null;
  athleteId: string | null;
  isActive: boolean;
}

export interface LoginResponse {
  accessToken: string;
  refreshToken: string;
  user: UserDto;
}

export interface CreateUserRequest {
  email: string;
  username: string;
  password: string;
  role: 'Coach' | 'Athlete' | 'Admin';
  coachId?: string;
  athleteId?: string;
}

export interface UpdateUserRequest {
  username?: string;
  email?: string;
  isActive?: boolean;
}

export interface ResetPasswordRequest {
  newPassword: string;
}

export const authApi = {
  login: (data: LoginRequest) =>
    apiClient.post<LoginResponse>(API_ENDPOINTS.auth.login, data).then((r) => r.data),

  refresh: () =>
    apiClient.post<LoginResponse>(API_ENDPOINTS.auth.refresh).then((r) => r.data),

  logout: () =>
    apiClient.post(API_ENDPOINTS.auth.logout).then((r) => r.data),

  me: () =>
    apiClient.get<UserDto>(API_ENDPOINTS.auth.me).then((r) => r.data),

  getUsers: () =>
    apiClient.get<UserDto[]>(API_ENDPOINTS.auth.users).then((r) => r.data),

  createUser: (data: CreateUserRequest) =>
    apiClient.post<UserDto>(API_ENDPOINTS.auth.users, data).then((r) => r.data),

  updateUser: (id: string, data: UpdateUserRequest) =>
    apiClient.put<UserDto>(API_ENDPOINTS.auth.userById(id), data).then((r) => r.data),

  resetPassword: (id: string, data: ResetPasswordRequest) =>
    apiClient.post(API_ENDPOINTS.auth.resetPassword(id), data).then((r) => r.data),
};
