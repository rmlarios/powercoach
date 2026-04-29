'use client';

import { useCallback, useMemo, useState } from 'react';
import { useAuth } from '@/providers';
import {
  useTodayWorkout,
  useWorkoutDetail,
  useStartWorkout,
  useSkipWorkout,
  useCompleteWorkout,
  useCompleteSet,
  useWeekWorkouts,
  useExerciseLiftHistory,
} from '@/hooks';
import { useAutoSave } from './lib/workout-utils';
import { WorkoutHeader } from './components/workout-header';
import { ExerciseCard } from './components/exercise-card';
import { EmptyWorkout } from './components/empty-workout';
import { WorkoutSkeleton } from './components/workout-skeleton';
import { SkipDialog, CompleteDialog } from './components/workout-dialogs';
import { SaveIndicator } from './components/save-indicator';
import { WeekStrip } from './components/week-strip';
import { ExerciseDetailSheet } from './components/exercise-detail-sheet';
import { WorkoutSummary } from './components/workout-summary';
import type { TodayWorkout, SaveSetRequest } from '@/types';

/**
 * Workout Tracking Page
 *
 * Design principles (GuiaUI.txt):
 * - "Paper feel": vertical notebook layout, no heavy borders
 * - Monospace data, Inter labels
 * - Inline editing — no modals for data entry
 * - Auto-save with debounce — no "Save" button
 * - 44px touch zones, haptic on check
 * - Auto-focus next set after completing one
 */
export default function WorkoutPage() {
  const { user } = useAuth();
  const athleteId = user?.athleteId ?? '';

  // ── Real API hooks ──
  const { data: apiWorkout, isLoading } = useTodayWorkout(athleteId);
  const startMutation = useStartWorkout();
  const skipMutation = useSkipWorkout();
  const completeMutation = useCompleteWorkout();
  const completeSetMutation = useCompleteSet();

  // Track which day is selected in the WeekStrip (for navigation)
  const [selectedWorkoutId, setSelectedWorkoutId] = useState<string | null>(null);

  // Effective workout: respects day selection via real API
  const { data: selectedDayWorkout } = useWorkoutDetail(
    athleteId,
    selectedWorkoutId && selectedWorkoutId !== apiWorkout?.workoutId ? selectedWorkoutId : '',
  );
  const workout: TodayWorkout | null | undefined =
    selectedWorkoutId && selectedWorkoutId !== apiWorkout?.workoutId
      ? selectedDayWorkout
      : apiWorkout;
  const workoutId = workout?.workoutId ?? '';

  const { save: autoSave, flush, isSaving } = useAutoSave(
    athleteId,
    workoutId,
  );

  // Dialog state
  const [showSkip, setShowSkip] = useState(false);
  const [showComplete, setShowComplete] = useState(false);

  // Focus management
  const [focusTarget, setFocusTarget] = useState<{ exerciseIndex: number; setIndex: number }>({
    exerciseIndex: 0,
    setIndex: 0,
  });

  // ── Week Strip state ──
  const [weekNumber, setWeekNumber] = useState<number | undefined>(undefined);
  const { data: weekData } = useWeekWorkouts(athleteId, weekNumber);

  const handlePrevWeek = useCallback(() => {
    const current = weekData?.weekNumber ?? 1;
    if (current > 1) setWeekNumber(current - 1);
  }, [weekData]);

  const handleNextWeek = useCallback(() => {
    const current = weekData?.weekNumber ?? 1;
    const total = weekData?.totalWeeks ?? 1;
    if (current < total) setWeekNumber(current + 1);
  }, [weekData]);

  const handleSelectDay = useCallback(
    (dayWorkoutId: string) => {
      setSelectedWorkoutId(dayWorkoutId);
      setFocusTarget({ exerciseIndex: 0, setIndex: 0 });
    },
    [],
  );

  // ── Exercise Detail Sheet state ──
  const [detailExerciseId, setDetailExerciseId] = useState<string | null>(null);
  const { data: liftData, isLoading: isLiftHistoryLoading } = useExerciseLiftHistory(
    athleteId,
    detailExerciseId,
  );

  const handleExerciseNameTap = useCallback((exerciseId: string) => {
    setDetailExerciseId(exerciseId);
  }, []);

  const handleDetailSheetClose = useCallback((open: boolean) => {
    if (!open) setDetailExerciseId(null);
  }, []);

  // Elapsed minutes for complete dialog
  const elapsedMinutes = useMemo(() => {
    if (!workout?.startedAt) return undefined;
    return Math.floor((Date.now() - new Date(workout.startedAt).getTime()) / 60000);
  }, [workout?.startedAt]);

  // ── Handlers ──
  const handleStart = useCallback(() => {
    if (!workoutId) return;
    startMutation.mutate({ athleteId, workoutId });
  }, [athleteId, workoutId, startMutation]);

  const handleSkip = useCallback(
    (reason?: string) => {
      if (!workoutId) return;
      skipMutation.mutate({ athleteId, workoutId, data: reason ? { reason } : undefined });
      setShowSkip(false);
    },
    [athleteId, workoutId, skipMutation],
  );

  const handleComplete = useCallback(
    (data: { durationMinutes?: number; fatigueRating?: number; notes?: string }) => {
      if (!workoutId) return;
      flush();
      completeMutation.mutate({ athleteId, workoutId, data });
      setShowComplete(false);
    },
    [athleteId, workoutId, completeMutation, flush],
  );

  const handleSaveSet = useCallback(
    (data: SaveSetRequest) => {
      autoSave(data);
    },
    [autoSave],
  );

  const handleCompleteSet = useCallback(
    (setId: string) => {
      if (!workoutId || !setId) return;
      completeSetMutation.mutate({ athleteId, workoutId, exerciseLogId: setId });
    },
    [athleteId, workoutId, completeSetMutation],
  );

  const handleUncompleteSet = useCallback(
    (setId: string) => {
      const exercise = workout?.exercises.find((ex) => ex.sets.some((s) => s.id === setId));
      const set = exercise?.sets.find((s) => s.id === setId);
      if (!exercise || !set) return;
      autoSave({
        exerciseLogId: setId,
        exerciseId: exercise.exerciseId,
        setNumber: set.setNumber,
        reps: set.actualReps,
        weight: set.actualWeight,
        rpe: set.actualRpe,
        isCompleted: false,
      });
    },
    [workout, autoSave],
  );

  const handleFocusNextExercise = useCallback(
    (currentExerciseIndex: number) => {
      const exercises = workout?.exercises ?? [];
      if (currentExerciseIndex + 1 < exercises.length) {
        setFocusTarget({ exerciseIndex: currentExerciseIndex + 1, setIndex: 0 });
      }
    },
    [workout?.exercises],
  );

  // Loading state
  if (isLoading) return <WorkoutSkeleton />;

  // No workout today
  if (!workout) return <EmptyWorkout />;;

  return (
    <div className="max-w-lg mx-auto pb-24">

      {/* Week Strip — sticky below header */}
      {weekData && (
        <div className="sticky top-0 z-20">
          <WeekStrip
            week={weekData}
            activeWorkoutId={workoutId}
            onSelectDay={handleSelectDay}
            onPrevWeek={handlePrevWeek}
            onNextWeek={handleNextWeek}
          />
        </div>
      )}

      {/* Header: status, timer, action buttons */}
      <WorkoutHeader
        workout={workout}
        onStart={handleStart}
        onSkip={() => setShowSkip(true)}
        onComplete={() => setShowComplete(true)}
        isStarting={startMutation.isPending}
      />

      {/* Save indicator */}
      <div className="sticky top-0 z-10 flex justify-end px-3 py-1">
        <SaveIndicator isSaving={isSaving} />
      </div>

      {/* Workout Summary — shown after completion */}
      {workout.status === 'Completed' && (
        <WorkoutSummary
          exercises={workout.exercises}
          durationMinutes={workout.durationMinutes}
          fatigueRating={workout.fatigueRating}
          completedDate={workout.completedDate}
        />
      )}

      {/* Exercise list — notebook layout */}
      <div className="divide-y divide-slate-100">
        {workout.exercises.map((exercise, i) => (
          <ExerciseCard
            key={exercise.exerciseId}
            exercise={exercise}
            exerciseIndex={i}
            focusSetIndex={focusTarget.exerciseIndex === i ? focusTarget.setIndex : -1}
            onSaveSet={handleSaveSet}
            onCompleteSet={handleCompleteSet}
            onUncompleteSet={handleUncompleteSet}
            onFocusNextExercise={() => handleFocusNextExercise(i)}
            onExerciseNameTap={handleExerciseNameTap}
          />
        ))}
      </div>

      {/* Dialogs */}
      <SkipDialog
        open={showSkip}
        onConfirm={handleSkip}
        onCancel={() => setShowSkip(false)}
      />
      <CompleteDialog
        open={showComplete}
        onConfirm={handleComplete}
        onCancel={() => setShowComplete(false)}
        elapsedMinutes={elapsedMinutes}
      />

      {/* Exercise Detail Bottom Sheet */}
      <ExerciseDetailSheet
        open={detailExerciseId !== null}
        onOpenChange={handleDetailSheetClose}
        data={liftData}
        isLoading={isLiftHistoryLoading}
      />
    </div>
  );
}


