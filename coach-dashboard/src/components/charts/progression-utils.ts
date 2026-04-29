/**
 * Pure utility functions for the Progression Chart.
 * Extracted from progression-chart.tsx for testability.
 */

import type { ExerciseProgressionInstance } from '@/types/builder';

export interface ChartDataPoint {
  week: string;
  weekNumber: number;
  percentageRM?: number;
  rpe?: number;
  volume?: number;
  intensity?: number;
  weight?: number;
}

export type BlockType = 'hypertrophy' | 'strength' | 'peaking' | 'deload' | 'unknown';

export interface TrainingBlock {
  startWeek: number;
  endWeek: number;
  type: BlockType;
  color: string;
}

/**
 * Transform exercise instances into chart-ready data points.
 */
export function instancesToChartData(
  instances: ExerciseProgressionInstance[],
  oneRM?: number
): ChartDataPoint[] {
  return instances.map((instance) => {
    const sets = instance.sets || 0;
    const reps = instance.repsMin || 0;
    const volume = sets * reps;

    const weight = oneRM && instance.percentageRM
      ? Math.round((oneRM * instance.percentageRM) / 100 / 2.5) * 2.5
      : undefined;

    return {
      week: `S${instance.weekNumber}`,
      weekNumber: instance.weekNumber,
      percentageRM: instance.percentageRM,
      rpe: instance.rpeTarget,
      volume,
      weight,
    };
  });
}

/**
 * Detect training block boundaries based on %RM patterns.
 */
export function detectBlocks(data: ChartDataPoint[]): TrainingBlock[] {
  if (data.length === 0) return [];

  const blocks: TrainingBlock[] = [];

  let blockStartWeek = data[0].weekNumber;
  let blockType = getBlockTypeForPoint(data[0], null);

  for (let i = 1; i < data.length; i++) {
    const point = data[i];
    const prevPoint = data[i - 1];
    const newBlockType = getBlockTypeForPoint(point, prevPoint);

    if (newBlockType !== blockType) {
      blocks.push({
        startWeek: blockStartWeek,
        endWeek: prevPoint.weekNumber,
        type: blockType,
        color: getBlockColor(blockType),
      });
      blockStartWeek = point.weekNumber;
      blockType = newBlockType;
    }
  }

  // Close the last block
  blocks.push({
    startWeek: blockStartWeek,
    endWeek: data[data.length - 1].weekNumber,
    type: blockType,
    color: getBlockColor(blockType),
  });

  return blocks;
}

/**
 * Determine the block type for a single data point.
 */
export function getBlockTypeForPoint(
  point: ChartDataPoint,
  prevPoint: ChartDataPoint | null
): BlockType {
  // Deload detection: significant drop in %RM
  if (prevPoint && point.percentageRM && prevPoint.percentageRM) {
    if (point.percentageRM < prevPoint.percentageRM - 10) {
      return 'deload';
    }
  }

  // Block classification by %RM threshold
  if (point.percentageRM) {
    if (point.percentageRM <= 70) {
      return 'hypertrophy';
    } else if (point.percentageRM <= 85) {
      return 'strength';
    } else {
      return 'peaking';
    }
  }

  return 'unknown';
}

/**
 * Get the background color for a training block type.
 */
export function getBlockColor(type: BlockType): string {
  switch (type) {
    case 'hypertrophy':
      return 'rgba(34, 197, 94, 0.1)';
    case 'strength':
      return 'rgba(59, 130, 246, 0.1)';
    case 'peaking':
      return 'rgba(239, 68, 68, 0.1)';
    case 'deload':
      return 'rgba(156, 163, 175, 0.15)';
    default:
      return 'rgba(156, 163, 175, 0.05)';
  }
}

/**
 * Get the display label for a training block type.
 */
export function getBlockLabel(type: BlockType): string {
  switch (type) {
    case 'hypertrophy':
      return 'Hypertrophy';
    case 'strength':
      return 'Strength';
    case 'peaking':
      return 'Peaking';
    case 'deload':
      return 'Deload';
    default:
      return '';
  }
}

/**
 * Determine trend direction from a trendKg value.
 */
export function getTrendDirection(trendKg?: number): 'improving' | 'declining' | 'stable' | null {
  if (trendKg === undefined || trendKg === null) return null;
  if (trendKg > 0) return 'improving';
  if (trendKg < 0) return 'declining';
  return 'stable';
}

/**
 * Calculate a suggested starting weight from 1RM and percentage.
 * Rounds to the nearest increment (default 2.5kg).
 */
export function calculateSuggestedWeight(
  oneRM: number,
  percentage: number,
  roundTo: number = 2.5
): number {
  const raw = (oneRM * percentage) / 100;
  return Math.round(raw / roundTo) * roundTo;
}
