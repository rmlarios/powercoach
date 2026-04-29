'use client';

import React, { useState, useCallback, useRef, KeyboardEvent } from 'react';
import { Sparkles, TrendingUp, Dumbbell } from 'lucide-react';
import { Input } from '@/components/ui/input';
import { Button } from '@/components/ui/button';
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
  DialogTrigger,
} from '@/components/ui/dialog';
import { Label } from '@/components/ui/label';
import { ExerciseProgressionInstance } from '@/types/builder';
import { cn } from '@/utils/cn';

interface ProgressionTableProps {
  exerciseName: string;
  instances: ExerciseProgressionInstance[];
  totalWeeks: number;
  recentlyUpdatedWeeks?: Set<number>;
  showHeader?: boolean;
  onUpdateInstance: (
    weekId: string,
    dayId: string,
    exerciseEntryId: string,
    updates: Partial<ExerciseProgressionInstance>
  ) => void;
  onAutoProgress?: (startPercent: number, endPercent: number) => void;
}

interface EditableCellProps {
  value: string | number | undefined;
  type: 'number' | 'text';
  placeholder?: string;
  className?: string;
  suffix?: string;
  onChange: (value: string | number | undefined) => void;
  onMoveNext?: () => void;
  onMovePrev?: () => void;
  inputRef?: React.RefObject<HTMLInputElement>;
}

function EditableCell({
  value,
  type,
  placeholder,
  className,
  suffix,
  onChange,
  onMoveNext,
  onMovePrev,
  inputRef,
}: EditableCellProps) {
  const [isEditing, setIsEditing] = useState(false);
  const [editValue, setEditValue] = useState(String(value ?? ''));
  const localRef = useRef<HTMLInputElement>(null);
  const ref = inputRef || localRef;

  const handleBlur = () => {
    setIsEditing(false);
    if (type === 'number') {
      const numValue = editValue === '' ? undefined : parseFloat(editValue);
      if (numValue !== value) {
        onChange(numValue);
      }
    } else {
      if (editValue !== value) {
        onChange(editValue);
      }
    }
  };

  const handleKeyDown = (e: KeyboardEvent<HTMLInputElement>) => {
    if (e.key === 'Enter' || e.key === 'Tab') {
      e.preventDefault();
      handleBlur();
      if (e.key === 'Tab' && e.shiftKey) {
        onMovePrev?.();
      } else {
        onMoveNext?.();
      }
    } else if (e.key === 'Escape') {
      setEditValue(String(value ?? ''));
      setIsEditing(false);
    } else if (e.key === 'ArrowDown') {
      e.preventDefault();
      handleBlur();
      onMoveNext?.();
    } else if (e.key === 'ArrowUp') {
      e.preventDefault();
      handleBlur();
      onMovePrev?.();
    }
  };

  const displayValue = value !== undefined && value !== null ? `${value}${suffix || ''}` : '';

  return (
    <div className={cn('relative group', className)}>
      {isEditing ? (
        <Input
          ref={ref}
          type={type}
          value={editValue}
          onChange={(e) => setEditValue(e.target.value)}
          onBlur={handleBlur}
          onKeyDown={handleKeyDown}
          className="h-8 text-sm text-center"
          autoFocus
        />
      ) : (
        <div
          onClick={() => {
            setIsEditing(true);
            setTimeout(() => ref.current?.focus(), 0);
          }}
          className={cn(
            'h-8 flex items-center justify-center text-sm cursor-text rounded-md',
            'hover:bg-muted/50 transition-colors',
            !displayValue && 'text-muted-foreground'
          )}
        >
          {displayValue || placeholder || '—'}
        </div>
      )}
    </div>
  );
}

export function ProgressionTable({
  exerciseName,
  instances,
  totalWeeks,
  recentlyUpdatedWeeks = new Set(),
  showHeader = true,
  onUpdateInstance,
  onAutoProgress,
}: ProgressionTableProps) {
  const [autoProgressOpen, setAutoProgressOpen] = useState(false);
  const [startPercent, setStartPercent] = useState(60);
  const [endPercent, setEndPercent] = useState(85);

  const handleAutoProgress = useCallback(() => {
    onAutoProgress?.(startPercent, endPercent);
    setAutoProgressOpen(false);
  }, [onAutoProgress, startPercent, endPercent]);

  // Create a map of week numbers to instances for easy lookup
  const instancesByWeek = new Map<number, ExerciseProgressionInstance>();
  instances.forEach((instance) => {
    instancesByWeek.set(instance.weekNumber, instance);
  });

  // Generate all weeks (even empty ones)
  const allWeeks = Array.from({ length: totalWeeks }, (_, i) => i + 1);

  const columns = [
    { key: 'week', label: 'Week', width: 'w-16' },
    { key: 'sets', label: 'Sets', width: 'w-16' },
    { key: 'reps', label: 'Reps', width: 'w-24' },
    { key: 'percentageRM', label: '%RM', width: 'w-16' },
    { key: 'rpeTarget', label: 'RPE', width: 'w-16' },
    { key: 'restSeconds', label: 'Rest', width: 'w-20' },
    { key: 'notes', label: 'Notes', width: 'flex-1 min-w-[120px]' },
  ];

  if (instances.length === 0) {
    return (
      <div className="flex flex-col items-center justify-center h-full py-20 text-center">
        <div className="w-16 h-16 rounded-full bg-muted flex items-center justify-center mb-4">
          <Dumbbell className="h-8 w-8 text-muted-foreground" />
        </div>
        <h3 className="font-semibold text-lg mb-2">Select an exercise</h3>
        <p className="text-sm text-muted-foreground max-w-md">
          Choose an exercise from the list to view and edit its progression across all weeks
        </p>
      </div>
    );
  }

  return (
    <div className="h-full flex flex-col">
      {/* Header */}
      {showHeader && (
        <div className="px-6 py-4 border-b flex items-center justify-between">
          <div>
            <h2 className="font-semibold text-lg">{exerciseName}</h2>
            <p className="text-sm text-muted-foreground mt-0.5">
              {instances.length} of {totalWeeks} weeks programmed
            </p>
          </div>
          {onAutoProgress && (
            <Dialog open={autoProgressOpen} onOpenChange={setAutoProgressOpen}>
              <DialogTrigger asChild>
                <Button variant="outline" size="sm" className="gap-2">
                  <Sparkles className="h-4 w-4" />
                  Auto Progression
                </Button>
              </DialogTrigger>
              <DialogContent className="sm:max-w-md">
                <DialogHeader>
                  <DialogTitle className="flex items-center gap-2">
                    <TrendingUp className="h-5 w-5 text-primary" />
                    Auto Progression
                  </DialogTitle>
                  <DialogDescription>
                    Automatically generate a linear progression for {exerciseName} across all weeks.
                  </DialogDescription>
                </DialogHeader>
                <div className="grid gap-4 py-4">
                  <div className="grid grid-cols-2 gap-4">
                    <div className="space-y-2">
                      <Label htmlFor="startPercent">Start %RM</Label>
                      <Input
                        id="startPercent"
                        type="number"
                        value={startPercent}
                        onChange={(e) => setStartPercent(Number(e.target.value))}
                        min={0}
                        max={100}
                      />
                    </div>
                    <div className="space-y-2">
                      <Label htmlFor="endPercent">End %RM</Label>
                      <Input
                        id="endPercent"
                        type="number"
                        value={endPercent}
                        onChange={(e) => setEndPercent(Number(e.target.value))}
                        min={0}
                        max={100}
                      />
                    </div>
                  </div>
                  <div className="p-3 bg-muted rounded-lg">
                    <p className="text-sm text-muted-foreground">
                      <strong>Preview:</strong> Week 1 at {startPercent}% → Week {totalWeeks} at {endPercent}%
                    </p>
                    <p className="text-xs text-muted-foreground mt-1">
                      Increment: ~{((endPercent - startPercent) / Math.max(totalWeeks - 1, 1)).toFixed(1)}% per week
                    </p>
                  </div>
                </div>
                <DialogFooter>
                  <Button variant="outline" onClick={() => setAutoProgressOpen(false)}>
                    Cancel
                  </Button>
                  <Button onClick={handleAutoProgress}>
                    Apply Progression
                  </Button>
                </DialogFooter>
              </DialogContent>
            </Dialog>
          )}
        </div>
      )}

      {/* Table */}
      <div className="flex-1 overflow-auto">
        <table className="w-full">
          <thead className="sticky top-0 bg-background border-b">
            <tr>
              {columns.map((col) => (
                <th
                  key={col.key}
                  className={cn(
                    'px-3 py-2 text-left text-xs font-medium text-muted-foreground uppercase tracking-wider',
                    col.width
                  )}
                >
                  {col.label}
                </th>
              ))}
            </tr>
          </thead>
          <tbody className="divide-y">
            {allWeeks.map((weekNumber) => {
              const instance = instancesByWeek.get(weekNumber);
              const hasData = !!instance;
              const isRecentlyUpdated = recentlyUpdatedWeeks.has(weekNumber);

              return (
                <tr
                  key={weekNumber}
                  className={cn(
                    'group transition-all duration-300',
                    hasData ? 'hover:bg-muted/30' : 'bg-muted/10',
                    isRecentlyUpdated && 'bg-primary/10 animate-pulse'
                  )}
                >
                  {/* Week Number */}
                  <td className="px-3 py-2">
                    <div className="flex items-center gap-2">
                      <span
                        className={cn(
                          'font-medium text-sm',
                          hasData ? 'text-foreground' : 'text-muted-foreground'
                        )}
                      >
                        {weekNumber}
                      </span>
                      {!hasData && (
                        <span className="text-xs text-muted-foreground">(Not scheduled)</span>
                      )}
                    </div>
                  </td>

                  {/* Sets */}
                  <td className="px-3 py-2">
                    {hasData ? (
                      <EditableCell
                        value={instance.sets}
                        type="number"
                        onChange={(v) =>
                          onUpdateInstance(
                            instance.weekId,
                            instance.dayId,
                            instance.exerciseEntryId,
                            { sets: v as number }
                          )
                        }
                      />
                    ) : (
                      <div className="h-8 flex items-center justify-center text-sm text-muted-foreground">
                        —
                      </div>
                    )}
                  </td>

                  {/* Reps */}
                  <td className="px-3 py-2">
                    {hasData ? (
                      <div className="flex items-center gap-1">
                        <EditableCell
                          value={instance.repsMin}
                          type="number"
                          className="w-12"
                          onChange={(v) =>
                            onUpdateInstance(
                              instance.weekId,
                              instance.dayId,
                              instance.exerciseEntryId,
                              { repsMin: v as number }
                            )
                          }
                        />
                        <span className="text-muted-foreground text-sm">-</span>
                        <EditableCell
                          value={instance.repsMax}
                          type="number"
                          className="w-12"
                          onChange={(v) =>
                            onUpdateInstance(
                              instance.weekId,
                              instance.dayId,
                              instance.exerciseEntryId,
                              { repsMax: v as number }
                            )
                          }
                        />
                      </div>
                    ) : (
                      <div className="h-8 flex items-center justify-center text-sm text-muted-foreground">
                        —
                      </div>
                    )}
                  </td>

                  {/* %RM */}
                  <td className="px-3 py-2">
                    {hasData ? (
                      <EditableCell
                        value={instance.percentageRM}
                        type="number"
                        suffix="%"
                        placeholder="—"
                        onChange={(v) =>
                          onUpdateInstance(
                            instance.weekId,
                            instance.dayId,
                            instance.exerciseEntryId,
                            { percentageRM: v as number }
                          )
                        }
                      />
                    ) : (
                      <div className="h-8 flex items-center justify-center text-sm text-muted-foreground">
                        —
                      </div>
                    )}
                  </td>

                  {/* RPE */}
                  <td className="px-3 py-2">
                    {hasData ? (
                      <EditableCell
                        value={instance.rpeTarget}
                        type="number"
                        placeholder="—"
                        onChange={(v) =>
                          onUpdateInstance(
                            instance.weekId,
                            instance.dayId,
                            instance.exerciseEntryId,
                            { rpeTarget: v as number }
                          )
                        }
                      />
                    ) : (
                      <div className="h-8 flex items-center justify-center text-sm text-muted-foreground">
                        —
                      </div>
                    )}
                  </td>

                  {/* Rest */}
                  <td className="px-3 py-2">
                    {hasData ? (
                      <EditableCell
                        value={instance.restSeconds}
                        type="number"
                        suffix="s"
                        onChange={(v) =>
                          onUpdateInstance(
                            instance.weekId,
                            instance.dayId,
                            instance.exerciseEntryId,
                            { restSeconds: v as number }
                          )
                        }
                      />
                    ) : (
                      <div className="h-8 flex items-center justify-center text-sm text-muted-foreground">
                        —
                      </div>
                    )}
                  </td>

                  {/* Notes */}
                  <td className="px-3 py-2">
                    {hasData ? (
                      <EditableCell
                        value={instance.notes}
                        type="text"
                        placeholder="Add notes..."
                        className="text-left"
                        onChange={(v) =>
                          onUpdateInstance(
                            instance.weekId,
                            instance.dayId,
                            instance.exerciseEntryId,
                            { notes: v as string }
                          )
                        }
                      />
                    ) : (
                      <div className="h-8 flex items-center text-sm text-muted-foreground">
                        —
                      </div>
                    )}
                  </td>
                </tr>
              );
            })}
          </tbody>
        </table>
      </div>
    </div>
  );
}
