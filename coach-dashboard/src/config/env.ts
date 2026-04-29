/**
 * Environment configuration
 * Centralized access to environment variables with type safety
 */

// API Configuration
export const apiConfig = {
  baseUrl: process.env.NEXT_PUBLIC_API_URL ?? 'http://localhost:5001',
  timeout: Number(process.env.NEXT_PUBLIC_API_TIMEOUT) || 30000,
} as const;

// App Configuration
export const appConfig = {
  name: process.env.NEXT_PUBLIC_APP_NAME ?? 'CoachPlatform',
  version: process.env.NEXT_PUBLIC_APP_VERSION ?? '1.0.0',
  environment: process.env.NODE_ENV ?? 'development',
} as const;

// Feature Flags
export const featureFlags = {
  enableAnalytics: process.env.NEXT_PUBLIC_ENABLE_ANALYTICS === 'true',
  enableNotifications: process.env.NEXT_PUBLIC_ENABLE_NOTIFICATIONS === 'true',
  enableDebugMode: process.env.NODE_ENV === 'development',
} as const;

// Pagination Defaults
export const paginationConfig = {
  defaultPageSize: 10,
  pageSizeOptions: [10, 25, 50, 100],
} as const;

/**
 * Validate required environment variables
 * Call this at app initialization to catch missing config early
 */
export function validateEnv(): void {
  const requiredVars: string[] = [
    // Add required env vars here as the app grows
    // 'NEXT_PUBLIC_API_URL',
  ];

  const missing = requiredVars.filter(
    (varName) => !process.env[varName]
  );

  if (missing.length > 0) {
    console.warn(
      `Missing environment variables: ${missing.join(', ')}`
    );
  }
}

export const env = {
  api: apiConfig,
  app: appConfig,
  features: featureFlags,
  pagination: paginationConfig,
  validate: validateEnv,
} as const;
