'use client';

import { useState } from 'react';
import { X } from 'lucide-react';
import {
  Sheet,
  SheetContent,
  SheetHeader,
  SheetTitle,
} from '@/components/ui/sheet';
import { Input } from '@/components/ui/input';
import { useBuilder } from '@/providers/builder-provider';
import { BuilderExercise } from '@/types/builder';

// Represents one row in the progression table
interface ProgressionRow {
  weekId: string;
  weekNumber: number;
  weekName: string;
  dayId: string;
  dayName: string;
  exerciseRowId: string; // builder exercise id (unique per week/day/exercise)
  exercise: BuilderExercise;
}

export function ExerciseProgressionPanel() {
  const { state, dispatch } = useBuilder();
  const { progressionPanel, weeks } = state;

  // Collect all occurrences of this exercise across all weeks/days
  const rows: ProgressionRow[] = [];

  if (progressionPanel) {
    for (const week of weeks) {
      for (const day of week.days) {
        for (const ex of day.exercises) {
          if (ex.exerciseId === progressionPanel.exerciseId) {
            rows.push({
              weekId: week.id,
              weekNumber: week.weekNumber,
              weekName: week.name || `Week ${week.weekNumber}`,
              dayId: day.id,
              dayName: day.name || `Day ${day.dayNumber}`,
              exerciseRowId: ex.id,
              exercise: ex,
            });
          }
        }
      }
    }
  }

  const handleClose = () => dispatch({ type: 'CLOSE_PROGRESSION_PANEL' });

  const handleUpdate = (weekId: string, dayId: string, exerciseId: string, updates: Partial<BuilderExercise>) => {
    dispatch({
      type: 'UPDATE_EXERCISE',
      payload: { weekId, dayId, exerciseId, updates },
    });
  };

  return (
    <Sheet open={!!progressionPanel} onOpenChange={(open) => !open && handleClose()}>
      <SheetContent side="right" className="w-full sm:max-w-2xl overflow-y-auto p-0">
        <SheetHeader className="px-6 py-4 border-b sticky top-0 bg-background z-10">
          <div className="flex items-center justify-between">
            <SheetTitle className="text-base">
              Progresión — {progressionPanel?.exerciseName ?? ''}
            </SheetTitle>
            <button
              onClick={handleClose}
              className="rounded-md p-1 hover:bg-muted transition-colors"
              aria-label="Cerrar"
            >
              <X className="h-4 w-4 text-muted-foreground" />
            </button>
          </div>
          {rows.length > 0 && (
            <p className="text-xs text-muted-foreground mt-1">
              Este ejercicio aparece en {rows.length} sesión{rows.length !== 1 ? 'es' : ''}.
            </p>
          )}
        </SheetHeader>

        <div className="px-6 py-4">
          {rows.length === 0 ? (
            <div className="py-16 text-center text-muted-foreground text-sm">
              Este ejercicio no aparece en ninguna semana todavía.
            </div>
          ) : (
            <table className="w-full text-sm">
              <thead>
                <tr className="border-b text-xs text-muted-foreground">
                  <th className="text-left pb-2 pr-3 font-medium w-24">Semana</th>
                  <th className="text-left pb-2 pr-3 font-medium w-28">Sesión</th>
                  <th className="text-center pb-2 pr-2 font-medium w-14">Series</th>
                  <th className="text-center pb-2 pr-2 font-medium w-14">Min</th>
                  <th className="text-center pb-2 pr-2 font-medium w-14">Max</th>
                  <th className="text-center pb-2 pr-2 font-medium w-16">%RM</th>
                  <th className="text-center pb-2 pr-2 font-medium w-14">RPE</th>
                  <th className="text-left pb-2 font-medium">Notas</th>
                </tr>
              </thead>
              <tbody>
                {rows.map((row) => (
                  <ProgressionTableRow
                    key={row.exerciseRowId}
                    row={row}
                    onUpdate={(updates) =>
                      handleUpdate(row.weekId, row.dayId, row.exerciseRowId, updates)
                    }
                  />
                ))}
              </tbody>
            </table>
          )}
        </div>
      </SheetContent>
    </Sheet>
  );
}

// ---------------------------------------------------------------------------
// Individual editable row
// ---------------------------------------------------------------------------

interface ProgressionTableRowProps {
  row: ProgressionRow;
  onUpdate: (updates: Partial<BuilderExercise>) => void;
}

function ProgressionTableRow({ row, onUpdate }: ProgressionTableRowProps) {
  const { exercise, weekName, dayName } = row;

  const numCell = (
    key: keyof BuilderExercise,
    placeholder?: string
  ) => {
    const value = exercise[key];
    return (
      <td className="pr-2 py-1.5 text-center">
        <Input
          type="number"
          defaultValue={value !== undefined && value !== null ? String(value) : ''}
          onBlur={(e) => {
            const raw = e.target.value;
            const parsed = raw === '' ? undefined : Number(raw);
            if (parsed !== value) {
              onUpdate({ [key]: parsed });
            }
          }}
          placeholder={placeholder}
          className="h-7 w-full text-center text-xs bg-transparent border-transparent hover:border-input focus:border-input focus:ring-1 focus:ring-primary/30"
        />
      </td>
    );
  };

  return (
    <tr className="border-b last:border-0 hover:bg-muted/40 transition-colors">
      <td className="pr-3 py-1.5 text-xs font-medium whitespace-nowrap">
        {weekName}
      </td>
      <td className="pr-3 py-1.5 text-xs text-muted-foreground whitespace-nowrap">
        {dayName}
      </td>
      {numCell('sets')}
      {numCell('repsMin')}
      {numCell('repsMax')}
      {numCell('percentageRM', '—')}
      {numCell('rpeTarget', '—')}
      <td className="py-1.5">
        <Input
          type="text"
          defaultValue={exercise.notes ?? ''}
          onBlur={(e) => {
            const val = e.target.value || undefined;
            if (val !== exercise.notes) {
              onUpdate({ notes: val });
            }
          }}
          placeholder="Notas..."
          className="h-7 w-full text-xs bg-transparent border-transparent hover:border-input focus:border-input focus:ring-1 focus:ring-primary/30"
        />
      </td>
    </tr>
  );
}
