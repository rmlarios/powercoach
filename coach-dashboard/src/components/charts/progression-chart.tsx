'use client';

import React, { useMemo } from 'react';
import {
  Line,
  XAxis,
  YAxis,
  CartesianGrid,
  Tooltip,
  Legend,
  ResponsiveContainer,
  ReferenceLine,
  ComposedChart,
  Bar,
} from 'recharts';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Switch } from '@/components/ui/switch';
import { Label } from '@/components/ui/label';
import { cn } from '@/utils/cn';
import type { ExerciseProgressionInstance } from '@/types/builder';
import {
  type ChartDataPoint,
  detectBlocks,
  getBlockColor,
  getBlockLabel,
  instancesToChartData,
} from './progression-utils';

interface ProgressionChartProps {
  exerciseName: string;
  instances: ExerciseProgressionInstance[];
  oneRM?: number;
  className?: string;
}

// Types and block detection functions imported from './progression-utils'

// Custom tooltip component
// eslint-disable-next-line @typescript-eslint/no-explicit-any
function CustomTooltip({ active, payload, label }: any) {
  if (!active || !payload?.length) return null;

  return (
    <div className="bg-background border rounded-lg shadow-lg p-3 text-sm">
      <p className="font-semibold mb-2">{label}</p>
      {/* eslint-disable-next-line @typescript-eslint/no-explicit-any */}
      {payload.map((entry: any, index: number) => (
        <div key={index} className="flex items-center gap-2">
          <div
            className="w-3 h-3 rounded-full"
            style={{ backgroundColor: entry.color }}
          />
          <span className="text-muted-foreground">{entry.name}:</span>
          <span className="font-medium">
            {entry.name === '%RM' ? `${entry.value}%` : 
             entry.name === 'RPE' ? `@${entry.value}` :
             entry.name === 'Weight' ? `${entry.value}kg` :
             entry.value}
          </span>
        </div>
      ))}
    </div>
  );
}

export function ProgressionChart({
  exerciseName,
  instances,
  oneRM,
  className,
}: ProgressionChartProps) {
  const [showPercentage, setShowPercentage] = React.useState(true);
  const [showRPE, setShowRPE] = React.useState(true);
  const [showVolume, setShowVolume] = React.useState(false);
  const [showBlocks, setShowBlocks] = React.useState(true);

  // Transform instances to chart data
  const chartData = useMemo<ChartDataPoint[]>(() => {
    return instancesToChartData(instances, oneRM);
  }, [instances, oneRM]);

  // Detect training blocks
  const blocks = useMemo(() => {
    if (!showBlocks) return [];
    return detectBlocks(chartData);
  }, [chartData, showBlocks]);

  if (instances.length === 0) {
    return (
      <Card className={cn('', className)}>
        <CardContent className="py-8 text-center text-muted-foreground">
          No hay datos de progresión para mostrar
        </CardContent>
      </Card>
    );
  }

  // Check if we have any meaningful data to show
  const hasPercentageData = chartData.some((d) => d.percentageRM !== undefined);
  const hasRPEData = chartData.some((d) => d.rpe !== undefined);
  const hasVolumeData = chartData.some((d) => d.volume && d.volume > 0);

  return (
    <Card className={cn('', className)}>
      <CardHeader className="pb-2">
        <div className="flex items-center justify-between">
          <CardTitle className="text-base font-medium">
            📊 Progresión: {exerciseName}
          </CardTitle>
          <div className="flex items-center gap-4 text-sm">
            {hasPercentageData && (
              <div className="flex items-center gap-2">
                <Switch
                  id="showPercentage"
                  checked={showPercentage}
                  onCheckedChange={setShowPercentage}
                  className="scale-75"
                />
                <Label htmlFor="showPercentage" className="text-xs cursor-pointer">
                  %RM
                </Label>
              </div>
            )}
            {hasRPEData && (
              <div className="flex items-center gap-2">
                <Switch
                  id="showRPE"
                  checked={showRPE}
                  onCheckedChange={setShowRPE}
                  className="scale-75"
                />
                <Label htmlFor="showRPE" className="text-xs cursor-pointer">
                  RPE
                </Label>
              </div>
            )}
            {hasVolumeData && (
              <div className="flex items-center gap-2">
                <Switch
                  id="showVolume"
                  checked={showVolume}
                  onCheckedChange={setShowVolume}
                  className="scale-75"
                />
                <Label htmlFor="showVolume" className="text-xs cursor-pointer">
                  Volume
                </Label>
              </div>
            )}
            <div className="flex items-center gap-2">
              <Switch
                id="showBlocks"
                checked={showBlocks}
                onCheckedChange={setShowBlocks}
                className="scale-75"
              />
              <Label htmlFor="showBlocks" className="text-xs cursor-pointer">
                Bloques
              </Label>
            </div>
          </div>
        </div>
        
        {/* Block legend */}
        {showBlocks && blocks.length > 0 && (
          <div className="flex gap-4 mt-2 text-xs text-muted-foreground">
            {Array.from(new Set(blocks.map(b => b.type))).map(type => (
              <div key={type} className="flex items-center gap-1">
                <div 
                  className="w-3 h-3 rounded" 
                  style={{ backgroundColor: getBlockColor(type).replace('0.1', '0.5') }}
                />
                <span>{getBlockLabel(type)}</span>
              </div>
            ))}
          </div>
        )}
      </CardHeader>
      
      <CardContent>
        <div className="h-64 w-full">
          <ResponsiveContainer width="100%" height="100%">
            <ComposedChart
              data={chartData}
              margin={{ top: 5, right: 30, left: 0, bottom: 5 }}
            >
              <CartesianGrid strokeDasharray="3 3" className="stroke-muted" opacity={0.3} />
              
              <XAxis 
                dataKey="week" 
                tick={{ fontSize: 12 }}
                className="text-muted-foreground"
              />
              
              {/* Primary Y-axis for percentage */}
              {showPercentage && hasPercentageData && (
                <YAxis 
                  yAxisId="percentage"
                  domain={[50, 100]}
                  tick={{ fontSize: 11 }}
                  tickFormatter={(v) => `${v}%`}
                  className="text-muted-foreground"
                />
              )}
              
              {/* Secondary Y-axis for RPE */}
              {showRPE && hasRPEData && !showPercentage && (
                <YAxis 
                  yAxisId="rpe"
                  domain={[5, 10]}
                  orientation="right"
                  tick={{ fontSize: 11 }}
                  tickFormatter={(v) => `@${v}`}
                  className="text-muted-foreground"
                />
              )}
              
              {/* Volume bars */}
              {showVolume && hasVolumeData && (
                <Bar
                  yAxisId={showPercentage ? "percentage" : "rpe"}
                  dataKey="volume"
                  name="Volume"
                  fill="rgba(156, 163, 175, 0.3)"
                  radius={[2, 2, 0, 0]}
                />
              )}
              
              {/* Percentage line */}
              {showPercentage && hasPercentageData && (
                <Line
                  yAxisId="percentage"
                  type="monotone"
                  dataKey="percentageRM"
                  name="%RM"
                  stroke="#3b82f6"
                  strokeWidth={2}
                  dot={{ r: 4, fill: '#3b82f6' }}
                  activeDot={{ r: 6, stroke: '#fff', strokeWidth: 2 }}
                  connectNulls
                />
              )}
              
              {/* RPE line */}
              {showRPE && hasRPEData && (
                <Line
                  yAxisId={showPercentage ? "percentage" : "rpe"}
                  type="monotone"
                  dataKey="rpe"
                  name="RPE"
                  stroke="#f59e0b"
                  strokeWidth={2}
                  strokeDasharray="5 5"
                  dot={{ r: 4, fill: '#f59e0b' }}
                  activeDot={{ r: 6, stroke: '#fff', strokeWidth: 2 }}
                  connectNulls
                />
              )}
              
              {/* Weight line (if 1RM available) */}
              {oneRM && showPercentage && (
                <Line
                  yAxisId="percentage"
                  type="monotone"
                  dataKey="weight"
                  name="Weight"
                  stroke="#22c55e"
                  strokeWidth={1.5}
                  dot={false}
                  connectNulls
                  strokeDasharray="2 2"
                />
              )}
              
              {/* Reference lines for common thresholds */}
              {showPercentage && hasPercentageData && (
                <>
                  <ReferenceLine 
                    yAxisId="percentage" 
                    y={70} 
                    stroke="#22c55e" 
                    strokeDasharray="3 3" 
                    strokeOpacity={0.5}
                  />
                  <ReferenceLine 
                    yAxisId="percentage" 
                    y={85} 
                    stroke="#3b82f6" 
                    strokeDasharray="3 3" 
                    strokeOpacity={0.5}
                  />
                </>
              )}
              
              <Tooltip content={<CustomTooltip />} />
              <Legend 
                iconType="circle" 
                wrapperStyle={{ fontSize: '12px', paddingTop: '10px' }}
              />
            </ComposedChart>
          </ResponsiveContainer>
        </div>
        
        {/* Summary stats */}
        <div className="flex gap-6 mt-4 pt-4 border-t text-xs text-muted-foreground">
          {hasPercentageData && (
            <div>
              <span className="font-medium text-foreground">%RM Range:</span>{' '}
              {Math.min(...chartData.filter(d => d.percentageRM).map(d => d.percentageRM!))}% -{' '}
              {Math.max(...chartData.filter(d => d.percentageRM).map(d => d.percentageRM!))}%
            </div>
          )}
          {hasRPEData && (
            <div>
              <span className="font-medium text-foreground">RPE Range:</span>{' '}
              @{Math.min(...chartData.filter(d => d.rpe).map(d => d.rpe!))} -{' '}
              @{Math.max(...chartData.filter(d => d.rpe).map(d => d.rpe!))}
            </div>
          )}
          {oneRM && (
            <div>
              <span className="font-medium text-foreground">1RM:</span> {oneRM}kg
            </div>
          )}
        </div>
      </CardContent>
    </Card>
  );
}
