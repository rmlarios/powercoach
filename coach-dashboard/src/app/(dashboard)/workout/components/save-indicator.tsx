'use client';

import { cn } from '@/lib/utils';
import { Check, Loader2 } from 'lucide-react';
import { useEffect, useState } from 'react';

interface SaveIndicatorProps {
  isSaving: boolean;
}

/**
 * Subtle, almost-invisible save status indicator.
 * Shows a brief "Guardando..." then "✓" feedback, then fades completely.
 */
export function SaveIndicator({ isSaving }: SaveIndicatorProps) {
  const [showSaved, setShowSaved] = useState(false);
  const [wasSaving, setWasSaving] = useState(false);

  useEffect(() => {
    if (isSaving) {
      setWasSaving(true);
      setShowSaved(false);
    } else if (wasSaving) {
      setShowSaved(true);
      setWasSaving(false);
      const timer = setTimeout(() => setShowSaved(false), 1500);
      return () => clearTimeout(timer);
    }
  }, [isSaving, wasSaving]);

  if (!isSaving && !showSaved) return null;

  return (
    <div
      className={cn(
        'flex items-center gap-1 text-[11px] transition-opacity duration-300',
        isSaving ? 'text-slate-400 opacity-100' : 'text-emerald-500 opacity-100',
        !isSaving && showSaved && 'animate-out fade-out duration-1000',
      )}
    >
      {isSaving ? (
        <>
          <Loader2 className="w-3 h-3 animate-spin" />
          <span>Guardando...</span>
        </>
      ) : (
        <>
          <Check className="w-3 h-3" />
          <span>Guardado</span>
        </>
      )}
    </div>
  );
}
