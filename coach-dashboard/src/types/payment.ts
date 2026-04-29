// Payment types — aligned with backend PaymentDtos

export type PaymentStatus = 'Pending' | 'Completed' | 'Failed' | 'Refunded' | 'Cancelled';

/** Full detail (matches backend PaymentDto) */
export interface Payment {
  id: string;
  athleteId: string;
  athleteName: string;
  subscriptionId?: string;
  amount: number;
  currency: string;
  status: PaymentStatus;
  paymentDate: string;
  processedAt?: string;
  paymentMethod?: string;
  transactionReference?: string;
  failureReason?: string;
  notes?: string;
  createdAt: string;
  updatedAt: string;
}

/** Lightweight list item (matches backend PaymentListItemDto) */
export interface PaymentListItem {
  id: string;
  athleteName: string;
  amount: number;
  currency: string;
  status: PaymentStatus;
  paymentDate: string;
  paymentMethod?: string;
}

export interface CreatePaymentRequest {
  athleteId: string;
  subscriptionId: string;
  amount: number;
  currency: string;
  paymentMethod?: string;
  paymentDate: string;
  notes?: string;
}

export interface PaymentSummary {
  totalReceived: number;
  totalPending: number;
  completedCount: number;
  pendingCount: number;
  failedCount: number;
  currency: string;
}
