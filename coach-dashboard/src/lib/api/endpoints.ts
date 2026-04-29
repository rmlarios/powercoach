// API Endpoints configuration
export const API_ENDPOINTS = {
  // Auth
  auth: {
    login: '/auth/login',
    refresh: '/auth/refresh-token',
    logout: '/auth/logout',
    me: '/auth/me',
    users: '/auth/users',
    userById: (id: string) => `/auth/users/${id}`,
    resetPassword: (id: string) => `/auth/users/${id}/reset-password`,
  },


  // Applications
  applications: {
    list: '/applications',
    getById: (id: string) => `/applications/${id}`,
    create: '/applications',
    approve: (id: string) => `/applications/${id}/approve`,
    reject: (id: string) => `/applications/${id}/reject`,
  },
  
  // Athletes
  athletes: {
    list: '/athletes',
    getById: (id: string) => `/athletes/${id}`,
    create: '/athletes',
    update: (id: string) => `/athletes/${id}`,
    deactivate: (id: string) => `/athletes/${id}`,
    maxLifts: (id: string) => `/athletes/${id}/max-lifts`,
    exerciseHistory: (athleteId: string, exerciseId: string) => 
      `/athletes/${athleteId}/exercise-history/${exerciseId}`,
  },
  
  // Exercises (global catalog)
  exercises: {
    list: '/exercises',
    getById: (exerciseId: string) => `/exercises/${exerciseId}`,
    create: '/exercises',
  },
  
  // Training Programs
  programs: {
    list: '/programs',
    getById: (id: string) => `/programs/${id}`,
    create: '/programs',
    update: (id: string) => `/programs/${id}`,
    saveFull: (id: string) => `/programs/${id}/full`,
    addWeek: (programId: string) => `/programs/${programId}/weeks`,
    deleteWeek: (programId: string, weekId: string) => `/programs/${programId}/weeks/${weekId}`,
    addDay: (programId: string, weekId: string) => `/programs/${programId}/weeks/${weekId}/days`,
    deleteDay: (programId: string, dayId: string) => `/programs/${programId}/days/${dayId}`,
    addExercise: (programId: string, weekId: string, dayId: string) => 
      `/programs/${programId}/weeks/${weekId}/days/${dayId}/exercises`,
    deleteExercise: (programId: string, exerciseTemplateId: string) => 
      `/programs/${programId}/exercises/${exerciseTemplateId}`,
  },
  
  // Athlete Programs
  athletePrograms: {
    get: (athleteId: string) => `/athletes/${athleteId}/program`,
    assign: (athleteId: string) => `/athletes/${athleteId}/assign-program`,
    bulkAssign: (programId: string) => `/programs/${programId}/bulk-assign`,
    getWorkout: (athleteId: string, workoutId: string) => `/athletes/${athleteId}/workouts/${workoutId}`,
    logWorkout: (athleteId: string, workoutId: string) => `/athletes/${athleteId}/workouts/${workoutId}/log`,
  },
  
  // Workout Tracking (F-015)
  workoutTracking: {
    today: (athleteId: string) => `/athletes/${athleteId}/workouts/today`,
    getById: (athleteId: string, workoutId: string) => `/athletes/${athleteId}/workouts/${workoutId}`,
    history: (athleteId: string) => `/athletes/${athleteId}/workouts/history`,
    week: (athleteId: string) => `/athletes/${athleteId}/workouts/week`,
    exerciseLiftHistory: (athleteId: string, exerciseId: string) => `/athletes/${athleteId}/workouts/exercises/${exerciseId}/lift-history`,
    start: (athleteId: string, workoutId: string) => `/athletes/${athleteId}/workouts/${workoutId}/start`,
    saveSet: (athleteId: string, workoutId: string) => `/athletes/${athleteId}/workouts/${workoutId}/sets`,
    updateSet: (athleteId: string, workoutId: string, logId: string) => `/athletes/${athleteId}/workouts/${workoutId}/sets/${logId}`,
    completeSet: (athleteId: string, workoutId: string, logId: string) => `/athletes/${athleteId}/workouts/${workoutId}/sets/${logId}/complete`,
    skip: (athleteId: string, workoutId: string) => `/athletes/${athleteId}/workouts/${workoutId}/skip`,
    complete: (athleteId: string, workoutId: string) => `/athletes/${athleteId}/workouts/${workoutId}/complete`,
  },
  
  // Subscriptions
  subscriptions: {
    list: '/subscriptions',
    getById: (id: string) => `/subscriptions/${id}`,
    create: '/subscriptions',
    cancel: (id: string) => `/subscriptions/${id}/cancel`,
    byAthlete: (athleteId: string) => `/athletes/${athleteId}/subscriptions`,
  },
  
  // Check-ins
  checkIns: {
    list: '/check-ins',
    coachList: '/check-ins/coach',
    getById: (id: string) => `/check-ins/${id}`,
    create: '/check-ins',
    review: (id: string) => `/check-ins/${id}/review`,
    athleteList: (athleteId: string) => `/athletes/${athleteId}/checkins`,
  },
  
  // Payments
  payments: {
    list: '/payments',
    getById: (id: string) => `/payments/${id}`,
    create: '/payments',
    byAthlete: (athleteId: string) => `/athletes/${athleteId}/payments`,
  },

  // Plans
  plans: {
    list: '/plans',
    create: '/plans',
    update: (id: string) => `/plans/${id}`,
    deactivate: (id: string) => `/plans/${id}/deactivate`,
  },

  // Dashboard
  dashboard: {
    coach: '/dashboard/coach',
  },
} as const;
