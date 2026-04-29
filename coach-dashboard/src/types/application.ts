// Application types — aligned with backend ApplicationDtos

export type ApplicationStatus = 'Pending' | 'UnderReview' | 'Accepted' | 'Rejected' | 'Withdrawn';

/** Lightweight list item (matches backend ApplicationListItemDto) */
export interface ApplicationListItem {
  id: string;
  fullName: string;
  email: string;
  country?: string;
  totalLifts?: number;
  status: ApplicationStatus;
  createdAt: string;
}

/** Full detail (matches backend ApplicationDto) */
export interface Application {
  id: string;
  coachId: string;
  firstName: string;
  lastName: string;
  fullName: string;
  email: string;
  phone?: string;
  age?: number;
  gender?: string;
  country?: string;
  trainingExperience?: string;
  currentSquat?: number;
  currentBench?: number;
  currentDeadlift?: number;
  totalLifts?: number;
  motivation?: string;
  goals?: string;
  message?: string;
  referralSource?: string;
  status: ApplicationStatus;
  coachNotes?: string;
  reviewedAt?: string;
  rejectionReason?: string;
  createdAt: string;
  updatedAt: string;
}

export interface CreateApplicationRequest {
  coachId: string;
  firstName: string;
  lastName: string;
  email: string;
  phone?: string;
  age?: number;
  gender?: string;
  country?: string;
  trainingExperience?: string;
  currentSquat?: number;
  currentBench?: number;
  currentDeadlift?: number;
  motivation?: string;
  goals?: string;
  message?: string;
  referralSource?: string;
}
