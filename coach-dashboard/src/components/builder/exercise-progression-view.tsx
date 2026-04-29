'use client';

import React, { useMemo, useState, useCallback } from 'react';
import { BarChart3 } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { ExerciseList } from './exercise-list';
import { ProgressionTable } from './progression-table';
import { ProgressionGenerator } from './progression-generator';
import { ProgressionChart, MultiExerciseChart } from '@/components/charts';
import type { ExerciseSeries } from '@/components/charts';
import { BuilderWeek, ExerciseProgressionInstance, BuilderExercise, GeneratedWeekData } from '@/types/builder';

interface ExerciseProgressionViewProps {
  weeks: BuilderWeek[];
  totalWeeks: number;
  athleteRM?: Record<string, number>; // Map of exerciseId -> 1RM value
  onUpdateExercise: (
    weekId: string,
    dayId: string,
    exerciseId: string,
    updates: Partial<BuilderExercise>
  ) => void;
}

interface ExerciseListItem {
  exerciseId: string;
  exerciseName: string;
  occurrences: number;
}

export function ExerciseProgressionView({
  weeks,
  totalWeeks,
  athleteRM,
  onUpdateExercise,
}: ExerciseProgressionViewProps) {
  const [selectedExerciseId, setSelectedExerciseId] = useState<string | null>(null);
  const [recentlyUpdatedWeeks, setRecentlyUpdatedWeeks] = useState<Set<number>>(new Set());
  const [showChart, setShowChart] = useState(true);
  const [compareMode, setCompareMode] = useState(false);
  const [compareSelection, setCompareSelection] = useState<Set<string>>(new Set());

  // Extract unique exercises from all weeks/days
  const exerciseList = useMemo<ExerciseListItem[]>(() => {
    const exerciseMap = new Map<string, ExerciseListItem>();

    weeks.forEach((week) => {
      week.days.forEach((day) => {
        day.exercises.forEach((exercise) => {
          const key = exercise.exerciseId || exercise.exerciseName;
          const existing = exerciseMap.get(key);
          if (existing) {
            existing.occurrences++;
          } else {
            exerciseMap.set(key, {
              exerciseId: key,
              exerciseName: exercise.exerciseName,
              occurrences: 1,
            });
          }
        });
      });
    });

    return Array.from(exerciseMap.values()).sort((a, b) =>
      a.exerciseName.localeCompare(b.exerciseName)
    );
  }, [weeks]);

  // Auto-select first exercise if none selected
  React.useEffect(() => {
    if (!selectedExerciseId && exerciseList.length > 0) {
      setSelectedExerciseId(exerciseList[0].exerciseId);
    }
  }, [selectedExerciseId, exerciseList]);

  // Get progression instances for selected exercise
  const { selectedExercise, progressionInstances } = useMemo(() => {
    if (!selectedExerciseId) {
      return { selectedExercise: null, progressionInstances: [] };
    }

    const selected = exerciseList.find((e) => e.exerciseId === selectedExerciseId);
    const instances: ExerciseProgressionInstance[] = [];

    weeks.forEach((week) => {
      week.days.forEach((day) => {
        day.exercises.forEach((exercise) => {
          const key = exercise.exerciseId || exercise.exerciseName;
          if (key === selectedExerciseId) {
            instances.push({
              weekId: week.id,
              weekNumber: week.weekNumber,
              dayId: day.id,
              dayName: day.name,
              exerciseEntryId: exercise.id,
              sets: exercise.sets,
              repsMin: exercise.repsMin,
              repsMax: exercise.repsMax,
              rpeTarget: exercise.rpeTarget,
              // eslint-disable-next-line @typescript-eslint/no-explicit-any
              percentageRM: (exercise as any).percentageRM, // May need to add this field
              restSeconds: exercise.restSeconds,
              notes: exercise.notes,
            });
          }
        });
      });
    });

    // Sort by week number
    instances.sort((a, b) => a.weekNumber - b.weekNumber);

    return { selectedExercise: selected, progressionInstances: instances };
  }, [selectedExerciseId, weeks, exerciseList]);

  // Handle instance update
  const handleUpdateInstance = useCallback(
    (
      weekId: string,
      dayId: string,
      exerciseEntryId: string,
      updates: Partial<ExerciseProgressionInstance>
    ) => {
      // Map the progression instance updates to builder exercise updates
      const exerciseUpdates: Partial<BuilderExercise> = {};

      if (updates.sets !== undefined) exerciseUpdates.sets = updates.sets;
      if (updates.repsMin !== undefined) exerciseUpdates.repsMin = updates.repsMin;
      if (updates.repsMax !== undefined) exerciseUpdates.repsMax = updates.repsMax;
      if (updates.rpeTarget !== undefined) exerciseUpdates.rpeTarget = updates.rpeTarget;
      if (updates.restSeconds !== undefined) exerciseUpdates.restSeconds = updates.restSeconds;
      if (updates.notes !== undefined) exerciseUpdates.notes = updates.notes;
      // Handle percentageRM if it's added to the model
      if (updates.percentageRM !== undefined) {
        // eslint-disable-next-line @typescript-eslint/no-explicit-any
        (exerciseUpdates as any).percentageRM = updates.percentageRM;
      }

      onUpdateExercise(weekId, dayId, exerciseEntryId, exerciseUpdates);
    },
    [onUpdateExercise]
  );

  // Handle auto progression
  const handleAutoProgress = useCallback(
    (startPercent: number, endPercent: number) => {
      if (!progressionInstances.length) return;

      const increment = (endPercent - startPercent) / Math.max(progressionInstances.length - 1, 1);
      const updatedWeeks = new Set<number>();

      progressionInstances.forEach((instance, index) => {
        const percentageRM = Math.round(startPercent + increment * index);
        handleUpdateInstance(instance.weekId, instance.dayId, instance.exerciseEntryId, {
          percentageRM,
        });
        updatedWeeks.add(instance.weekNumber);
      });

      // Trigger animation
      setRecentlyUpdatedWeeks(updatedWeeks);
      setTimeout(() => setRecentlyUpdatedWeeks(new Set()), 1500);
    },
    [progressionInstances, handleUpdateInstance]
  );

  // Handle generated progression from ProgressionGenerator
  const handleGeneratedProgression = useCallback(
    (generatedWeeks: GeneratedWeekData[]) => {
      if (!progressionInstances.length) return;

      const updatedWeeks = new Set<number>();

      // Map generated weeks to existing instances
      generatedWeeks.forEach((generated) => {
        const instance = progressionInstances.find(
          (inst) => inst.weekNumber === generated.weekNumber
        );
        if (instance) {
          handleUpdateInstance(instance.weekId, instance.dayId, instance.exerciseEntryId, {
            sets: generated.sets,
            repsMin: generated.repsMin,
            repsMax: generated.repsMax,
            percentageRM: generated.percentageRM,
            rpeTarget: generated.rpeTarget,
            restSeconds: generated.restSeconds,
          });
          updatedWeeks.add(instance.weekNumber);
        }
      });

      // Trigger animation for visual feedback
      setRecentlyUpdatedWeeks(updatedWeeks);
      setTimeout(() => setRecentlyUpdatedWeeks(new Set()), 1500);
    },
    [progressionInstances, handleUpdateInstance]
  );

  // Get current exercise's 1RM if available
  const currentExerciseRM = selectedExerciseId && athleteRM 
    ? athleteRM[selectedExerciseId] 
    : undefined;

  // Toggle compare mode
  const handleToggleCompareMode = useCallback(() => {
    setCompareMode((prev) => {
      if (!prev && selectedExerciseId) {
        // Entering compare mode: pre-select current exercise
        setCompareSelection(new Set([selectedExerciseId]));
      } else {
        // Leaving compare mode: clear selection
        setCompareSelection(new Set());
      }
      return !prev;
    });
  }, [selectedExerciseId]);

  // Toggle an exercise in compare selection
  const handleToggleCompareExercise = useCallback((exerciseId: string) => {
    setCompareSelection((prev) => {
      const next = new Set(prev);
      if (next.has(exerciseId)) {
        next.delete(exerciseId);
      } else {
        next.add(exerciseId);
      }
      return next;
    });
  }, []);

  // Build comparison series data
  const compareSeries = useMemo<ExerciseSeries[]>(() => {
    if (!compareMode || compareSelection.size === 0) return [];

    return Array.from(compareSelection).map((exId) => {
      const exerciseInfo = exerciseList.find((e) => e.exerciseId === exId);
      const instances: ExerciseProgressionInstance[] = [];

      weeks.forEach((week) => {
        week.days.forEach((day) => {
          day.exercises.forEach((exercise) => {
            const key = exercise.exerciseId || exercise.exerciseName;
            if (key === exId) {
              instances.push({
                weekId: week.id,
                weekNumber: week.weekNumber,
                dayId: day.id,
                dayName: day.name,
                exerciseEntryId: exercise.id,
                sets: exercise.sets,
                repsMin: exercise.repsMin,
                repsMax: exercise.repsMax,
                rpeTarget: exercise.rpeTarget,
                // eslint-disable-next-line @typescript-eslint/no-explicit-any
                percentageRM: (exercise as any).percentageRM,
                restSeconds: exercise.restSeconds,
                notes: exercise.notes,
              });
            }
          });
        });
      });

      instances.sort((a, b) => a.weekNumber - b.weekNumber);

      return {
        exerciseId: exId,
        exerciseName: exerciseInfo?.exerciseName ?? exId,
        instances,
        oneRM: athleteRM ? athleteRM[exId] : undefined,
      };
    });
  }, [compareMode, compareSelection, exerciseList, weeks, athleteRM]);

  return (
    <div className="h-[calc(100vh-200px)] flex border rounded-xl overflow-hidden bg-background">
      {/* Left Panel - Exercise List */}
      <div className="w-72 border-r bg-muted/20 flex-shrink-0">
        <ExerciseList
          exercises={exerciseList}
          selectedExerciseId={selectedExerciseId}
          onSelectExercise={setSelectedExerciseId}
          compareMode={compareMode}
          onToggleCompareMode={handleToggleCompareMode}
          compareSelection={compareSelection}
          onToggleCompareExercise={handleToggleCompareExercise}
        />
      </div>

      {/* Right Panel - Progression Table */}
      <div className="flex-1 min-w-0 flex flex-col">
        {/* Generator Header */}
        {selectedExercise && (
          <div className="px-6 py-4 border-b flex items-center justify-between bg-background">
            <div>
              <h2 className="font-semibold text-lg">{selectedExercise.exerciseName}</h2>
              <p className="text-sm text-muted-foreground">
                {progressionInstances.length} of {totalWeeks} weeks programmed
                {currentExerciseRM && (
                  <span className="ml-2 text-primary">• 1RM: {currentExerciseRM}kg</span>
                )}
              </p>
            </div>
            <div className="flex items-center gap-2">
              <Button
                variant={showChart ? 'secondary' : 'outline'}
                size="sm"
                onClick={() => setShowChart(!showChart)}
                className="gap-1.5"
              >
                <BarChart3 className="h-4 w-4" />
                {showChart ? 'Ocultar gráfico' : 'Ver gráfico'}
              </Button>
              <ProgressionGenerator
                exerciseId={selectedExerciseId || ''}
                exerciseName={selectedExercise.exerciseName}
                totalWeeks={totalWeeks}
                athleteRM={currentExerciseRM}
                onGenerate={handleGeneratedProgression}
              />
            </div>
          </div>
        )}
        
        {/* Comparison Chart (compare mode) */}
        {compareMode && compareSeries.length >= 2 && (
          <div className="px-6 py-4 border-b bg-muted/20">
            <MultiExerciseChart series={compareSeries} />
          </div>
        )}

        {compareMode && compareSeries.length < 2 && (
          <div className="px-6 py-8 border-b bg-muted/20 text-center text-sm text-muted-foreground">
            Selecciona al menos 2 ejercicios para comparar
          </div>
        )}

        {/* Single Exercise Chart (normal mode) */}
        {!compareMode && showChart && selectedExercise && progressionInstances.length > 0 && (
          <div className="px-6 py-4 border-b bg-muted/20">
            <ProgressionChart
              exerciseName={selectedExercise.exerciseName}
              instances={progressionInstances}
              oneRM={currentExerciseRM}
            />
          </div>
        )}
        
        {/* Table */}
        <div className="flex-1 overflow-auto">
          <ProgressionTable
            exerciseName={selectedExercise?.exerciseName || ''}
            instances={progressionInstances}
            totalWeeks={totalWeeks}
            recentlyUpdatedWeeks={recentlyUpdatedWeeks}
            onUpdateInstance={handleUpdateInstance}
            onAutoProgress={handleAutoProgress}
            showHeader={false}
          />
        </div>
      </div>
    </div>
  );
}
