'use client';

import { TrendingUp } from 'lucide-react';
import type { PreviousPerformance as PreviousPerformanceType } from '@/types';

interface PreviousPerformanceProps {
  data: PreviousPerformanceType;
}

/**
 * Subtle inline display of last session's performance for an exercise.
 * Shown beneath the exercise name as faint reference data.
 */
export function PreviousPerformanceBadge({ data }: PreviousPerformanceProps) {
  const date = new Date(data.date);
  const ago = getRelativeDate(date);

  return (
    <div className="flex items-center gap-2 text-xs text-slate-400 font-mono select-none">
      <TrendingUp className="w-3 h-3 flex-shrink-0" />
      <span>
        {data.maxWeight}kg × {data.maxReps}r
        {data.bestRpe != null && <span className="text-slate-300"> @{data.bestRpe}</span>}
      </span>
      {data.estimatedOneRM != null && (
        <span className="text-slate-300">
          · e1RM {data.estimatedOneRM.toFixed(0)}kg
        </span>
      )}
      <span className="text-slate-300">· {ago}</span>
    </div>
  );
}

function getRelativeDate(date: Date): string {
  const now = new Date();
  const diffMs = now.getTime() - date.getTime();
  const diffDays = Math.floor(diffMs / (1000 * 60 * 60 * 24));

  if (diffDays === 0) return 'hoy';
  if (diffDays === 1) return 'ayer';
  if (diffDays < 7) return `hace ${diffDays}d`;
  if (diffDays < 30) return `hace ${Math.floor(diffDays / 7)}sem`;
  return `hace ${Math.floor(diffDays / 30)}m`;
}
