// Plan types — aligned with backend PlanDtos

export type PlanType = 'Monthly' | 'Quarterly' | 'SemiAnnual' | 'Annual' | 'Custom';

/** Lightweight list item (matches backend PlanListItemDto) */
export interface PlanListItem {
  id: string;
  name: string;
  price: number;
  currency: string;
  planType: PlanType;
  isActive: boolean;
  displayOrder: number;
}

/** Full detail (matches backend PlanDto) */
export interface Plan {
  id: string;
  coachId: string;
  name: string;
  description?: string;
  price: number;
  currency: string;
  durationDays: number;
  planType: PlanType;
  features: string[];
  maxAthletes?: number;
  isActive: boolean;
  displayOrder: number;
  createdAt: string;
  updatedAt: string;
}

export interface CreatePlanRequest {
  coachId: string;
  name: string;
  description?: string;
  price: number;
  currency: string;
  durationInMonths: number;
}

export interface UpdatePlanRequest {
  name: string;
  description?: string;
  price: number;
  currency: string;
  durationDays: number;
  features?: string[];
  maxAthletes?: number;
}
