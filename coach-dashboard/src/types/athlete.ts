// Athlete types — aligned with backend AthleteDtos

export type AthleteStatus = 'Active' | 'Inactive' | 'OnHold' | 'Graduated' | 'Suspended';

/** Lightweight list item (matches backend AthleteListItemDto) */
export interface AthleteListItem {
  id: string;
  fullName: string;
  email: string;
  country?: string;
  status: AthleteStatus;
  startDate: string;
  profilePictureUrl?: string;
}

/** Full detail (matches backend AthleteDto) */
export interface Athlete {
  id: string;
  coachId: string;
  firstName: string;
  lastName: string;
  fullName: string;
  email: string;
  phone?: string;
  goals?: string;
  notes?: string;
  country?: string;
  gender?: string;
  dateOfBirth?: string;
  height?: number;
  weight?: number;
  experienceLevel?: string;
  startDate: string;
  endDate?: string;
  status: AthleteStatus;
  profilePictureUrl?: string;
  applicationId?: string;
  createdAt: string;
  updatedAt: string;
}

/** Extended detail with related data (matches backend AthleteDetailDto) */
export interface AthleteDetail extends Athlete {
  activeSubscriptions: SubscriptionSummary[];
  recentCheckIns: CheckInSummary[];
}

export interface SubscriptionSummary {
  id: string;
  planName: string;
  startDate: string;
  endDate: string;
  status: string;
}

export interface CheckInSummary {
  id: string;
  checkInDate: string;
  weight?: number;
  isReviewed: boolean;
}

export interface CreateAthleteRequest {
  coachId: string;
  firstName: string;
  lastName: string;
  email: string;
  phone?: string;
  goals?: string;
  country?: string;
  gender?: string;
  dateOfBirth?: string;
  height?: number;
  weight?: number;
  experienceLevel?: string;
}

export interface UpdateAthleteRequest {
  country?: string;
  height?: number;
  weight?: number;
  experienceLevel?: string;
}

// ==========================================
// Max Lifts (1RM) types
// ==========================================

/**
 * A single max lift record
 */
export interface MaxLift {
  id: string;
  exerciseId: string;
  exerciseName: string;
  weight: number;
  isTested: boolean;
  recordedAt: string;
  notes?: string;
  estimationDetails?: string;
}

/**
 * Response from GET /athletes/{id}/max-lifts
 */
export interface AthleteMaxLifts {
  athleteId: string;
  athleteName: string;
  maxLifts: MaxLift[];
}

/**
 * Request to register a new max lift
 */
export interface RegisterMaxLiftRequest {
  exerciseId: string;
  weight: number;
  isTested?: boolean;
  recordedAt?: string;
  notes?: string;
  estimationDetails?: string;
}

// ==========================================
// Exercise History types
// ==========================================

/**
 * Summary of a max lift record
 */
export interface MaxLiftSummary {
  weight: number;
  recordedAt: string;
  isTested: boolean;
  source?: string;
}

/**
 * Summary of last programmed exercise
 */
export interface LastProgrammed {
  programName: string;
  weekNumber: number;
  dayNumber: number;
  prescription: string;
  programDate: string;
}

/**
 * Summary of an exercise performance log
 */
export interface ExerciseLogSummary {
  performedAt: string;
  sets: number;
  reps: number;
  weight: number;
  rpe?: number;
  notes?: string;
}

/**
 * Response from GET /athletes/{id}/exercise-history/{exerciseId}
 */
export interface ExerciseHistory {
  athleteId: string;
  athleteName: string;
  exerciseId: string;
  exerciseName: string;
  current1RM?: MaxLiftSummary;
  personalRecord?: MaxLiftSummary;
  lastProgrammed?: LastProgrammed;
  trendKg?: number;
  suggestedStartPercentage?: number;
  suggestedStartWeight?: number;
  recentLogs: ExerciseLogSummary[];
  hasHistory: boolean;
}
