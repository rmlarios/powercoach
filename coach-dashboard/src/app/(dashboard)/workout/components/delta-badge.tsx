'use client';

import { ChevronUp, ChevronDown, Equal } from 'lucide-react';
import { cn } from '@/lib/utils';
import type { DeltaDirection } from '../lib/workout-utils';

interface DeltaBadgeProps {
  value: string;
  direction: DeltaDirection;
}

const CONFIG: Record<DeltaDirection, { icon: React.ComponentType<{ className?: string }>; color: string }> = {
  up: { icon: ChevronUp, color: 'text-emerald-500' },
  down: { icon: ChevronDown, color: 'text-red-400' },
  equal: { icon: Equal, color: 'text-slate-300' },
};

/**
 * DeltaBadge — micro indicator showing actual vs target.
 *
 * "+5kg ↑" green  |  "-2r ↓" red  |  "=" gray
 *
 * Placed next to weight/reps inputs after set completion.
 * Compact: 9px font, inline with data.
 */
export function DeltaBadge({ value, direction }: DeltaBadgeProps) {
  const cfg = CONFIG[direction];
  const Icon = cfg.icon;

  return (
    <span
      className={cn(
        'inline-flex items-center gap-px text-[10px] font-mono font-medium whitespace-nowrap',
        cfg.color,
      )}
    >
      {value !== '=' && value}
      <Icon className="w-2.5 h-2.5" />
    </span>
  );
}
