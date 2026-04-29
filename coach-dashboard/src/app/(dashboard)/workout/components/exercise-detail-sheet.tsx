'use client';

import { useMemo } from 'react';
import {
  Sheet,
  SheetContent,
  SheetHeader,
  SheetTitle,
  SheetDescription,
} from '@/components/ui/sheet';
import {
  Trophy,
  Dumbbell,
  TrendingUp,
  Activity,
  Play,
  ChevronUp,
  ChevronDown,
  Minus,
} from 'lucide-react';
import { cn } from '@/lib/utils';
import type { ExerciseLiftHistory, LiftEntry, ExerciseLiftPR } from '@/types';

interface ExerciseDetailSheetProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  data: ExerciseLiftHistory | null | undefined;
  isLoading?: boolean;
}

/**
 * ExerciseDetailSheet — bottom sheet showing:
 * 1) Exercise info (muscle, equipment, compound badge)
 * 2) Video embed (if videoUrl present)
 * 3) Instructions + coaching cues
 * 4) PR cards row
 * 5) Lift history table with e1RM, weekly trend arrows
 * 6) Sparkline chart showing e1RM progression
 *
 * Opens from bottom, 85vh max height, scrollable inner.
 * Design: paper-feel notebook aesthetic, monospace data, Inter labels.
 */
export function ExerciseDetailSheet({
  open,
  onOpenChange,
  data,
  isLoading,
}: ExerciseDetailSheetProps) {
  return (
    <Sheet open={open} onOpenChange={onOpenChange}>
      <SheetContent
        side="bottom"
        className="max-h-[85vh] rounded-t-2xl px-0 pt-2 pb-6 overflow-hidden flex flex-col"
      >
        {/* Drag handle */}
        <div className="flex justify-center mb-2">
          <div className="w-10 h-1 rounded-full bg-slate-200" />
        </div>

        {isLoading || !data ? (
          <SheetLoadingSkeleton />
        ) : (
          <>
            <SheetHeader className="px-5 pb-3">
              <SheetTitle className="text-base font-bold text-slate-900 text-left">
                {data.exerciseName}
              </SheetTitle>
              <SheetDescription className="text-left">
                <ExerciseMeta data={data} />
              </SheetDescription>
            </SheetHeader>

            <div className="flex-1 overflow-y-auto px-5 space-y-5">
              {/* Video */}
              {data.videoUrl && <VideoEmbed url={data.videoUrl} name={data.exerciseName} />}

              {/* Instructions */}
              {data.instructions.length > 0 && (
                <InstructionsSection instructions={data.instructions} />
              )}

              {/* Coaching Cues */}
              {data.coachingCues.length > 0 && (
                <CoachingCuesSection cues={data.coachingCues} />
              )}

              {/* PR Cards */}
              {data.personalRecords && (
                <PRCardsRow prs={data.personalRecords} currentE1RM={data.currentEstimated1RM} />
              )}

              {/* Sparkline Chart */}
              {data.entries.length >= 2 && <E1RMSparkline entries={data.entries} />}

              {/* History Table */}
              {data.entries.length > 0 && <LiftHistoryTable entries={data.entries} />}

              {/* Description */}
              {data.description && (
                <p className="text-xs text-slate-400 italic pb-2">{data.description}</p>
              )}
            </div>
          </>
        )}
      </SheetContent>
    </Sheet>
  );
}

// ─── Sub-components ──────────────────────────────────────────

function ExerciseMeta({ data }: { data: ExerciseLiftHistory }) {
  const tags = [
    data.primaryMuscleGroup,
    data.equipment,
    data.isCompound ? 'Compuesto' : 'Aislamiento',
    data.category,
  ].filter(Boolean);

  return (
    <div className="flex flex-wrap gap-1.5 mt-1">
      {tags.map((tag) => (
        <span
          key={tag}
          className="inline-block text-[10px] font-medium px-2 py-0.5 rounded-full bg-slate-100 text-slate-500"
        >
          {tag}
        </span>
      ))}
    </div>
  );
}

function VideoEmbed({ url, name }: { url: string; name: string }) {
  // Convert YouTube watch URL to embed
  const embedUrl = url.replace('watch?v=', 'embed/').replace('youtu.be/', 'youtube.com/embed/');

  return (
    <div className="relative w-full aspect-video rounded-lg overflow-hidden bg-slate-900">
      <iframe
        src={embedUrl}
        title={`Video: ${name}`}
        className="absolute inset-0 w-full h-full"
        allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture"
        allowFullScreen
        loading="lazy"
      />
    </div>
  );
}

function InstructionsSection({ instructions }: { instructions: string[] }) {
  return (
    <div>
      <h4 className="text-[11px] font-semibold text-slate-500 uppercase tracking-wider mb-2">
        Instrucciones
      </h4>
      <ol className="space-y-1.5">
        {instructions.map((step, i) => (
          <li key={i} className="flex gap-2 text-xs text-slate-600 leading-relaxed">
            <span className="flex-shrink-0 w-5 h-5 rounded-full bg-slate-100 text-slate-400 text-[10px] font-mono flex items-center justify-center">
              {i + 1}
            </span>
            {step}
          </li>
        ))}
      </ol>
    </div>
  );
}

function CoachingCuesSection({ cues }: { cues: string[] }) {
  return (
    <div>
      <h4 className="text-[11px] font-semibold text-slate-500 uppercase tracking-wider mb-2">
        Cues del Coach
      </h4>
      <div className="flex flex-wrap gap-1.5">
        {cues.map((cue) => (
          <span
            key={cue}
            className="inline-flex items-center gap-1 text-[11px] font-medium px-2.5 py-1 rounded-full bg-blue-50 text-blue-600 border border-blue-100"
          >
            <Play className="w-2.5 h-2.5" />
            {cue}
          </span>
        ))}
      </div>
    </div>
  );
}

function PRCardsRow({ prs, currentE1RM }: { prs: ExerciseLiftPR; currentE1RM?: number }) {
  const cards = [
    {
      label: 'Max Peso',
      value: `${prs.maxWeight}kg`,
      date: formatShortDate(prs.maxWeightDate),
      icon: Dumbbell,
      color: 'text-orange-500 bg-orange-50',
    },
    {
      label: 'Max Reps',
      value: `${prs.maxReps}`,
      date: formatShortDate(prs.maxRepsDate),
      icon: Activity,
      color: 'text-purple-500 bg-purple-50',
    },
    {
      label: 'e1RM',
      value: currentE1RM ? `${currentE1RM.toFixed(1)}kg` : `${prs.maxEstimated1RM.toFixed(1)}kg`,
      date: formatShortDate(prs.maxEstimated1RMDate),
      icon: TrendingUp,
      color: 'text-emerald-500 bg-emerald-50',
    },
    {
      label: 'Vol Max',
      value: `${(prs.maxVolume / 1000).toFixed(1)}t`,
      date: formatShortDate(prs.maxVolumeDate),
      icon: Trophy,
      color: 'text-blue-500 bg-blue-50',
    },
  ];

  return (
    <div>
      <h4 className="text-[11px] font-semibold text-slate-500 uppercase tracking-wider mb-2">
        Records Personales
      </h4>
      <div className="grid grid-cols-2 gap-2">
        {cards.map((card) => {
          const Icon = card.icon;
          return (
            <div
              key={card.label}
              className="flex items-center gap-2 p-2.5 rounded-lg bg-slate-50 border border-slate-100"
            >
              <div className={cn('flex items-center justify-center w-8 h-8 rounded-lg', card.color)}>
                <Icon className="w-4 h-4" />
              </div>
              <div>
                <p className="text-[10px] text-slate-400 leading-none">{card.label}</p>
                <p className="text-sm font-mono font-semibold text-slate-800 leading-tight">
                  {card.value}
                </p>
                <p className="text-[9px] text-slate-300 leading-none">{card.date}</p>
              </div>
            </div>
          );
        })}
      </div>
    </div>
  );
}

/**
 * SVG sparkline showing e1RM trend over time.
 * Simple, clean — no axis labels, just the trend line.
 */
function E1RMSparkline({ entries }: { entries: LiftEntry[] }) {
  const sorted = useMemo(
    () => [...entries].sort((a, b) => new Date(a.date).getTime() - new Date(b.date).getTime()),
    [entries],
  );

  const values = sorted.map((e) => e.estimated1RM ?? 0).filter((v) => v > 0);
  if (values.length < 2) return null;

  const min = Math.min(...values);
  const max = Math.max(...values);
  const range = max - min || 1;
  const W = 280;
  const H = 60;
  const pad = 4;

  const points = values.map((v, i) => {
    const x = pad + (i / (values.length - 1)) * (W - 2 * pad);
    const y = H - pad - ((v - min) / range) * (H - 2 * pad);
    return `${x},${y}`;
  });

  const polyline = points.join(' ');
  const last = values[values.length - 1];
  const prev = values[values.length - 2];
  const trendUp = last > prev;

  return (
    <div>
      <h4 className="text-[11px] font-semibold text-slate-500 uppercase tracking-wider mb-2">
        Progresión e1RM
      </h4>
      <div className="relative bg-slate-50 rounded-lg p-2 border border-slate-100">
        <svg viewBox={`0 0 ${W} ${H}`} className="w-full h-16">
          {/* Gradient fill under line */}
          <defs>
            <linearGradient id="sparkGrad" x1="0" x2="0" y1="0" y2="1">
              <stop offset="0%" stopColor={trendUp ? '#10b981' : '#f59e0b'} stopOpacity="0.2" />
              <stop offset="100%" stopColor={trendUp ? '#10b981' : '#f59e0b'} stopOpacity="0" />
            </linearGradient>
          </defs>
          {/* Fill area */}
          <polygon
            points={`${points[0].split(',')[0]},${H} ${polyline} ${points[points.length - 1].split(',')[0]},${H}`}
            fill="url(#sparkGrad)"
          />
          {/* Line */}
          <polyline
            points={polyline}
            fill="none"
            stroke={trendUp ? '#10b981' : '#f59e0b'}
            strokeWidth="2"
            strokeLinecap="round"
            strokeLinejoin="round"
          />
          {/* Last point dot */}
          <circle
            cx={points[points.length - 1].split(',')[0]}
            cy={points[points.length - 1].split(',')[1]}
            r="3"
            fill={trendUp ? '#10b981' : '#f59e0b'}
          />
        </svg>
        {/* Labels */}
        <div className="flex justify-between mt-1">
          <span className="text-[9px] font-mono text-slate-300">
            {formatShortDate(sorted[0].date)}
          </span>
          <span
            className={cn(
              'text-[11px] font-mono font-semibold',
              trendUp ? 'text-emerald-600' : 'text-amber-600',
            )}
          >
            {last.toFixed(1)}kg
          </span>
          <span className="text-[9px] font-mono text-slate-300">
            {formatShortDate(sorted[sorted.length - 1].date)}
          </span>
        </div>
      </div>
    </div>
  );
}

function LiftHistoryTable({ entries }: { entries: LiftEntry[] }) {
  // Most recent first
  const sorted = useMemo(
    () => [...entries].sort((a, b) => new Date(b.date).getTime() - new Date(a.date).getTime()),
    [entries],
  );

  return (
    <div>
      <h4 className="text-[11px] font-semibold text-slate-500 uppercase tracking-wider mb-2">
        Historial de Sesiones
      </h4>
      <div className="overflow-x-auto">
        <table className="w-full text-xs">
          <thead>
            <tr className="text-[10px] text-slate-400 uppercase">
              <th className="text-left font-medium pb-1 pr-2">Fecha</th>
              <th className="text-right font-medium pb-1 px-1">Peso</th>
              <th className="text-right font-medium pb-1 px-1">Reps</th>
              <th className="text-right font-medium pb-1 px-1">e1RM</th>
              <th className="text-right font-medium pb-1 px-1">Vol</th>
              <th className="text-center font-medium pb-1 pl-1 w-5"></th>
            </tr>
          </thead>
          <tbody className="divide-y divide-slate-50">
            {sorted.map((entry, i) => {
              const prev = sorted[i + 1]; // older entry
              const trend = prev && entry.estimated1RM && prev.estimated1RM
                ? entry.estimated1RM > prev.estimated1RM
                  ? 'up'
                  : entry.estimated1RM < prev.estimated1RM
                    ? 'down'
                    : 'flat'
                : null;

              return (
                <tr key={entry.date} className="font-mono">
                  <td className="py-1.5 pr-2 text-slate-500">{formatShortDate(entry.date)}</td>
                  <td className="py-1.5 px-1 text-right text-slate-700 font-semibold">
                    {entry.maxWeight}
                  </td>
                  <td className="py-1.5 px-1 text-right text-slate-600">{entry.bestReps}</td>
                  <td className="py-1.5 px-1 text-right text-slate-700">
                    {entry.estimated1RM?.toFixed(1) ?? '—'}
                  </td>
                  <td className="py-1.5 px-1 text-right text-slate-400">
                    {(entry.totalVolume / 1000).toFixed(1)}t
                  </td>
                  <td className="py-1.5 pl-1 text-center">
                    {trend === 'up' && <ChevronUp className="w-3 h-3 text-emerald-500 inline" />}
                    {trend === 'down' && <ChevronDown className="w-3 h-3 text-red-400 inline" />}
                    {trend === 'flat' && <Minus className="w-3 h-3 text-slate-300 inline" />}
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

function SheetLoadingSkeleton() {
  return (
    <div className="px-5 space-y-4 py-4">
      <div className="h-5 w-40 bg-slate-100 rounded animate-pulse" />
      <div className="flex gap-1.5">
        {[1, 2, 3].map((i) => (
          <div key={i} className="h-5 w-16 bg-slate-50 rounded-full animate-pulse" />
        ))}
      </div>
      <div className="h-40 bg-slate-50 rounded-lg animate-pulse" />
      <div className="grid grid-cols-2 gap-2">
        {[1, 2, 3, 4].map((i) => (
          <div key={i} className="h-16 bg-slate-50 rounded-lg animate-pulse" />
        ))}
      </div>
      <div className="space-y-2">
        {[1, 2, 3, 4].map((i) => (
          <div key={i} className="h-6 bg-slate-50 rounded animate-pulse" />
        ))}
      </div>
    </div>
  );
}

// ─── Util ───────────────────────────────────────────────────

function formatShortDate(iso: string): string {
  const d = new Date(iso);
  return d.toLocaleDateString('es-ES', { day: 'numeric', month: 'short' });
}
