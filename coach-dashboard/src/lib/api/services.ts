// Re-export all services from modular API files
// This file is kept for backward compatibility
// New code should import from specific API modules directly

export { applicationsApi as applicationsService } from './applications-api';
export { athletesApi as athletesService } from './athletes-api';
export { exercisesApi as exercisesService } from './exercises-api';
export { programsApi as programsService } from './programs-api';
export { athleteProgramsApi as athleteProgramsService } from './programs-api';
export { plansApi as plansService } from './plans-api';
export { subscriptionsApi as subscriptionsService } from './subscriptions-api';
export { paymentsApi as paymentsService } from './payments-api';
