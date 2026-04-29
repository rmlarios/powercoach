'use client';

import { useEffect, useState } from 'react';
import { Trophy } from 'lucide-react';
import { cn } from '@/lib/utils';
import type { PRType } from '../lib/workout-utils';

interface PRBadgeProps {
  types: PRType[];
  deltas: string[];
}

const TYPE_LABEL: Record<PRType, string> = {
  weight: 'Peso',
  reps: 'Reps',
  e1rm: 'e1RM',
};

/**
 * PRBadge — compact golden badge shown next to the ✓ button
 * when a set beats the athlete's previous best.
 *
 * Animates in with a scale-up + glow effect.
 * Paper-feel: subtle gold, not confetti.
 */
export function PRBadge({ types, deltas }: PRBadgeProps) {
  const [show, setShow] = useState(false);

  // Animate-in on mount
  useEffect(() => {
    const t = setTimeout(() => setShow(true), 50);
    return () => clearTimeout(t);
  }, []);

  // eslint-disable-next-line @typescript-eslint/no-unused-vars
  const _label = types.map((t) => TYPE_LABEL[t]).join(' + ');
  const deltaText = deltas.join(' · ');

  return (
    <div
      className={cn(
        'inline-flex items-center gap-1 px-1.5 py-0.5 rounded-full',
        'bg-gradient-to-r from-amber-100 to-yellow-100 border border-amber-200/60',
        'transition-all duration-500 ease-out',
        show
          ? 'opacity-100 scale-100'
          : 'opacity-0 scale-75',
      )}
    >
      <Trophy className="w-3 h-3 text-amber-500 flex-shrink-0" />
      <span className="text-[11px] font-bold text-amber-700 whitespace-nowrap">
        PR!
      </span>
      <span className="text-[10px] text-amber-500 font-mono whitespace-nowrap">
        {deltaText}
      </span>
    </div>
  );
}
