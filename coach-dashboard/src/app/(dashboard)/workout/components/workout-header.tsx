'use client';

import { useCallback, useEffect, useRef, useState } from 'react';
import { Play, SkipForward, CheckCircle2, Clock, ChevronDown, ChevronUp, Info } from 'lucide-react';
import { cn } from '@/lib/utils';
import { formatTimer, haptic } from '../lib/workout-utils';
import type { TodayWorkout, WorkoutStatus } from '@/types';

interface WorkoutHeaderProps {
  workout: TodayWorkout;
  onStart: () => void;
  onSkip: () => void;
  onComplete: () => void;
  isStarting?: boolean;
}

const STATUS_CONFIG: Record<WorkoutStatus, { label: string; color: string }> = {
  NotStarted: { label: 'Listo', color: 'bg-slate-200 text-slate-600' },
  InProgress: { label: 'En curso', color: 'bg-blue-100 text-blue-700' },
  Completed: { label: 'Completado', color: 'bg-emerald-100 text-emerald-700' },
  Skipped: { label: 'Saltado', color: 'bg-amber-100 text-amber-700' },
  PartiallyCompleted: { label: 'Parcial', color: 'bg-yellow-100 text-yellow-700' },
};

/**
 * WorkoutHeader — top section showing the workout name, status,
 * elapsed timer, and primary action buttons.
 *
 * Minimal, distraction-free. Timer only visible during InProgress.
 */
export function WorkoutHeader({
  workout,
  onStart,
  onSkip,
  onComplete,
  isStarting,
}: WorkoutHeaderProps) {
  const { status } = workout;
  const isInProgress = status === 'InProgress';
  const isNotStarted = status === 'NotStarted';
  const isDone = status === 'Completed' || status === 'PartiallyCompleted' || status === 'Skipped';

  // Elapsed timer
  const [elapsed, setElapsed] = useState(0);
  const intervalRef = useRef<ReturnType<typeof setInterval> | null>(null);
  const [notesExpanded, setNotesExpanded] = useState(false);

  useEffect(() => {
    if (isInProgress && workout.startedAt) {
      const start = new Date(workout.startedAt).getTime();
      const tick = () => setElapsed(Math.floor((Date.now() - start) / 1000));
      tick();
      intervalRef.current = setInterval(tick, 1000);
      return () => {
        if (intervalRef.current) clearInterval(intervalRef.current);
      };
    }
    return () => {
      if (intervalRef.current) clearInterval(intervalRef.current);
    };
  }, [isInProgress, workout.startedAt]);

  const statusCfg = STATUS_CONFIG[status] ?? STATUS_CONFIG.NotStarted;

  // Focus info line
  const infoLine = [
    workout.programName,
    `Semana ${workout.weekNumber}`,
    workout.dayName ?? `Día ${workout.dayNumber}`,
    workout.focus,
  ]
    .filter(Boolean)
    .join(' · ');

  const handleStart = useCallback(() => {
    haptic(15);
    onStart();
  }, [onStart]);

  const handleComplete = useCallback(() => {
    haptic(20);
    onComplete();
  }, [onComplete]);

  return (
    <div className="px-2 pb-3 pt-1">
      {/* Top row: status badge + timer */}
      <div className="flex items-center justify-between mb-1">
        <span className={cn('text-xs font-medium px-2 py-0.5 rounded-full', statusCfg.color)}>
          {statusCfg.label}
        </span>

        {isInProgress && (
          <div className="flex items-center gap-1 text-sm font-mono text-slate-500">
            <Clock className="w-3.5 h-3.5" />
            <span>{formatTimer(elapsed)}</span>
          </div>
        )}

        {isDone && workout.durationMinutes != null && (
          <span className="text-xs font-mono text-slate-400">
            {workout.durationMinutes}min
          </span>
        )}
      </div>

      {/* Info line */}
      <p className="text-xs text-slate-400 leading-tight mb-3 truncate">{infoLine}</p>

      {/* Week notes banner — collapsible */}
      {workout.weekNotes && (
        <button
          type="button"
          onClick={() => setNotesExpanded((v) => !v)}
          className="w-full mb-3 flex items-start gap-2 rounded-lg bg-blue-50 border border-blue-100 px-3 py-2 text-left text-xs text-blue-700 hover:bg-blue-100 transition-colors"
        >
          <Info className="w-3.5 h-3.5 mt-0.5 shrink-0" />
          <span className={cn('flex-1', !notesExpanded && 'line-clamp-1')}>
            {workout.weekNotes}
          </span>
          {notesExpanded ? (
            <ChevronUp className="w-3.5 h-3.5 shrink-0" />
          ) : (
            <ChevronDown className="w-3.5 h-3.5 shrink-0" />
          )}
        </button>
      )}

      {/* Action buttons */}
      <div className="flex gap-2">
        {isNotStarted && (
          <>
            <button
              type="button"
              onClick={handleStart}
              disabled={isStarting}
              className={cn(
                'flex-1 flex items-center justify-center gap-2 h-11 rounded-lg text-sm font-medium transition-all',
                'bg-blue-600 text-white active:scale-[0.98]',
                isStarting && 'opacity-60 cursor-wait',
              )}
            >
              <Play className="w-4 h-4" />
              Comenzar
            </button>
            <button
              type="button"
              onClick={onSkip}
              className="flex items-center justify-center w-11 h-11 rounded-lg bg-slate-100 text-slate-500 hover:bg-slate-200 active:scale-95 transition-all"
              aria-label="Skip workout"
            >
              <SkipForward className="w-4 h-4" />
            </button>
          </>
        )}

        {isInProgress && (
          <button
            type="button"
            onClick={handleComplete}
            className="flex-1 flex items-center justify-center gap-2 h-11 rounded-lg text-sm font-medium bg-emerald-600 text-white active:scale-[0.98] transition-all"
          >
            <CheckCircle2 className="w-4 h-4" />
            Finalizar Entrenamiento
          </button>
        )}
      </div>
    </div>
  );
}
