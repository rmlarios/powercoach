'use client';

import { CalendarOff } from 'lucide-react';

/**
 * Empty state when no workout is scheduled for today.
 * Clean, minimal — no heavy cards or distracting elements.
 */
export function EmptyWorkout() {
  return (
    <div className="flex flex-col items-center justify-center min-h-[60vh] gap-4 px-6 text-center">
      <div className="flex items-center justify-center w-16 h-16 rounded-full bg-slate-100">
        <CalendarOff className="w-7 h-7 text-slate-400" />
      </div>
      <div>
        <p className="text-sm font-medium text-slate-700">Sin entrenamiento hoy</p>
        <p className="mt-1 text-xs text-slate-400 max-w-[240px]">
          No hay un workout programado para hoy. Disfruta tu día de descanso.
        </p>
      </div>
    </div>
  );
}
