'use client';

import { useCallback, useRef } from 'react';
import { useSaveSet } from '@/hooks';
import type { SaveSetRequest } from '@/types';

/**
 * Parses "smart input" notation for weight × reps.
 * Accepts: "100x8", "100X8", "100 8", "100*8"
 * Returns null if parsing fails.
 */
export function parseSmartInput(raw: string): { weight: number; reps: number } | null {
  const trimmed = raw.trim();
  if (!trimmed) return null;

  // Match: number (separator) number
  const match = trimmed.match(/^(\d+(?:\.\d+)?)\s*[xX*×\s]\s*(\d+)$/);
  if (match) {
    const weight = parseFloat(match[1]);
    const reps = parseInt(match[2], 10);
    if (weight > 0 && reps > 0) return { weight, reps };
  }

  return null;
}

/**
 * Debounced auto-save hook for set data.
 * Fires save after `delay`ms of inactivity. Cancels on unmount.
 */
export function useAutoSave(athleteId: string, workoutId: string, delay = 600) {
  const { mutate: saveSet, isPending } = useSaveSet();
  const timerRef = useRef<ReturnType<typeof setTimeout> | null>(null);
  const latestRef = useRef<SaveSetRequest | null>(null);

  const save = useCallback(
    (data: SaveSetRequest) => {
      latestRef.current = data;

      if (timerRef.current) clearTimeout(timerRef.current);

      timerRef.current = setTimeout(() => {
        if (latestRef.current) {
          saveSet({ athleteId, workoutId, data: latestRef.current });
        }
      }, delay);
    },
    [athleteId, workoutId, delay, saveSet],
  );

  /** Flush any pending save immediately. */
  const flush = useCallback(() => {
    if (timerRef.current) {
      clearTimeout(timerRef.current);
      timerRef.current = null;
    }
    if (latestRef.current) {
      saveSet({ athleteId, workoutId, data: latestRef.current });
      latestRef.current = null;
    }
  }, [athleteId, workoutId, saveSet]);

  return { save, flush, isSaving: isPending };
}

/**
 * Trigger haptic feedback on mobile (short vibration).
 */
export function haptic(duration = 10) {
  if (typeof navigator !== 'undefined' && 'vibrate' in navigator) {
    navigator.vibrate(duration);
  }
}

/**
 * Format seconds as m:ss or mm:ss.
 */
export function formatTimer(seconds: number): string {
  const m = Math.floor(seconds / 60);
  const s = seconds % 60;
  return `${m}:${s.toString().padStart(2, '0')}`;
}

// ─── PR Detection ────────────────────────────────────────────

export type PRType = 'weight' | 'reps' | 'e1rm';

export interface PRResult {
  isPR: boolean;
  types: PRType[];
  /** e.g. "+5kg", "+2r", "+3.2 e1RM" */
  deltas: string[];
}

/**
 * Detect if the current set is a personal record compared to previous best.
 *
 * Checks three axes:
 * 1. **Weight PR**: actualWeight > previousPerformance.maxWeight
 * 2. **Reps PR**: actualReps > previousPerformance.maxReps (at same or higher weight)
 * 3. **e1RM PR**: Epley e1RM > previousPerformance.estimatedOneRM
 *
 * Epley formula: weight × (1 + reps / 30)
 */
export function detectPR(
  actualWeight: number,
  actualReps: number,
  prev: { maxWeight: number; maxReps: number; estimatedOneRM?: number } | undefined | null,
): PRResult {
  const empty: PRResult = { isPR: false, types: [], deltas: [] };

  if (!prev || actualWeight <= 0 || actualReps <= 0) return empty;

  const types: PRType[] = [];
  const deltas: string[] = [];

  // Weight PR
  if (actualWeight > prev.maxWeight) {
    types.push('weight');
    deltas.push(`+${(actualWeight - prev.maxWeight).toFixed(1).replace(/\.0$/, '')}kg`);
  }

  // Reps PR (only meaningful at same or higher weight)
  if (actualReps > prev.maxReps && actualWeight >= prev.maxWeight) {
    types.push('reps');
    deltas.push(`+${actualReps - prev.maxReps}r`);
  }

  // e1RM PR (Epley)
  if (prev.estimatedOneRM && prev.estimatedOneRM > 0) {
    const currentE1RM = actualReps === 1
      ? actualWeight
      : actualWeight * (1 + actualReps / 30);
    if (currentE1RM > prev.estimatedOneRM) {
      types.push('e1rm');
      deltas.push(`+${(currentE1RM - prev.estimatedOneRM).toFixed(1)} e1RM`);
    }
  }

  return {
    isPR: types.length > 0,
    types,
    deltas,
  };
}

// ─── Workout Summary Stats ───────────────────────────────────

export interface WorkoutStats {
  totalVolume: number;       // sum of weight × reps for all completed sets
  completedSets: number;
  totalSets: number;
  prsCount: number;          // number of sets that beat previousPerformance
  exerciseCount: number;
}

/**
 * Calculate aggregate workout stats from exercise groups.
 * Used by the WorkoutSummary card shown after completion.
 */
export function calculateWorkoutStats(
  exercises: Array<{
    sets: Array<{ actualWeight: number; actualReps: number; isCompleted: boolean }>;
    previousPerformance?: { maxWeight: number; maxReps: number; estimatedOneRM?: number } | null;
  }>,
): WorkoutStats {
  let totalVolume = 0;
  let completedSets = 0;
  let totalSets = 0;
  let prsCount = 0;

  for (const ex of exercises) {
    for (const s of ex.sets) {
      totalSets++;
      if (s.isCompleted) {
        completedSets++;
        totalVolume += s.actualWeight * s.actualReps;
        // Count PRs
        const pr = detectPR(s.actualWeight, s.actualReps, ex.previousPerformance);
        if (pr.isPR) prsCount++;
      }
    }
  }

  return {
    totalVolume,
    completedSets,
    totalSets,
    prsCount,
    exerciseCount: exercises.length,
  };
}

// ─── Delta vs Plan ───────────────────────────────────────────

export type DeltaDirection = 'up' | 'down' | 'equal';

export interface DeltaResult {
  value: string;
  direction: DeltaDirection;
}

/**
 * Calculate the delta between actual and target values.
 * Returns null if no meaningful comparison is possible.
 *
 * - Weight: "+5kg" / "-2.5kg" / "="
 * - Reps: "+2r" / "-1r" / "="
 */
export function calculateDelta(
  actual: number,
  target: number | undefined | null,
  unit: 'kg' | 'r',
): DeltaResult | null {
  if (target == null || target <= 0 || actual <= 0) return null;

  const diff = actual - target;

  if (Math.abs(diff) < 0.01) {
    return { value: '=', direction: 'equal' };
  }

  const sign = diff > 0 ? '+' : '';
  const formatted = unit === 'kg'
    ? `${sign}${diff.toFixed(1).replace(/\.0$/, '')}${unit}`
    : `${sign}${Math.round(diff)}${unit}`;

  return {
    value: formatted,
    direction: diff > 0 ? 'up' : 'down',
  };
}
