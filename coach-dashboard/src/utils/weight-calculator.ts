/**
 * Weight calculation utilities for strength training
 * 
 * Provides 1RM estimation formulas and working weight calculations
 */

/**
 * Epley formula: weight × (1 + reps/30)
 * Best for reps ≤ 10
 */
export function epley(weight: number, reps: number): number {
  if (reps <= 0) return weight;
  if (reps === 1) return weight;
  return weight * (1 + reps / 30);
}

/**
 * Brzycki formula: weight × 36 / (37 - reps)
 * Most accurate for 1-10 reps
 */
export function brzycki(weight: number, reps: number): number {
  if (reps <= 0) return weight;
  if (reps === 1) return weight;
  if (reps >= 37) return weight * 3; // Prevent division by zero/negative
  return weight * 36 / (37 - reps);
}

/**
 * Calculate estimated 1RM using the average of Epley and Brzycki
 */
export function estimateOneRepMax(weight: number, reps: number): number {
  return (epley(weight, reps) + brzycki(weight, reps)) / 2;
}

/**
 * Calculate working weight from 1RM and percentage
 * @param oneRepMax The 1RM weight
 * @param percentage Target percentage (e.g., 75 for 75%)
 * @param roundTo Round to nearest increment (e.g., 2.5 or 5). Use 0 for no rounding.
 */
export function calculateWorkingWeight(
  oneRepMax: number,
  percentage: number,
  roundTo: number = 2.5
): number {
  const targetWeight = oneRepMax * (percentage / 100);
  if (roundTo <= 0) return targetWeight;
  return Math.round(targetWeight / roundTo) * roundTo;
}

/**
 * Get estimated reps for a given percentage of 1RM
 * Based on Prilepin's Chart / percentage-rep relationship
 */
export function estimatedRepsForPercentage(percentage: number): { min: number; max: number; optimal: number } {
  if (percentage >= 90) return { min: 1, max: 2, optimal: 1 };
  if (percentage >= 85) return { min: 2, max: 4, optimal: 3 };
  if (percentage >= 80) return { min: 3, max: 5, optimal: 4 };
  if (percentage >= 75) return { min: 4, max: 6, optimal: 5 };
  if (percentage >= 70) return { min: 5, max: 8, optimal: 6 };
  if (percentage >= 65) return { min: 6, max: 10, optimal: 8 };
  if (percentage >= 60) return { min: 8, max: 12, optimal: 10 };
  return { min: 10, max: 15, optimal: 12 };
}

/**
 * Get estimated RPE for a given weight relative to 1RM
 * @param weight Working weight
 * @param oneRepMax 1RM
 * @param reps Number of reps
 */
export function estimateRPE(weight: number, oneRepMax: number, reps: number): number {
  const percentage = (weight / oneRepMax) * 100;
  const estimated = estimatedRepsForPercentage(percentage);
  
  // If doing fewer reps than optimal, RPE is lower
  if (reps <= estimated.min) return 6;
  if (reps <= estimated.optimal - 1) return 7;
  if (reps <= estimated.optimal) return 8;
  if (reps <= estimated.optimal + 1) return 9;
  return 10; // Near failure
}

/**
 * Format weight for display with unit
 */
export function formatWeight(weight: number, unit: 'kg' | 'lb' = 'kg'): string {
  const rounded = Math.round(weight * 10) / 10; // Round to 1 decimal
  return `${rounded}${unit}`;
}

/**
 * Convert between kg and lb
 */
export function convertWeight(weight: number, from: 'kg' | 'lb', to: 'kg' | 'lb'): number {
  if (from === to) return weight;
  if (from === 'kg' && to === 'lb') return weight * 2.20462;
  return weight / 2.20462; // lb to kg
}

/**
 * Common rounding increments for different equipment
 */
export const ROUNDING_OPTIONS = [
  { value: 1, label: '1kg', description: 'Micro plates' },
  { value: 2.5, label: '2.5kg', description: 'Standard plates' },
  { value: 5, label: '5kg', description: 'Large plates only' },
  { value: 0, label: 'No rounding', description: 'Exact calculation' },
];

/**
 * Calculate a series of progressive weights
 * @param oneRepMax Starting 1RM
 * @param percentages Array of percentages for each week
 * @param roundTo Rounding increment
 */
export function calculateProgressiveWeights(
  oneRepMax: number,
  percentages: number[],
  roundTo: number = 2.5
): number[] {
  return percentages.map(p => calculateWorkingWeight(oneRepMax, p, roundTo));
}

/**
 * Extract percentage from notation string (e.g., "3x5 @75%" -> 75)
 */
export function extractPercentageFromNotation(notation: string): number | null {
  const match = notation.match(/(\d+(?:\.\d+)?)\s*%/);
  if (!match) return null;
  return parseFloat(match[1]);
}

/**
 * Create a weight suggestion string for UI display
 */
export function formatWeightSuggestion(
  oneRepMax: number | null,
  percentage: number | null,
  roundTo: number = 2.5
): string | null {
  if (!oneRepMax || !percentage) return null;
  const weight = calculateWorkingWeight(oneRepMax, percentage, roundTo);
  return formatWeight(weight);
}
