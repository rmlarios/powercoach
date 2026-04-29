// Subscription types — aligned with backend SubscriptionDtos

export type SubscriptionStatus = 'Active' | 'Paused' | 'Cancelled' | 'Expired' | 'PendingPayment';

/** Full detail (matches backend SubscriptionDto) */
export interface Subscription {
  id: string;
  athleteId: string;
  planId: string;
  planName: string;
  startDate: string;
  endDate: string;
  status: SubscriptionStatus;
  price: number;
  currency: string;
  autoRenew: boolean;
  cancelledAt?: string;
  cancellationReason?: string;
  notes?: string;
  remainingDays: number;
  createdAt: string;
  updatedAt: string;
}

/** Lightweight list item (matches backend SubscriptionListItemDto) */
export interface SubscriptionListItem {
  id: string;
  athleteName: string;
  planName: string;
  status: SubscriptionStatus;
  startDate: string;
  endDate: string;
  remainingDays: number;
}

export interface CreateSubscriptionRequest {
  athleteId: string;
  planId: string;
  startDate: string;
}

export interface CancelSubscriptionRequest {
  reason?: string;
}
