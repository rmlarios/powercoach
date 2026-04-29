/**
 * Application routes configuration
 * Centralized route definitions for the entire application
 */
export const routes = {
  // Main dashboard routes
  dashboard: '/dashboard',
  applications: '/applications',
  athletes: '/athletes',
  checkIns: '/check-ins',
  programs: '/programs',
  exercises: '/exercises',
  plans: '/plans',
  subscriptions: '/subscriptions',
  payments: '/payments',
  workout: '/workout',
  settings: '/settings',

  // Public routes
  apply: '/apply',

  // Athlete portal routes
  athleteDashboard: '/athlete',
  athleteCheckIn: '/athlete/check-in',

  // Detail routes
  applicationDetail: (id: string) => `/applications/${id}`,
  athleteDetail: (id: string) => `/athletes/${id}`,
  programDetail: (id: string) => `/programs/${id}`,
  exerciseDetail: (id: string) => `/exercises/${id}`,

  // Athlete sub-routes
  athleteProgram: (athleteId: string) => `/athletes/${athleteId}/program`,
  athleteWorkout: (athleteId: string, workoutId: string) => 
    `/athletes/${athleteId}/workouts/${workoutId}`,
  athleteCheckIns: (athleteId: string) => `/athletes/${athleteId}/check-ins`,

  // Program builder routes
  programBuilder: (programId: string) => `/programs/${programId}/builder`,
  
  // Admin routes
  adminUsers: '/admin/users',

  // Auth routes
  login: '/login',
  register: '/register',
  forgotPassword: '/forgot-password',
  resetPassword: '/reset-password',
} as const;

export type AppRoute = typeof routes;
