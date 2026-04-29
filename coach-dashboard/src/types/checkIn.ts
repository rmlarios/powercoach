export interface CheckInDto {
  id: string;
  athleteId: string;
  athleteName: string;
  checkInDate: string;
  weight: number | null;
  weightUnit: string | null;
  notes: string | null;
  photoUrls: string[];
  coachFeedback: string | null;
  reviewedAt: string | null;
  createdAt: string;
  updatedAt: string;
  energyLevel: number | null;
  sleepQuality: number | null;
  sleepHours: number | null;
  stressLevel: number | null;
  nutritionAdherence: number | null;
  trainingAdherence: number | null;
}

export interface PagedResult<T> {
  items: T[];
  pageNumber: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
}

export interface ReviewCheckInRequest {
  coachId: string;
  feedback: string;
}
