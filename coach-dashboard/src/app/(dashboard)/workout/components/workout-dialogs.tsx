'use client';

import { useState } from 'react';
import { X } from 'lucide-react';
import { cn } from '@/lib/utils';

interface SkipDialogProps {
  open: boolean;
  onConfirm: (reason?: string) => void;
  onCancel: () => void;
}

/**
 * Inline slide-up confirmation for skipping a workout.
 * Light overlay instead of heavy modal.
 */
export function SkipDialog({ open, onConfirm, onCancel }: SkipDialogProps) {
  const [reason, setReason] = useState('');

  if (!open) return null;

  return (
    <div className="fixed inset-0 z-50 flex items-end justify-center">
      {/* Backdrop */}
      <div className="absolute inset-0 bg-black/20" onClick={onCancel} />

      {/* Sheet */}
      <div className="relative w-full max-w-lg bg-white rounded-t-2xl p-5 pb-8 space-y-4 animate-in slide-in-from-bottom duration-200">
        <div className="flex items-center justify-between">
          <h3 className="text-sm font-semibold text-slate-900">¿Saltar entrenamiento?</h3>
          <button onClick={onCancel} className="p-1 text-slate-400 hover:text-slate-600">
            <X className="w-4 h-4" />
          </button>
        </div>

        <input
          type="text"
          className="w-full h-11 px-3 rounded-lg border border-slate-200 text-sm focus:outline-none focus:ring-2 focus:ring-blue-400 placeholder:text-slate-300"
          placeholder="Razón (opcional)"
          value={reason}
          onChange={(e) => setReason(e.target.value)}
          autoFocus
        />

        <div className="flex gap-2">
          <button
            type="button"
            onClick={onCancel}
            className="flex-1 h-11 rounded-lg text-sm font-medium bg-slate-100 text-slate-600 active:scale-[0.98] transition-all"
          >
            Cancelar
          </button>
          <button
            type="button"
            onClick={() => onConfirm(reason.trim() || undefined)}
            className="flex-1 h-11 rounded-lg text-sm font-medium bg-amber-500 text-white active:scale-[0.98] transition-all"
          >
            Saltar
          </button>
        </div>
      </div>
    </div>
  );
}

interface CompleteDialogProps {
  open: boolean;
  onConfirm: (data: { durationMinutes?: number; fatigueRating?: number; notes?: string }) => void;
  onCancel: () => void;
  elapsedMinutes?: number;
}

/**
 * Inline slide-up for completing a workout with optional metadata.
 */
export function CompleteDialog({ open, onConfirm, onCancel, elapsedMinutes }: CompleteDialogProps) {
  const [fatigue, setFatigue] = useState<number | undefined>(undefined);
  const [notes, setNotes] = useState('');

  if (!open) return null;

  return (
    <div className="fixed inset-0 z-50 flex items-end justify-center">
      <div className="absolute inset-0 bg-black/20" onClick={onCancel} />

      <div className="relative w-full max-w-lg bg-white rounded-t-2xl p-5 pb-8 space-y-4 animate-in slide-in-from-bottom duration-200">
        <div className="flex items-center justify-between">
          <h3 className="text-sm font-semibold text-slate-900">Finalizar entrenamiento</h3>
          <button onClick={onCancel} className="p-1 text-slate-400 hover:text-slate-600">
            <X className="w-4 h-4" />
          </button>
        </div>

        {/* Fatigue rating — 1 to 10 tap row */}
        <div>
          <p className="text-xs text-slate-500 mb-2">¿Cómo te sentiste? (1-10)</p>
          <div className="flex gap-1">
            {[1, 2, 3, 4, 5, 6, 7, 8, 9, 10].map((n) => (
              <button
                key={n}
                type="button"
                onClick={() => setFatigue(n)}
                className={cn(
                  'flex-1 h-10 rounded text-xs font-mono transition-all',
                  fatigue === n
                    ? 'bg-blue-600 text-white scale-105'
                    : 'bg-slate-100 text-slate-500 hover:bg-slate-200',
                )}
              >
                {n}
              </button>
            ))}
          </div>
        </div>

        {/* Notes */}
        <input
          type="text"
          className="w-full h-11 px-3 rounded-lg border border-slate-200 text-sm focus:outline-none focus:ring-2 focus:ring-blue-400 placeholder:text-slate-300"
          placeholder="Notas (opcional)"
          value={notes}
          onChange={(e) => setNotes(e.target.value)}
        />

        <div className="flex gap-2">
          <button
            type="button"
            onClick={onCancel}
            className="flex-1 h-11 rounded-lg text-sm font-medium bg-slate-100 text-slate-600 active:scale-[0.98] transition-all"
          >
            Cancelar
          </button>
          <button
            type="button"
            onClick={() =>
              onConfirm({
                durationMinutes: elapsedMinutes,
                fatigueRating: fatigue,
                notes: notes.trim() || undefined,
              })
            }
            className="flex-1 h-11 rounded-lg text-sm font-medium bg-emerald-600 text-white active:scale-[0.98] transition-all"
          >
            Completar
          </button>
        </div>
      </div>
    </div>
  );
}
