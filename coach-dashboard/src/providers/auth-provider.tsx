'use client';

import {
  createContext,
  useContext,
  useState,
  useEffect,
  useCallback,
  ReactNode,
} from 'react';
import { useRouter } from 'next/navigation';
import { authApi, UserDto, LoginRequest } from '@/lib/api/auth-api';
import { storeTokens, clearTokens, TOKEN_KEY, USER_KEY } from '@/lib/api/client';

interface AuthContextType {
  user: UserDto | null;
  isLoading: boolean;
  isAuthenticated: boolean;
  login: (data: LoginRequest) => Promise<void>;
  logout: () => Promise<void>;
}

const AuthContext = createContext<AuthContextType | undefined>(undefined);

export function AuthProvider({ children }: { children: ReactNode }) {
  const [user, setUser] = useState<UserDto | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const router = useRouter();

  // On mount, restore user from localStorage
  useEffect(() => {
    if (typeof window === 'undefined') {
      setIsLoading(false);
      return;
    }
    const token = localStorage.getItem(TOKEN_KEY);
    const stored = localStorage.getItem(USER_KEY);
    if (token && stored) {
      try {
        setUser(JSON.parse(stored));
      } catch {
        clearTokens();
      }
    }
    setIsLoading(false);
  }, []);

  const login = useCallback(async (data: LoginRequest) => {
    const response = await authApi.login(data);
    storeTokens(response.accessToken, response.refreshToken);
    // Set cookies for middleware route protection
    document.cookie = `cp_access_token=${response.accessToken}; path=/; max-age=900; SameSite=Lax`;
    document.cookie = `cp_user_role=${response.user.role}; path=/; max-age=900; SameSite=Lax`;
    if (typeof window !== 'undefined') {
      localStorage.setItem(USER_KEY, JSON.stringify(response.user));
    }
    setUser(response.user);
    // Redirect based on role
    const destination = response.user.role === 'Athlete' ? '/athlete' : '/dashboard';
    router.push(destination);
  }, [router]);

  const logout = useCallback(async () => {
    try {
      await authApi.logout();
    } catch {
      // ignore errors on logout
    } finally {
      clearTokens();
      // Clear the middleware cookies
      document.cookie = 'cp_access_token=; path=/; expires=Thu, 01 Jan 1970 00:00:00 GMT';
      document.cookie = 'cp_user_role=; path=/; expires=Thu, 01 Jan 1970 00:00:00 GMT';
      setUser(null);
      router.push('/login');
    }
  }, [router]);

  return (
    <AuthContext.Provider
      value={{
        user,
        isLoading,
        isAuthenticated: !!user,
        login,
        logout,
      }}
    >
      {children}
    </AuthContext.Provider>
  );
}

export function useAuth(): AuthContextType {
  const context = useContext(AuthContext);
  if (!context) throw new Error('useAuth must be used within AuthProvider');
  return context;
}
