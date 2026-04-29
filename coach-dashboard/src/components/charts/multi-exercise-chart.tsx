'use client';

import React, { useMemo } from 'react';
import {
  LineChart,
  Line,
  XAxis,
  YAxis,
  CartesianGrid,
  Tooltip,
  Legend,
  ResponsiveContainer,
} from 'recharts';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Switch } from '@/components/ui/switch';
import { Label } from '@/components/ui/label';
import { cn } from '@/utils/cn';
import type { ExerciseProgressionInstance } from '@/types/builder';

// ============================================
// Constants
// ============================================
const SERIES_COLORS = [
  '#3b82f6', // blue
  '#ef4444', // red
  '#22c55e', // green
  '#f59e0b', // amber
  '#8b5cf6', // violet
  '#ec4899', // pink
  '#06b6d4', // cyan
  '#f97316', // orange
];

// ============================================
// Types
// ============================================
export interface ExerciseSeries {
  exerciseId: string;
  exerciseName: string;
  instances: ExerciseProgressionInstance[];
  oneRM?: number;
}

interface MergedDataPoint {
  week: string;
  weekNumber: number;
  [key: string]: string | number | undefined; // dynamic keys: pct_<id>, rpe_<id>, vol_<id>
}

interface MultiExerciseChartProps {
  series: ExerciseSeries[];
  className?: string;
}

// ============================================
// Tooltip
// ============================================
// eslint-disable-next-line @typescript-eslint/no-explicit-any
function MultiTooltip({ active, payload, label }: any) {
  if (!active || !payload?.length) return null;

  return (
    <div className="bg-background border rounded-lg shadow-lg p-3 text-sm max-w-xs">
      <p className="font-semibold mb-2">{label}</p>
      {/* eslint-disable-next-line @typescript-eslint/no-explicit-any */}
      {payload.map((entry: any, index: number) => (
        <div key={index} className="flex items-center gap-2">
          <div
            className="w-2.5 h-2.5 rounded-full flex-shrink-0"
            style={{ backgroundColor: entry.color }}
          />
          <span className="text-muted-foreground truncate">{entry.name}:</span>
          <span className="font-medium">
            {entry.name.includes('%RM')
              ? `${entry.value}%`
              : entry.name.includes('RPE')
                ? `@${entry.value}`
                : entry.value}
          </span>
        </div>
      ))}
    </div>
  );
}

// ============================================
// Main Component
// ============================================
export function MultiExerciseChart({
  series,
  className,
}: MultiExerciseChartProps) {
  const [metric, setMetric] = React.useState<'percentage' | 'rpe' | 'volume'>('percentage');

  // Merge all series into a single data array keyed by weekNumber
  const { mergedData, seriesInfo } = useMemo(() => {
    const weekMap = new Map<number, MergedDataPoint>();
    const info = series.map((s, i) => ({
      ...s,
      color: SERIES_COLORS[i % SERIES_COLORS.length],
      safeId: s.exerciseId.replace(/[^a-zA-Z0-9]/g, '_'),
    }));

    // Determine max week across all series
    let maxWeek = 0;
    series.forEach((s) => {
      s.instances.forEach((inst) => {
        if (inst.weekNumber > maxWeek) maxWeek = inst.weekNumber;
      });
    });

    // Initialize week points
    for (let w = 1; w <= maxWeek; w++) {
      weekMap.set(w, { week: `S${w}`, weekNumber: w });
    }

    // Fill data per series
    info.forEach((s) => {
      const pctKey = `pct_${s.safeId}`;
      const rpeKey = `rpe_${s.safeId}`;
      const volKey = `vol_${s.safeId}`;

      s.instances.forEach((inst) => {
        const point = weekMap.get(inst.weekNumber);
        if (!point) return;

        if (inst.percentageRM) {
          point[pctKey] = inst.percentageRM;
        }
        if (inst.rpeTarget) {
          point[rpeKey] = inst.rpeTarget;
        }
        const sets = inst.sets || 0;
        const reps = inst.repsMin || 0;
        const volume = sets * reps;
        if (volume > 0) {
          point[volKey] = volume;
        }
      });
    });

    return {
      mergedData: Array.from(weekMap.values()).sort((a, b) => a.weekNumber - b.weekNumber),
      seriesInfo: info,
    };
  }, [series]);

  // Check what data types are available
  const hasPercentage = useMemo(
    () => seriesInfo.some((s) => s.instances.some((i) => i.percentageRM)),
    [seriesInfo]
  );
  const hasRPE = useMemo(
    () => seriesInfo.some((s) => s.instances.some((i) => i.rpeTarget)),
    [seriesInfo]
  );
  const hasVolume = useMemo(
    () => seriesInfo.some((s) => s.instances.some((i) => (i.sets || 0) * (i.repsMin || 0) > 0)),
    [seriesInfo]
  );

  // Auto-select first available metric
  React.useEffect(() => {
    if (metric === 'percentage' && !hasPercentage) {
      if (hasRPE) setMetric('rpe');
      else if (hasVolume) setMetric('volume');
    }
  }, [metric, hasPercentage, hasRPE, hasVolume]);

  if (series.length === 0) {
    return (
      <Card className={cn('', className)}>
        <CardContent className="py-8 text-center text-muted-foreground">
          Selecciona ejercicios para comparar
        </CardContent>
      </Card>
    );
  }

  // Determine which data key to use per series
  const getDataKey = (safeId: string) => {
    switch (metric) {
      case 'percentage':
        return `pct_${safeId}`;
      case 'rpe':
        return `rpe_${safeId}`;
      case 'volume':
        return `vol_${safeId}`;
    }
  };

  const getLabel = (name: string) => {
    switch (metric) {
      case 'percentage':
        return `${name} %RM`;
      case 'rpe':
        return `${name} RPE`;
      case 'volume':
        return `${name} Vol`;
    }
  };

  const yAxisConfig = {
    percentage: { domain: [50, 100] as [number, number], formatter: (v: number) => `${v}%` },
    rpe: { domain: [5, 10] as [number, number], formatter: (v: number) => `@${v}` },
    volume: { domain: ['auto', 'auto'] as [string, string], formatter: (v: number) => `${v}` },
  };

  const axisConfig = yAxisConfig[metric];

  return (
    <Card className={cn('', className)}>
      <CardHeader className="pb-2">
        <div className="flex items-center justify-between">
          <CardTitle className="text-base font-medium">
            📊 Comparación de Ejercicios
          </CardTitle>
          <div className="flex items-center gap-3 text-sm">
            {hasPercentage && (
              <div className="flex items-center gap-1.5">
                <Switch
                  id="cmp-pct"
                  checked={metric === 'percentage'}
                  onCheckedChange={() => setMetric('percentage')}
                  className="scale-75"
                />
                <Label htmlFor="cmp-pct" className="text-xs cursor-pointer">
                  %RM
                </Label>
              </div>
            )}
            {hasRPE && (
              <div className="flex items-center gap-1.5">
                <Switch
                  id="cmp-rpe"
                  checked={metric === 'rpe'}
                  onCheckedChange={() => setMetric('rpe')}
                  className="scale-75"
                />
                <Label htmlFor="cmp-rpe" className="text-xs cursor-pointer">
                  RPE
                </Label>
              </div>
            )}
            {hasVolume && (
              <div className="flex items-center gap-1.5">
                <Switch
                  id="cmp-vol"
                  checked={metric === 'volume'}
                  onCheckedChange={() => setMetric('volume')}
                  className="scale-75"
                />
                <Label htmlFor="cmp-vol" className="text-xs cursor-pointer">
                  Volumen
                </Label>
              </div>
            )}
          </div>
        </div>

        {/* Series legend with colors */}
        <div className="flex flex-wrap gap-3 mt-2">
          {seriesInfo.map((s) => (
            <div key={s.exerciseId} className="flex items-center gap-1.5 text-xs">
              <div
                className="w-3 h-1.5 rounded-full"
                style={{ backgroundColor: s.color }}
              />
              <span className="text-muted-foreground">{s.exerciseName}</span>
              {s.oneRM && (
                <span className="text-muted-foreground/60">(1RM: {s.oneRM}kg)</span>
              )}
            </div>
          ))}
        </div>
      </CardHeader>

      <CardContent>
        <div className="h-64 w-full">
          <ResponsiveContainer width="100%" height="100%">
            <LineChart
              data={mergedData}
              margin={{ top: 5, right: 30, left: 0, bottom: 5 }}
            >
              <CartesianGrid strokeDasharray="3 3" className="stroke-muted" opacity={0.3} />

              <XAxis
                dataKey="week"
                tick={{ fontSize: 12 }}
                className="text-muted-foreground"
              />

              <YAxis
                // eslint-disable-next-line @typescript-eslint/no-explicit-any
                domain={axisConfig.domain as any}
                tick={{ fontSize: 11 }}
                // eslint-disable-next-line @typescript-eslint/no-explicit-any
                tickFormatter={axisConfig.formatter as any}
                className="text-muted-foreground"
              />

              {seriesInfo.map((s) => (
                <Line
                  key={s.exerciseId}
                  type="monotone"
                  dataKey={getDataKey(s.safeId)}
                  name={getLabel(s.exerciseName)}
                  stroke={s.color}
                  strokeWidth={2}
                  dot={{ r: 3.5, fill: s.color }}
                  activeDot={{ r: 5, stroke: '#fff', strokeWidth: 2 }}
                  connectNulls
                />
              ))}

              <Tooltip content={<MultiTooltip />} />
              <Legend
                iconType="circle"
                wrapperStyle={{ fontSize: '11px', paddingTop: '10px' }}
              />
            </LineChart>
          </ResponsiveContainer>
        </div>
      </CardContent>
    </Card>
  );
}
