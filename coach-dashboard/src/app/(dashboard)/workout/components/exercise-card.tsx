'use client';

import { useCallback, useEffect, useRef, useState } from 'react';
import { Timer, Clock, Layers, TrendingDown, RotateCw } from 'lucide-react';
import { cn } from '@/lib/utils';
import { SetRow } from './set-row';
import { RestTimer } from './rest-timer';
import { CoachNote } from './coach-note';
import { PreviousPerformanceBadge } from './previous-performance';
import type { WorkoutExerciseGroup, WorkoutSet, SaveSetRequest } from '@/types';

interface ExerciseCardProps {
  exercise: WorkoutExerciseGroup;
  /** Index of the current exercise within the workout (for first-set auto-focus). */
  exerciseIndex: number;
  /** Index of the set that should auto-focus, or -1 for none. */
  focusSetIndex: number;
  onSaveSet: (data: SaveSetRequest) => void;
  onCompleteSet: (setId: string) => void;
  onUncompleteSet: (setId: string) => void;
  /** Called when the last set of this exercise wants to move focus forward. */
  onFocusNextExercise: () => void;
  /** Called when the exercise name is tapped — opens detail sheet. */
  onExerciseNameTap?: (exerciseId: string) => void;
}

// ─── Exercise type badge config ─────────────────────────────
const TYPE_BADGE: Record<string, { label: string; extraLabel?: string; color: string; icon: React.ComponentType<{ className?: string }> }> = {
  Emom: { label: 'EMOM', color: 'bg-blue-100 text-blue-700', icon: Timer },
  Tempo: { label: 'TEMPO', color: 'bg-purple-100 text-purple-700', icon: Clock },
  Superset: { label: 'SS', color: 'bg-orange-100 text-orange-700', icon: Layers },
  Circuit: { label: 'CIRCUIT', color: 'bg-green-100 text-green-700', icon: RotateCw },
  Dropset: { label: 'DROP', color: 'bg-red-100 text-red-700', icon: TrendingDown },
};

function ExerciseTypeBadge({ exercise }: { exercise: WorkoutExerciseGroup }) {
  const type = exercise.exerciseType;
  if (!type || type === 'Standard') return null;

  const cfg = TYPE_BADGE[type];
  if (!cfg) return null;

  const Icon = cfg.icon;
  let extra = '';
  if (type === 'Emom' && exercise.emomConfig) {
    extra = `${exercise.emomConfig.totalMinutes}min`;
  } else if (type === 'Tempo' && exercise.tempoConfig) {
    const t = exercise.tempoConfig;
    extra = `${t.eccentric}:${t.pauseBottom}:${t.concentric}${t.pauseTop ? `:${t.pauseTop}` : ''}`;
  } else if (type === 'Superset' && exercise.supersetGroupId) {
    extra = `${exercise.supersetGroupId}${exercise.supersetPosition ? exercise.supersetPosition : ''}`;
  }

  return (
    <span
      className={cn('inline-flex items-center gap-0.5 px-1.5 py-0.5 rounded text-[11px] font-semibold', cfg.color)}
      title={`${cfg.label}${extra ? ` (${extra})` : ''}`}
    >
      <Icon className="h-3 w-3" />
      {cfg.label}
      {extra && <span className="opacity-75 ml-0.5">({extra})</span>}
    </span>
  );
}

/**
 * ExerciseCard — a clean, borderless block for one exercise within the
 * "notebook" layout. Shows exercise name, prescribed scheme, optional
 * previous performance, and set rows.
 */
export function ExerciseCard({
  exercise,
  // eslint-disable-next-line @typescript-eslint/no-unused-vars
  exerciseIndex: _exerciseIndex,
  focusSetIndex,
  onSaveSet,
  onCompleteSet,
  onUncompleteSet,
  onFocusNextExercise,
  onExerciseNameTap,
}: ExerciseCardProps) {
  const setRefs = useRef<Map<number, () => void>>(new Map());

  // ── Rest Timer state ──
  // Index of the set AFTER which the timer is showing (null = no timer)
  const [timerAfterSetIndex, setTimerAfterSetIndex] = useState<number | null>(null);
  const restSeconds = exercise.restSeconds ?? 0;

  // Register a callback for each SetRow so we can focus them by index
  const registerFocusHandler = useCallback((setIndex: number, handler: () => void) => {
    setRefs.current.set(setIndex, handler);
  }, []);

  // Dismiss timer and focus next set
  const dismissTimer = useCallback(() => {
    const idx = timerAfterSetIndex;
    setTimerAfterSetIndex(null);
    if (idx !== null) {
      const nextHandler = setRefs.current.get(idx + 1);
      if (nextHandler) {
        nextHandler();
      } else {
        onFocusNextExercise();
      }
    }
  }, [timerAfterSetIndex, onFocusNextExercise]);

  const handleFocusNext = useCallback(
    (currentSetIndex: number) => {
      // If rest timer should trigger, show it instead of immediately focusing next
      if (restSeconds > 0) {
        setTimerAfterSetIndex(currentSetIndex);
        return;
      }
      const nextHandler = setRefs.current.get(currentSetIndex + 1);
      if (nextHandler) {
        nextHandler();
      } else {
        // Last set in this exercise — move to the next exercise
        onFocusNextExercise();
      }
    },
    [onFocusNextExercise, restSeconds],
  );

  // Build a default sets array if no logged sets yet (based on prescribed)
  const sets: WorkoutSet[] =
    exercise.sets.length > 0
      ? exercise.sets
      : Array.from({ length: exercise.prescribedSets || 3 }, (_, i) => ({
          id: '',
          setNumber: i + 1,
          targetReps: undefined,
          targetWeight: undefined,
          targetRpe: undefined,
          targetPercentageRM: undefined,
          setLabel: undefined,
          actualReps: 0,
          actualWeight: 0,
          isCompleted: false,
        }));

  // Prescription label: "4 × 8-12 @RPE 8" or raw notation "1x1 3x4"
  const prescription = exercise.rawNotation
    ? exercise.rawNotation
    : [
        exercise.prescribedSets ? `${exercise.prescribedSets}×` : null,
        exercise.prescribedReps ?? null,
        exercise.prescribedRpe ? `@${exercise.prescribedRpe}` : null,
      ]
        .filter(Boolean)
        .join(' ');

  // %1RM reference
  const rmRef = exercise.percentageRM ? `${exercise.percentageRM}% 1RM` : null;

  return (
    <div className="py-4">
      {/* Exercise heading */}
      <div className="px-2 mb-2">
        <div className="flex items-baseline justify-between gap-2">
          <div className="flex items-center gap-1.5">
            {/* Superset label (A1, A2, B1...) */}
            {exercise.supersetGroupId && (
              <span className="text-xs font-bold text-orange-600 bg-orange-50 px-1 rounded">
                {exercise.supersetGroupId}{exercise.supersetPosition ?? ''}
              </span>
            )}
            <h3
              className={cn(
                'text-[15px] font-semibold text-slate-900 tracking-tight',
                onExerciseNameTap && 'underline decoration-dotted decoration-slate-300 underline-offset-2 cursor-pointer hover:text-blue-700 active:text-blue-800 transition-colors',
              )}
              role={onExerciseNameTap ? 'button' : undefined}
              tabIndex={onExerciseNameTap ? 0 : undefined}
              onClick={onExerciseNameTap ? () => onExerciseNameTap(exercise.exerciseId) : undefined}
              onKeyDown={onExerciseNameTap ? (e) => {
                if (e.key === 'Enter' || e.key === ' ') {
                  e.preventDefault();
                  onExerciseNameTap(exercise.exerciseId);
                }
              } : undefined}
            >              {exercise.exerciseName}
            </h3>
            <ExerciseTypeBadge exercise={exercise} />
          </div>
          <div className="flex items-baseline gap-1.5">
            {rmRef && (
              <span className="text-[11px] text-blue-500 font-mono whitespace-nowrap">
                {rmRef}
              </span>
            )}
            {prescription && (
              <span className="text-xs text-slate-400 font-mono whitespace-nowrap">
                {prescription}
              </span>
            )}
          </div>
        </div>

        {/* Tempo display */}
        {exercise.exerciseType === 'Tempo' && exercise.tempoConfig && (
          <p className="mt-0.5 text-xs text-purple-500 font-mono">
            Tempo: {exercise.tempoConfig.eccentric}:{exercise.tempoConfig.pauseBottom}:{exercise.tempoConfig.concentric}
            {exercise.tempoConfig.pauseTop ? `:${exercise.tempoConfig.pauseTop}` : ''}
            <span className="text-purple-400 ml-1">(ecc:pause:con{exercise.tempoConfig.pauseTop ? ':top' : ''})</span>
          </p>
        )}

        {/* EMOM timer display */}
        {exercise.exerciseType === 'Emom' && exercise.emomConfig && (
          <p className="mt-0.5 text-xs text-blue-500 font-mono">
            EMOM {exercise.emomConfig.totalMinutes}min
            {exercise.emomConfig.workSeconds && ` — ${exercise.emomConfig.workSeconds}s work`}
            {exercise.emomConfig.restSeconds && ` / ${exercise.emomConfig.restSeconds}s rest`}
          </p>
        )}

        {exercise.previousPerformance && (
          <PreviousPerformanceBadge data={exercise.previousPerformance} />
        )}

        {exercise.exerciseNotes && (
          <CoachNote text={exercise.exerciseNotes} />
        )}
      </div>

      {/* Column headers */}
      <div className="grid grid-cols-[40px_1fr_1fr_60px_44px] gap-1 px-2 mb-0.5">
        <span className="text-[11px] text-slate-400 text-center uppercase tracking-wider">Set</span>
        <span className="text-[11px] text-slate-400 text-center uppercase tracking-wider">Kg</span>
        <span className="text-[11px] text-slate-400 text-center uppercase tracking-wider">Reps</span>
        <span className="text-[11px] text-slate-400 text-center uppercase tracking-wider">RPE</span>
        <span className="text-[11px] text-slate-400 text-center uppercase tracking-wider">✓</span>
      </div>

      {/* Set rows + inline rest timer */}
      <div className="divide-y divide-slate-100">
        {sets.map((set, i) => (
          <div key={set.id || `empty-${i}`}>
            <SetRowWrapper
              set={set}
              exerciseId={exercise.exerciseId}
              setIndex={i}
              autoFocus={i === focusSetIndex}
              previousPerformance={exercise.previousPerformance}
              onSave={onSaveSet}
              onComplete={onCompleteSet}
              onUncomplete={onUncompleteSet}
              onFocusNext={() => handleFocusNext(i)}
              registerFocus={registerFocusHandler}
            />
            {/* Rest timer appears after this set if it was just completed */}
            {timerAfterSetIndex === i && (
              <RestTimer
                duration={restSeconds}
                onFinish={dismissTimer}
                onSkip={dismissTimer}
              />
            )}
          </div>
        ))}
      </div>

      {restSeconds > 0 && timerAfterSetIndex === null && (
        <p className="px-2 mt-1 text-[11px] text-slate-400 font-mono">
          descanso {restSeconds}s
        </p>
      )}
    </div>
  );
}

/**
 * Wrapper to register focus handler for each set row.
 */
function SetRowWrapper({
  set,
  exerciseId,
  setIndex,
  autoFocus,
  previousPerformance,
  onSave,
  onComplete,
  onUncomplete,
  onFocusNext,
  registerFocus,
}: {
  set: WorkoutSet;
  exerciseId: string;
  setIndex: number;
  autoFocus: boolean;
  previousPerformance?: import('@/types').PreviousPerformance;
  onSave: (data: SaveSetRequest) => void;
  onComplete: (setId: string) => void;
  onUncomplete: (setId: string) => void;
  onFocusNext: () => void;
  registerFocus: (index: number, handler: () => void) => void;
}) {
  const rowRef = useRef<HTMLDivElement>(null);

  // Register a focus handler that focuses the weight input inside this row
  const focusSelf = useCallback(() => {
    const input = rowRef.current?.querySelector<HTMLInputElement>('input[aria-label^="Weight"]');
    input?.focus();
  }, []);

  // Register on mount
  useEffect(() => {
    registerFocus(setIndex, focusSelf);
  }, [registerFocus, setIndex, focusSelf]);

  return (
    <div ref={rowRef}>
      <SetRow
        set={set}
        exerciseId={exerciseId}
        index={setIndex}
        autoFocus={autoFocus}
        previousPerformance={previousPerformance}
        onSave={onSave}
        onComplete={onComplete}
        onUncomplete={onUncomplete}
        onFocusNext={onFocusNext}
      />
    </div>
  );
}
