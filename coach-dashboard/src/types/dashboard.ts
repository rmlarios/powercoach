// Dashboard Types — mirrors backend DTOs

export interface CoachDashboard {
  stats: DashboardStats;
  alerts: DashboardAlert[];
  athleteStatuses: AthleteStatusRow[];
  recentActivity: ActivityFeedItem[];
}

export interface DashboardStats {
  activeAthletes: number;
  pendingApplications: number;
  activePrograms: number;
  completionRate: number;
  pendingCheckIns: number;
  athletesTrend: number;
}

export interface DashboardAlert {
  alertType: 'HighFatigue' | 'MissedWorkout' | 'MissedCheckIn' | 'ExpiringSubscription' | 'PendingApplication';
  severity: 'warning' | 'danger';
  message: string;
  athleteId?: string;
  athleteName?: string;
  createdAt: string;
}

export interface AthleteStatusRow {
  athleteId: string;
  name: string;
  profilePictureUrl?: string;
  status: string;
  statusColor: 'green' | 'yellow' | 'red';
  lastWorkoutDate?: string;
  lastWorkoutName?: string;
  lastCheckInDate?: string;
  isCheckInReviewed?: boolean;
  activeProgramName?: string;
  currentWeek?: number;
}

export interface ActivityFeedItem {
  type: 'WorkoutCompleted' | 'WorkoutStarted' | 'CheckInSubmitted' | 'ApplicationReceived';
  athleteName?: string;
  athleteId?: string;
  description: string;
  timestamp: string;
}
