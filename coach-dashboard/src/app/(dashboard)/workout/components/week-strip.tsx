'use client';

import { useCallback } from 'react';
import { ChevronLeft, ChevronRight, Check, Minus } from 'lucide-react';
import { cn } from '@/lib/utils';
import { haptic } from '../lib/workout-utils';
import type { WeekWorkouts, WeekDay, WorkoutStatus } from '@/types';

interface WeekStripProps {
  week: WeekWorkouts;
  /** Currently displayed workout id (highlighted in the strip). */
  activeWorkoutId?: string;
  onSelectDay: (workoutId: string) => void;
  onPrevWeek: () => void;
  onNextWeek: () => void;
}

// ─── Status dot config ───────────────────────────────────────
const DOT_STATUS: Record<
  WorkoutStatus,
  { bg: string; ring: string; icon?: React.ComponentType<{ className?: string }> }
> = {
  Completed: {
    bg: 'bg-emerald-500',
    ring: 'ring-emerald-200',
    icon: Check,
  },
  PartiallyCompleted: {
    bg: 'bg-yellow-400',
    ring: 'ring-yellow-200',
    icon: Minus,
  },
  InProgress: {
    bg: 'bg-blue-500',
    ring: 'ring-blue-200',
  },
  NotStarted: {
    bg: 'bg-slate-200',
    ring: 'ring-slate-100',
  },
  Skipped: {
    bg: 'bg-amber-300',
    ring: 'ring-amber-100',
    icon: Minus,
  },
};

const DAY_LETTERS = ['L', 'M', 'X', 'J', 'V', 'S', 'D']; // Mon–Sun

function dayLetter(scheduledDate: string, dayNumber: number): string {
  const d = new Date(scheduledDate);
  const jsDay = d.getDay(); // 0=Sun
  const idx = jsDay === 0 ? 6 : jsDay - 1; // 0=Mon
  return DAY_LETTERS[idx] ?? `D${dayNumber}`;
}

/**
 * WeekStrip — compact week-navigation bar inspired by Strong/Hevy.
 *
 * Shows day dots with status colors:
 *   green ✓ = Completed
 *   blue ○ = InProgress / today
 *   gray ● = NotStarted
 *   amber - = Skipped
 *   yellow - = PartiallyCompleted
 *
 * ← → arrows to navigate weeks. Tap a day to load it.
 * Must have 44px min touch target per GuiaUI.txt.
 */
export function WeekStrip({
  week,
  activeWorkoutId,
  onSelectDay,
  onPrevWeek,
  onNextWeek,
}: WeekStripProps) {
  const handleDayTap = useCallback(
    (workoutId: string) => {
      haptic(10);
      onSelectDay(workoutId);
    },
    [onSelectDay],
  );

  const isFirstWeek = week.weekNumber <= 1;
  const isLastWeek = week.weekNumber >= week.totalWeeks;

  return (
    <div className="bg-white/80 backdrop-blur-sm border-b border-slate-100">
      {/* Program + week label */}
      <div className="flex items-center justify-between px-3 pt-2 pb-1">
        <p className="text-xs text-slate-500 truncate max-w-[60%]">
          {week.programName}
        </p>
        <span className="text-xs font-mono text-slate-500">
          Sem {week.weekNumber}/{week.totalWeeks}
        </span>
      </div>

      {/* Day dots row */}
      <div className="flex items-center px-1 pb-2">
        {/* Prev arrow */}
        <button
          type="button"
          onClick={onPrevWeek}
          disabled={isFirstWeek}
          className={cn(
            'flex-shrink-0 flex items-center justify-center w-8 h-10 rounded-md transition-colors',
            isFirstWeek
              ? 'text-slate-200 cursor-default'
              : 'text-slate-400 hover:text-slate-600 active:bg-slate-100',
          )}
          aria-label="Semana anterior"
        >
          <ChevronLeft className="w-4 h-4" />
        </button>

        {/* Day buttons */}
        <div className="flex-1 flex justify-around items-center">
          {week.days.map((day) => (
            <DayDot
              key={day.workoutId}
              day={day}
              isActive={day.workoutId === activeWorkoutId}
              onTap={handleDayTap}
            />
          ))}
        </div>

        {/* Next arrow */}
        <button
          type="button"
          onClick={onNextWeek}
          disabled={isLastWeek}
          className={cn(
            'flex-shrink-0 flex items-center justify-center w-8 h-10 rounded-md transition-colors',
            isLastWeek
              ? 'text-slate-200 cursor-default'
              : 'text-slate-400 hover:text-slate-600 active:bg-slate-100',
          )}
          aria-label="Semana siguiente"
        >
          <ChevronRight className="w-4 h-4" />
        </button>
      </div>
    </div>
  );
}

// ─── Individual day dot ──────────────────────────────────────
function DayDot({
  day,
  isActive,
  onTap,
}: {
  day: WeekDay;
  isActive: boolean;
  onTap: (workoutId: string) => void;
}) {
  const cfg = DOT_STATUS[day.status] ?? DOT_STATUS.NotStarted;
  const Icon = cfg.icon;
  const letter = dayLetter(day.scheduledDate, day.dayNumber);

  return (
    <button
      type="button"
      onClick={() => onTap(day.workoutId)}
      className={cn(
        'flex flex-col items-center justify-center gap-0.5',
        'min-w-[44px] min-h-[44px] rounded-lg transition-all',
        isActive && 'bg-slate-100',
        day.isToday && !isActive && 'bg-blue-50/60',
      )}
      aria-label={`${day.dayName ?? `Día ${day.dayNumber}`} — ${day.statusName}`}
      title={day.dayName ?? `Día ${day.dayNumber}`}
    >
      {/* Day letter */}
      <span
        className={cn(
          'text-xs font-semibold leading-none',
          day.isToday ? 'text-blue-600' : 'text-slate-500',
          isActive && 'text-slate-800',
        )}
      >
        {letter}
      </span>

      {/* Status dot */}
      <span
        className={cn(
          'w-5 h-5 rounded-full flex items-center justify-center transition-all',
          cfg.bg,
          isActive && `ring-2 ${cfg.ring}`,
          day.isToday && day.status === 'InProgress' && 'animate-pulse',
        )}
      >
        {Icon && <Icon className="w-3 h-3 text-white" />}
      </span>

      {/* Focus label (tiny, under dot) */}
      {day.focus && (
        <span className="text-[10px] text-slate-400 leading-none truncate max-w-[48px]">
          {day.focus}
        </span>
      )}
    </button>
  );
}
