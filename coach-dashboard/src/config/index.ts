// Config module index
// Re-exports all configuration for convenient imports

export { routes, type AppRoute } from './routes';
export { 
  mainNavigation, 
  getNavigationItem, 
  getNavigationByRole,
  isActiveRoute,
  type NavigationItem,
  type UserRole,
} from './navigation';
export { 
  apiConfig, 
  appConfig, 
  featureFlags, 
  paginationConfig,
  validateEnv,
  env,
} from './env';
