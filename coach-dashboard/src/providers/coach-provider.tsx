'use client';

import { createContext, useContext, useState, useEffect, useCallback, ReactNode } from 'react';
import { USER_KEY } from '@/lib/api/client';

export interface Coach {
  id: string;
  name: string;
  email: string;
  logoUrl?: string;
}

interface CoachContextType {
  coach: Coach | null;
  setCoach: (coach: Coach | null) => void;
  updateCoach: (updates: Partial<Coach>) => void;
  isLoading: boolean;
}

const CoachContext = createContext<CoachContextType | undefined>(undefined);

const COACH_STORAGE_KEY = 'coach-platform-coach';

// Fallback for demo/development environment
const DEMO_COACH: Coach = {
  id: '00000000-0000-0000-0000-000000000001',
  name: 'Demo Coach',
  email: 'demo@coachplatform.com',
};

function loadCoachFromStorage(): Coach | null {
  if (typeof window === 'undefined') return null;
  try {
    // Try to get coach identity from JWT user data
    const userData = localStorage.getItem(USER_KEY);
    if (userData) {
      const user = JSON.parse(userData);
      // Only coaches have a coachId
      if (user.coachId && user.role === 'Coach') {
        const stored = localStorage.getItem(COACH_STORAGE_KEY);
        const profile = stored ? JSON.parse(stored) : {};
        return {
          id: user.coachId,
          name: profile.name ?? user.username ?? 'Coach',
          email: user.email,
          logoUrl: profile.logoUrl,
        };
      }
    }
  } catch {
    // ignore
  }
  // Admin and Athlete users don't have a coach context
  return null;
}

function saveCoachToStorage(coach: Coach) {
  try {
    localStorage.setItem(COACH_STORAGE_KEY, JSON.stringify(coach));
  } catch {
    // ignore
  }
}

export function CoachProvider({ children }: { children: ReactNode }) {
  const [coach, setCoachState] = useState<Coach | null>(null);
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    setCoachState(loadCoachFromStorage());
    setIsLoading(false);
  }, []);

  const setCoach = useCallback((c: Coach | null) => {
    setCoachState(c);
    if (c) saveCoachToStorage(c);
  }, []);

  const updateCoach = useCallback((updates: Partial<Coach>) => {
    setCoachState((prev) => {
      if (!prev) return prev;
      const updated = { ...prev, ...updates };
      saveCoachToStorage(updated);
      return updated;
    });
  }, []);

  return (
    <CoachContext.Provider value={{ coach, setCoach, updateCoach, isLoading }}>
      {children}
    </CoachContext.Provider>
  );
}

export function useCoach() {
  const context = useContext(CoachContext);
  if (context === undefined) {
    throw new Error('useCoach must be used within a CoachProvider');
  }
  return context;
}

