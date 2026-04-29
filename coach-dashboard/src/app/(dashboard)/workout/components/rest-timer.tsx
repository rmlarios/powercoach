'use client';

import { useCallback, useEffect, useRef, useState } from 'react';
import { X, Plus, Minus } from 'lucide-react';
import { cn } from '@/lib/utils';
import { haptic, formatTimer } from '../lib/workout-utils';

interface RestTimerProps {
  /** Initial countdown duration in seconds. */
  duration: number;
  /** Called when timer finishes naturally or is skipped. */
  onFinish: () => void;
  /** Called when timer is dismissed manually (skip). */
  onSkip: () => void;
}

const RADIUS = 36;
const CIRCUMFERENCE = 2 * Math.PI * RADIUS;

/**
 * RestTimer — circular SVG countdown that auto-starts.
 *
 * Design:
 * - Circular progress ring (stroke-dashoffset animation)
 * - Monospace countdown centered inside the ring
 * - Skip / +30s / -30s buttons below
 * - Haptic + audio beep at 0
 * - Compact: fits between set rows without disrupting notebook feel
 */
export function RestTimer({ duration, onFinish, onSkip }: RestTimerProps) {
  const [remaining, setRemaining] = useState(duration);
  const [totalDuration, setTotalDuration] = useState(duration);
  const intervalRef = useRef<ReturnType<typeof setInterval> | null>(null);
  // eslint-disable-next-line @typescript-eslint/no-unused-vars
  const _audioRef = useRef<AudioContext | null>(null);
  const hasFinishedRef = useRef(false);

  // Start countdown on mount
  useEffect(() => {
    intervalRef.current = setInterval(() => {
      setRemaining((prev) => {
        if (prev <= 1) {
          return 0;
        }
        return prev - 1;
      });
    }, 1000);

    return () => {
      if (intervalRef.current) clearInterval(intervalRef.current);
    };
  }, []);

  // Fire finish when reaching 0
  useEffect(() => {
    if (remaining <= 0 && !hasFinishedRef.current) {
      hasFinishedRef.current = true;
      if (intervalRef.current) clearInterval(intervalRef.current);
      playBeep();
      haptic(30);
      // Small delay so user sees "0:00" before dismiss
      setTimeout(onFinish, 800);
    }
  }, [remaining, onFinish]);

  const handleAdd30 = useCallback(() => {
    setRemaining((prev) => prev + 30);
    setTotalDuration((prev) => prev + 30);
  }, []);

  const handleMinus30 = useCallback(() => {
    setRemaining((prev) => Math.max(0, prev - 30));
  }, []);

  const handleSkip = useCallback(() => {
    if (intervalRef.current) clearInterval(intervalRef.current);
    haptic(10);
    onSkip();
  }, [onSkip]);

  // Progress fraction: 1 = full, 0 = done
  const progress = totalDuration > 0 ? remaining / totalDuration : 0;
  const dashOffset = CIRCUMFERENCE * (1 - progress);

  // Color transitions: green > 50%, yellow 20-50%, red < 20%
  const colorClass =
    progress > 0.5
      ? 'text-emerald-400'
      : progress > 0.2
        ? 'text-amber-400'
        : 'text-red-400';

  return (
    <div className="flex flex-col items-center py-3 animate-in fade-in slide-in-from-top-2 duration-300">
      {/* Circular timer */}
      <div className="relative w-20 h-20">
        <svg viewBox="0 0 80 80" className="w-full h-full -rotate-90">
          {/* Background ring */}
          <circle
            cx="40"
            cy="40"
            r={RADIUS}
            fill="none"
            stroke="currentColor"
            strokeWidth="4"
            className="text-slate-100"
          />
          {/* Progress ring */}
          <circle
            cx="40"
            cy="40"
            r={RADIUS}
            fill="none"
            stroke="currentColor"
            strokeWidth="4"
            strokeLinecap="round"
            strokeDasharray={CIRCUMFERENCE}
            strokeDashoffset={dashOffset}
            className={cn('transition-all duration-1000 ease-linear', colorClass)}
          />
        </svg>
        {/* Centered countdown text */}
        <div className="absolute inset-0 flex items-center justify-center">
          <span
            className={cn(
              'text-lg font-mono font-bold tabular-nums',
              remaining <= 5 && remaining > 0 ? 'text-red-500 animate-pulse' : 'text-slate-700',
              remaining === 0 && 'text-emerald-500',
            )}
          >
            {formatTimer(remaining)}
          </span>
        </div>
      </div>

      {/* Controls row */}
      <div className="flex items-center gap-2 mt-2">
        <button
          type="button"
          onClick={handleMinus30}
          disabled={remaining <= 0}
          className={cn(
            'flex items-center justify-center gap-0.5 h-8 px-2.5 rounded-full text-xs font-medium transition-all',
            'bg-slate-100 text-slate-500 hover:bg-slate-200 active:scale-95',
            remaining <= 0 && 'opacity-40 cursor-default',
          )}
          aria-label="Reduce 30 seconds"
        >
          <Minus className="w-3 h-3" />
          30s
        </button>

        <button
          type="button"
          onClick={handleSkip}
          className="flex items-center justify-center gap-1 h-8 px-3 rounded-full text-xs font-medium bg-slate-200 text-slate-600 hover:bg-slate-300 active:scale-95 transition-all"
          aria-label="Skip rest timer"
        >
          <X className="w-3 h-3" />
          Skip
        </button>

        <button
          type="button"
          onClick={handleAdd30}
          className="flex items-center justify-center gap-0.5 h-8 px-2.5 rounded-full text-xs font-medium bg-slate-100 text-slate-500 hover:bg-slate-200 active:scale-95 transition-all"
          aria-label="Add 30 seconds"
        >
          <Plus className="w-3 h-3" />
          30s
        </button>
      </div>

      {/* Label */}
      <p className="text-[11px] text-slate-400 mt-1 font-mono">
        descanso
      </p>
    </div>
  );
}

/**
 * Play a short beep using Web Audio API.
 * Clean sine wave, no external audio files needed.
 */
function playBeep() {
  try {
    const ctx = new (window.AudioContext || (window as unknown as { webkitAudioContext: typeof AudioContext }).webkitAudioContext)();
    const osc = ctx.createOscillator();
    const gain = ctx.createGain();

    osc.connect(gain);
    gain.connect(ctx.destination);

    osc.type = 'sine';
    osc.frequency.value = 880; // A5

    gain.gain.setValueAtTime(0.3, ctx.currentTime);
    gain.gain.exponentialRampToValueAtTime(0.01, ctx.currentTime + 0.3);

    osc.start(ctx.currentTime);
    osc.stop(ctx.currentTime + 0.3);

    // Cleanup
    setTimeout(() => ctx.close(), 500);
  } catch {
    // Audio not available — haptic is the fallback
  }
}
