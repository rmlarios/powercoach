'use client';

import { useCallback, useRef, useState, KeyboardEvent, useEffect } from 'react';
import { Check, Minus, StickyNote } from 'lucide-react';
import { cn } from '@/lib/utils';
import { haptic, parseSmartInput, detectPR, calculateDelta } from '../lib/workout-utils';
import { PRBadge } from './pr-badge';
import { DeltaBadge } from './delta-badge';
import type { WorkoutSet, SaveSetRequest, PreviousPerformance } from '@/types';

interface SetRowProps {
  set: WorkoutSet;
  exerciseId: string;
  index: number;
  /** Focus this row on mount? */
  autoFocus?: boolean;
  /** Previous best for this exercise — used for PR detection. */
  previousPerformance?: PreviousPerformance;
  onSave: (data: SaveSetRequest) => void;
  onComplete: (setId: string) => void;
  onUncomplete: (setId: string) => void;
  onFocusNext: () => void;
}

/**
 * Editable set row — the core data entry component.
 *
 * Design: inline-edit inputs with monospace font, 44px touch zones,
 * smart parser ("100x8"), auto-tab to next field, silent auto-save on blur.
 */
export function SetRow({
  set,
  exerciseId,
  // eslint-disable-next-line @typescript-eslint/no-unused-vars
  index: _index,
  autoFocus,
  previousPerformance,
  onSave,
  onComplete,
  onUncomplete,
  onFocusNext,
}: SetRowProps) {
  const [weight, setWeight] = useState(String(set.actualWeight || ''));
  const [reps, setReps] = useState(String(set.actualReps || ''));
  const [rpe, setRpe] = useState(set.actualRpe != null ? String(set.actualRpe) : '');
  const [smartInput, setSmartInput] = useState('');
  const [isSmartMode, setIsSmartMode] = useState(false);

  // PR state
  const [prResult, setPrResult] = useState<{ types: import('../lib/workout-utils').PRType[]; deltas: string[] } | null>(null);
  const [showGoldenFlash, setShowGoldenFlash] = useState(false);

  // Notes state
  const [showNotes, setShowNotes] = useState(false);
  const [notesText, setNotesText] = useState(set.notes ?? '');
  const longPressRef = useRef<ReturnType<typeof setTimeout> | null>(null);

  const weightRef = useRef<HTMLInputElement>(null);
  const repsRef = useRef<HTMLInputElement>(null);
  const rpeRef = useRef<HTMLInputElement>(null);
  const smartRef = useRef<HTMLInputElement>(null);

  // Auto-focus weight input when flagged
  useEffect(() => {
    if (autoFocus && weightRef.current && !set.isCompleted) {
      weightRef.current.focus();
    }
  }, [autoFocus, set.isCompleted]);

  const buildSavePayload = useCallback(
    (w?: string, r?: string, rpeVal?: string): SaveSetRequest => ({
      exerciseLogId: set.id || undefined,
      exerciseId,
      setNumber: set.setNumber,
      reps: parseInt(r ?? reps, 10) || 0,
      weight: parseFloat(w ?? weight) || 0,
      rpe: rpeVal !== undefined ? (parseFloat(rpeVal) || undefined) : (parseFloat(rpe) || undefined),
      targetReps: set.targetReps ?? undefined,
      targetWeight: set.targetWeight ?? undefined,
      isCompleted: set.isCompleted,
      notes: notesText || undefined,
    }),
    [set, exerciseId, weight, reps, rpe, notesText],
  );

  // Fire auto-save on blur
  const handleBlur = useCallback(() => {
    const w = parseFloat(weight);
    const r = parseInt(reps, 10);
    if (w > 0 || r > 0) {
      onSave(buildSavePayload());
    }
  }, [weight, reps, onSave, buildSavePayload]);

  // Smart input handler — parse "100x8" and fill both fields
  const handleSmartKey = useCallback(
    (e: KeyboardEvent<HTMLInputElement>) => {
      if (e.key === 'Enter') {
        const parsed = parseSmartInput(smartInput);
        if (parsed) {
          setWeight(String(parsed.weight));
          setReps(String(parsed.reps));
          setIsSmartMode(false);
          onSave(buildSavePayload(String(parsed.weight), String(parsed.reps)));
          rpeRef.current?.focus();
        }
      } else if (e.key === 'Escape') {
        setIsSmartMode(false);
        setSmartInput('');
      }
    },
    [smartInput, onSave, buildSavePayload],
  );

  // Tab flow: weight → reps → rpe → next row
  const handleFieldKey = useCallback(
    (e: KeyboardEvent<HTMLInputElement>, nextRef: React.RefObject<HTMLInputElement | null> | 'next') => {
      if (e.key === 'Enter' || (e.key === 'Tab' && !e.shiftKey)) {
        e.preventDefault();
        if (nextRef === 'next') {
          onFocusNext();
        } else {
          nextRef.current?.focus();
        }
      }
    },
    [onFocusNext],
  );

  const isCompleted = set.isCompleted;
  const setLabel = set.setLabel || `S${set.setNumber}`;
  const hasNote = notesText.trim().length > 0;

  // Long-press handlers for notes toggle
  const handleTouchStart = useCallback(() => {
    longPressRef.current = setTimeout(() => {
      haptic(15);
      setShowNotes((prev) => !prev);
    }, 500);
  }, []);

  const handleTouchEnd = useCallback(() => {
    if (longPressRef.current) {
      clearTimeout(longPressRef.current);
      longPressRef.current = null;
    }
  }, []);

  // Save notes on blur
  const handleNotesBlur = useCallback(() => {
    onSave({ ...buildSavePayload(), notes: notesText || undefined });
    if (!notesText.trim()) setShowNotes(false);
  }, [notesText, onSave, buildSavePayload]);

  const handleCheck = useCallback(() => {
    haptic();
    if (isCompleted) {
      // Un-complete: toggle back to editable
      onSave({ ...buildSavePayload(), isCompleted: false });
      onUncomplete(set.id);
      setPrResult(null);
      setShowGoldenFlash(false);
      return;
    }
    const w = parseFloat(weight);
    const r = parseInt(reps, 10);
    if (w > 0 && r > 0) {
      // PR detection
      const pr = detectPR(w, r, previousPerformance);
      if (pr.isPR) {
        setPrResult({ types: pr.types, deltas: pr.deltas });
        setShowGoldenFlash(true);
        haptic(30); // Stronger haptic for PR
        setTimeout(() => setShowGoldenFlash(false), 1200);
      }
      // Save with isCompleted = true
      onSave({ ...buildSavePayload(), isCompleted: true });
      onComplete(set.id);
      // Move focus to next set after short delay for animation
      setTimeout(onFocusNext, 80);
    }
  }, [weight, reps, onSave, onComplete, onUncomplete, buildSavePayload, set.id, onFocusNext, isCompleted, previousPerformance]);

  // Show target RPE hint if set has specific RPE
  const rpeHint = set.targetRpe ? `@${set.targetRpe}` : '';
  // Show %1RM per set if different from group
  const rmHint = set.targetPercentageRM ? `${set.targetPercentageRM}%` : '';

  // Best weight placeholder: suggestedWeight (from %RM × e1RM) > targetWeight > 'kg'
  const weightPlaceholder = set.suggestedWeight
    ? String(set.suggestedWeight)
    : set.targetWeight
      ? String(set.targetWeight)
      : 'kg';

  // Delta vs plan (only shown when completed)
  const weightDelta = isCompleted
    ? calculateDelta(parseFloat(weight) || 0, set.targetWeight ?? set.suggestedWeight, 'kg')
    : null;
  const repsDelta = isCompleted
    ? calculateDelta(parseInt(reps, 10) || 0, set.targetReps, 'r')
    : null;

  return (
    <div
      className={cn(
        'grid grid-cols-[40px_1fr_1fr_60px_44px] items-center gap-1 py-1 px-2 rounded-md transition-all duration-200',
        isCompleted && 'opacity-60 bg-emerald-50/40',
        showGoldenFlash && 'bg-amber-50/80 ring-1 ring-amber-200/60 opacity-100',
      )}
    >
      {/* Set number / label */}
      <div
        className="flex flex-col items-center select-none"
        onTouchStart={handleTouchStart}
        onTouchEnd={handleTouchEnd}
        onTouchCancel={handleTouchEnd}
        onMouseDown={handleTouchStart}
        onMouseUp={handleTouchEnd}
        onMouseLeave={handleTouchEnd}
      >
        <span
          className={cn(
            'text-[11px] font-medium text-slate-500 text-center leading-tight',
            isCompleted && 'line-through',
            set.setLabel && 'text-[11px] text-slate-600 font-semibold',
          )}
        >
          {setLabel}
        </span>
        {hasNote && !showNotes && (
          <StickyNote
            className="w-2.5 h-2.5 text-amber-400 mt-0.5 cursor-pointer"
            onClick={(e) => { e.stopPropagation(); setShowNotes(true); }}
          />
        )}
        {(rmHint || rpeHint) && (
          <span className="text-[10px] text-blue-400 font-mono leading-tight">
            {rmHint}{rmHint && rpeHint ? ' ' : ''}{rpeHint}
          </span>
        )}
      </div>

      {/* Weight */}
      {isSmartMode ? (
        <input
          ref={smartRef}
          type="text"
          inputMode="decimal"
          className="col-span-2 h-11 px-2 bg-white border border-blue-300 rounded text-center font-mono text-sm focus:outline-none focus:ring-2 focus:ring-blue-400"
          placeholder="100x8"
          value={smartInput}
          onChange={(e) => setSmartInput(e.target.value)}
          onKeyDown={handleSmartKey}
          onBlur={() => { setIsSmartMode(false); setSmartInput(''); }}
          autoFocus
        />
      ) : (
        <>
          <div className="relative">
            <input
              ref={weightRef}
              type="text"
              inputMode="decimal"
              className={cn(
                'w-full h-11 px-2 bg-transparent border-b border-slate-200 text-center font-mono text-sm',
                'focus:outline-none focus:border-blue-400 transition-colors',
                'placeholder:text-slate-300',
                isCompleted && 'line-through text-slate-400',
              )}
              placeholder={weightPlaceholder}
              value={weight}
              onChange={(e) => setWeight(e.target.value)}
              onBlur={handleBlur}
              onKeyDown={(e) => handleFieldKey(e, repsRef)}
              onDoubleClick={() => { setIsSmartMode(true); setTimeout(() => smartRef.current?.focus(), 0); }}
              aria-label={`Weight for set ${set.setNumber}`}
            />
            {/* Suggested weight from %RM × e1RM: show as blue hint below input */}
            {set.suggestedWeight && !weight && (
              <span className="absolute bottom-0 left-1/2 -translate-x-1/2 text-[11px] text-blue-400 font-mono pointer-events-none whitespace-nowrap">
                {set.suggestedWeight}kg
                {set.targetPercentageRM && <span className="text-blue-300 ml-0.5">({set.targetPercentageRM}%)</span>}
              </span>
            )}
            {/* Fallback: show targetWeight hint when no suggestedWeight */}
            {!set.suggestedWeight && set.targetWeight && !weight && (
              <span className="absolute bottom-0 left-1/2 -translate-x-1/2 text-[11px] text-slate-300 font-mono pointer-events-none">
                {set.targetWeight}kg
              </span>
            )}
            {/* Delta vs plan */}
            {weightDelta && (
              <span className="absolute -bottom-0.5 right-0.5 pointer-events-none">
                <DeltaBadge value={weightDelta.value} direction={weightDelta.direction} />
              </span>
            )}
          </div>

          {/* Reps */}
          <div className="relative">
            <input
              ref={repsRef}
              type="text"
              inputMode="numeric"
              className={cn(
                'w-full h-11 px-2 bg-transparent border-b border-slate-200 text-center font-mono text-sm',
                'focus:outline-none focus:border-blue-400 transition-colors',
                'placeholder:text-slate-300',
                isCompleted && 'line-through text-slate-400',
              )}
              placeholder={set.targetReps ? String(set.targetReps) : 'reps'}
              value={reps}
              onChange={(e) => setReps(e.target.value)}
              onBlur={handleBlur}
              onKeyDown={(e) => handleFieldKey(e, rpeRef)}
              aria-label={`Reps for set ${set.setNumber}`}
            />
            {set.targetReps && !reps && (
              <span className="absolute bottom-0 left-1/2 -translate-x-1/2 text-[11px] text-slate-300 font-mono pointer-events-none">
                {set.targetReps}r
              </span>
            )}
            {/* Delta vs plan */}
            {repsDelta && (
              <span className="absolute -bottom-0.5 right-0.5 pointer-events-none">
                <DeltaBadge value={repsDelta.value} direction={repsDelta.direction} />
              </span>
            )}
          </div>
        </>
      )}

      {/* RPE (optional) */}
      {!isSmartMode && (
        <input
          ref={rpeRef}
          type="text"
          inputMode="decimal"
          className={cn(
            'w-full h-11 px-1 bg-transparent border-b border-slate-200 text-center font-mono text-sm text-slate-500',
            'focus:outline-none focus:border-blue-400 transition-colors',
            'placeholder:text-slate-300',
            isCompleted && 'line-through text-slate-400',
          )}
          placeholder="RPE"
          value={rpe}
          onChange={(e) => setRpe(e.target.value)}
          onBlur={handleBlur}
          onKeyDown={(e) => handleFieldKey(e, 'next')}
          aria-label={`RPE for set ${set.setNumber}`}
        />
      )}

      {/* Complete check button — 44x44 touch zone */}
      <button
        type="button"
        className={cn(
          'flex items-center justify-center w-11 h-11 rounded-md transition-all duration-200',
          isCompleted
            ? 'bg-emerald-500 text-white hover:bg-emerald-600 active:scale-95'
            : 'bg-slate-100 text-slate-400 hover:bg-slate-200 active:scale-95',
          prResult && 'bg-amber-400 hover:bg-amber-500',
        )}
        onClick={handleCheck}
        aria-label={isCompleted ? 'Undo set completion' : `Complete set ${set.setNumber}`}
      >
        {isCompleted ? (
          <Check className="w-4 h-4" />
        ) : (
          <Minus className="w-4 h-4" />
        )}
      </button>

      {/* PR Badge — spans full row below */}
      {prResult && isCompleted && (
        <div className="col-span-5 flex justify-center py-0.5">
          <PRBadge types={prResult.types} deltas={prResult.deltas} />
        </div>
      )}

      {/* Inline notes field — spans full row */}
      {showNotes && (
        <div className="col-span-5 px-1 pb-1">
          <input
            type="text"
            className="w-full h-8 px-2 text-xs text-slate-600 bg-amber-50/50 border border-amber-200/60 rounded-md placeholder:text-amber-300 focus:outline-none focus:ring-1 focus:ring-amber-300 font-mono"
            placeholder="Nota… (dolor, técnica, ajuste)"
            value={notesText}
            onChange={(e) => setNotesText(e.target.value)}
            onBlur={handleNotesBlur}
            onKeyDown={(e) => { if (e.key === 'Enter') (e.target as HTMLInputElement).blur(); }}
            autoFocus
          />
        </div>
      )}
    </div>
  );
}
