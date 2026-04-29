'use client';

import React from 'react';
import { Dumbbell, ChevronRight, GitCompareArrows, Check } from 'lucide-react';
import { cn } from '@/utils/cn';
import { ScrollArea } from '@/components/ui/scroll-area';
import { Button } from '@/components/ui/button';

interface ExerciseListItem {
  exerciseId: string;
  exerciseName: string;
  occurrences: number;
}

interface ExerciseListProps {
  exercises: ExerciseListItem[];
  selectedExerciseId: string | null;
  onSelectExercise: (exerciseId: string) => void;
  /** Compare mode support */
  compareMode?: boolean;
  onToggleCompareMode?: () => void;
  compareSelection?: Set<string>;
  onToggleCompareExercise?: (exerciseId: string) => void;
}

export function ExerciseList({
  exercises,
  selectedExerciseId,
  onSelectExercise,
  compareMode = false,
  onToggleCompareMode,
  compareSelection,
  onToggleCompareExercise,
}: ExerciseListProps) {
  if (exercises.length === 0) {
    return (
      <div className="flex flex-col items-center justify-center h-full py-12 px-4 text-center">
        <div className="w-12 h-12 rounded-full bg-muted flex items-center justify-center mb-4">
          <Dumbbell className="h-6 w-6 text-muted-foreground" />
        </div>
        <h3 className="font-medium text-sm mb-1">No exercises yet</h3>
        <p className="text-xs text-muted-foreground">
          Add exercises in the Weekly Builder view first
        </p>
      </div>
    );
  }

  return (
    <div className="h-full flex flex-col">
      <div className="px-4 py-3 border-b">
        <div className="flex items-center justify-between">
          <div>
            <h3 className="font-semibold text-sm">Exercises in Program</h3>
            <p className="text-xs text-muted-foreground mt-0.5">
              {exercises.length} exercise{exercises.length !== 1 ? 's' : ''}
              {compareMode && compareSelection && compareSelection.size > 0 && (
                <span className="text-primary ml-1">
                  • {compareSelection.size} selected
                </span>
              )}
            </p>
          </div>
          {onToggleCompareMode && exercises.length >= 2 && (
            <Button
              variant={compareMode ? 'secondary' : 'ghost'}
              size="sm"
              onClick={onToggleCompareMode}
              className="gap-1 h-7 text-xs"
              title="Comparar ejercicios"
            >
              <GitCompareArrows className="h-3.5 w-3.5" />
              {compareMode ? 'Done' : 'Compare'}
            </Button>
          )}
        </div>
      </div>
      <ScrollArea className="flex-1">
        <div className="p-2 space-y-1">
          {exercises.map((exercise) => {
            const isSelected = selectedExerciseId === exercise.exerciseId;
            const isCompared = compareSelection?.has(exercise.exerciseId) ?? false;

            return (
              <button
                key={exercise.exerciseId}
                onClick={() => {
                  if (compareMode && onToggleCompareExercise) {
                    onToggleCompareExercise(exercise.exerciseId);
                  } else {
                    onSelectExercise(exercise.exerciseId);
                  }
                }}
                className={cn(
                  'w-full flex items-center gap-3 px-3 py-2.5 rounded-lg text-left transition-all',
                  'hover:bg-muted/80',
                  'focus:outline-none focus:ring-2 focus:ring-primary/20',
                  compareMode && isCompared
                    ? 'bg-primary/10 text-primary border border-primary/20'
                    : isSelected && !compareMode
                      ? 'bg-primary/10 text-primary border border-primary/20'
                      : 'text-foreground'
                )}
              >
                {compareMode ? (
                  <div
                    className={cn(
                      'w-5 h-5 rounded border-2 flex items-center justify-center flex-shrink-0 transition-colors',
                      isCompared
                        ? 'bg-primary border-primary text-primary-foreground'
                        : 'border-muted-foreground/40'
                    )}
                  >
                    {isCompared && <Check className="h-3 w-3" />}
                  </div>
                ) : (
                  <div
                    className={cn(
                      'w-8 h-8 rounded-md flex items-center justify-center flex-shrink-0',
                      isSelected ? 'bg-primary/20' : 'bg-muted'
                    )}
                  >
                    <Dumbbell className="h-4 w-4" />
                  </div>
                )}
                <div className="flex-1 min-w-0">
                  <p className="font-medium text-sm truncate">{exercise.exerciseName}</p>
                  <p className="text-xs text-muted-foreground">
                    {exercise.occurrences} week{exercise.occurrences !== 1 ? 's' : ''}
                  </p>
                </div>
                {!compareMode && (
                  <ChevronRight
                    className={cn(
                      'h-4 w-4 flex-shrink-0 transition-transform',
                      isSelected ? 'text-primary' : 'text-muted-foreground'
                    )}
                  />
                )}
              </button>
            );
          })}
        </div>
      </ScrollArea>
    </div>
  );
}
