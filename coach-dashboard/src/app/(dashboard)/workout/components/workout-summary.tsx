'use client';

import { useMemo } from 'react';
import { Trophy, Dumbbell, Clock, Flame, CheckCircle2, BarChart3 } from 'lucide-react';
import { calculateWorkoutStats } from '../lib/workout-utils';
import type { WorkoutExerciseGroup } from '@/types';

interface WorkoutSummaryProps {
  exercises: WorkoutExerciseGroup[];
  durationMinutes?: number;
  fatigueRating?: number;
  completedDate?: string;
}

/**
 * Workout Summary Card — shown after workout is completed.
 *
 * Design: paper-feel card, monospace numbers, emerald accent for completion.
 * Shows volume, sets, PRs, duration, fatigue.
 */
export function WorkoutSummary({
  exercises,
  durationMinutes,
  fatigueRating,
  completedDate,
}: WorkoutSummaryProps) {
  const stats = useMemo(() => calculateWorkoutStats(exercises), [exercises]);

  const formattedVolume = useMemo(() => {
    if (stats.totalVolume >= 1000) {
      return `${(stats.totalVolume / 1000).toFixed(1).replace(/\.0$/, '')}t`;
    }
    return `${stats.totalVolume.toLocaleString()}kg`;
  }, [stats.totalVolume]);

  const completionPct = stats.totalSets > 0
    ? Math.round((stats.completedSets / stats.totalSets) * 100)
    : 0;

  const formattedTime = useMemo(() => {
    if (!durationMinutes) return null;
    if (durationMinutes >= 60) {
      const h = Math.floor(durationMinutes / 60);
      const m = durationMinutes % 60;
      return `${h}h ${m}m`;
    }
    return `${durationMinutes}m`;
  }, [durationMinutes]);

  const fatigueLabel = useMemo(() => {
    if (!fatigueRating) return null;
    const labels: Record<number, string> = {
      1: 'Fresco',
      2: 'Ligero',
      3: 'Moderado',
      4: 'Duro',
      5: 'Máximo',
    };
    return labels[fatigueRating] ?? `${fatigueRating}/5`;
  }, [fatigueRating]);

  return (
    <div className="mx-3 my-4 rounded-xl border border-emerald-200 bg-gradient-to-br from-emerald-50 to-white p-5 shadow-sm">
      {/* Header */}
      <div className="flex items-center gap-2 mb-4">
        <CheckCircle2 className="h-5 w-5 text-emerald-600" />
        <h2 className="text-base font-semibold text-emerald-800">
          Entrenamiento completado
        </h2>
      </div>

      {/* Date stamp */}
      {completedDate && (
        <p className="text-xs text-slate-400 font-mono mb-4 -mt-2">
          {new Date(completedDate).toLocaleDateString('es-ES', {
            weekday: 'short',
            day: 'numeric',
            month: 'short',
            hour: '2-digit',
            minute: '2-digit',
          })}
        </p>
      )}

      {/* Stats grid — 2×3 */}
      <div className="grid grid-cols-3 gap-3">
        {/* Volume */}
        <StatCell
          icon={<BarChart3 className="h-4 w-4 text-blue-500" />}
          label="Volumen"
          value={formattedVolume}
        />

        {/* Sets */}
        <StatCell
          icon={<Dumbbell className="h-4 w-4 text-slate-500" />}
          label="Series"
          value={`${stats.completedSets}/${stats.totalSets}`}
          sublabel={`${completionPct}%`}
        />

        {/* PRs */}
        <StatCell
          icon={<Trophy className="h-4 w-4 text-amber-500" />}
          label="PRs"
          value={`${stats.prsCount}`}
          highlight={stats.prsCount > 0}
        />

        {/* Duration */}
        {formattedTime && (
          <StatCell
            icon={<Clock className="h-4 w-4 text-indigo-500" />}
            label="Duración"
            value={formattedTime}
          />
        )}

        {/* Fatigue */}
        {fatigueRating && (
          <StatCell
            icon={<Flame className="h-4 w-4 text-orange-500" />}
            label="Fatiga"
            value={`${fatigueRating}/5`}
            sublabel={fatigueLabel ?? undefined}
          />
        )}

        {/* Exercises */}
        <StatCell
          icon={<Dumbbell className="h-4 w-4 text-purple-500" />}
          label="Ejercicios"
          value={`${stats.exerciseCount}`}
        />
      </div>
    </div>
  );
}

// ── Stat Cell ──────────────────────────────────────────────────

interface StatCellProps {
  icon: React.ReactNode;
  label: string;
  value: string;
  sublabel?: string;
  highlight?: boolean;
}

function StatCell({ icon, label, value, sublabel, highlight }: StatCellProps) {
  return (
    <div
      className={`rounded-lg p-3 text-center ${
        highlight
          ? 'bg-amber-50 ring-1 ring-amber-200'
          : 'bg-white ring-1 ring-slate-100'
      }`}
    >
      <div className="flex justify-center mb-1">{icon}</div>
      <div className="font-mono text-lg font-bold text-slate-800">{value}</div>
      <div className="text-[11px] text-slate-500 uppercase tracking-wider">{label}</div>
      {sublabel && (
        <div className="text-[11px] text-slate-500 mt-0.5">{sublabel}</div>
      )}
    </div>
  );
}
