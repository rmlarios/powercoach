'use client';

import React from 'react';
import { CalendarDays, TrendingUp, LayoutGrid, Calendar } from 'lucide-react';
import { BuilderViewMode } from '@/types/builder';
import { cn } from '@/utils/cn';

interface ViewModeSelectorProps {
  mode: BuilderViewMode;
  onChange: (mode: BuilderViewMode) => void;
}

const viewModes: { value: BuilderViewMode; label: string; icon: React.ReactNode; description: string }[] = [
  {
    value: 'weekly',
    label: 'Weekly',
    icon: <CalendarDays className="h-4 w-4" />,
    description: 'Organize by weeks and days',
  },
  {
    value: 'day-centric',
    label: 'Day View',
    icon: <LayoutGrid className="h-4 w-4" />,
    description: 'Excel-like view by training days',
  },
  {
    value: 'progression',
    label: 'Progression',
    icon: <TrendingUp className="h-4 w-4" />,
    description: 'View exercise progression',
  },
  {
    value: 'calendar',
    label: 'Calendar',
    icon: <Calendar className="h-4 w-4" />,
    description: 'View program on calendar',
  },
];

export function ViewModeSelector({ mode, onChange }: ViewModeSelectorProps) {
  return (
    <div className="inline-flex items-center bg-muted p-1 rounded-lg">
      {viewModes.map((viewMode) => (
        <button
          key={viewMode.value}
          onClick={() => onChange(viewMode.value)}
          className={cn(
            'flex items-center gap-2 px-3 py-1.5 rounded-md text-sm font-medium transition-all',
            'focus:outline-none focus:ring-2 focus:ring-primary/20',
            mode === viewMode.value
              ? 'bg-background text-foreground shadow-sm'
              : 'text-muted-foreground hover:text-foreground'
          )}
        >
          {viewMode.icon}
          <span>{viewMode.label}</span>
        </button>
      ))}
    </div>
  );
}
