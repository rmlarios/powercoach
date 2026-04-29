/**
 * Pure utility functions for PDF generation.
 * Extracted from program-pdf-document.tsx for testability.
 */

import type { BuilderWeek } from '@/types/builder';
import type { MaxLift } from '@/types/athlete';
import { calculateWorkingWeight } from '@/utils/weight-calculator';

// ============================================
// Types
// ============================================
export interface DayExerciseForPDF {
  exerciseId: string;
  exerciseName: string;
  exerciseType?: string;
  weekPrescriptions: Map<number, {
    reps: string;
    percentageRM?: number;
    rpeTarget?: number;
    weight?: number;
  }>;
}

export interface DayDataForPDF {
  name: string;
  exercises: DayExerciseForPDF[];
}

// ============================================
// Transform Functions
// ============================================

/**
 * Transform weekly builder structure into day-centric structure for PDF export.
 * Groups exercises by dayNumber across all weeks.
 */
export function transformToAllDays(weeks: BuilderWeek[]): Map<number, DayDataForPDF> {
  const dayMap = new Map<number, DayDataForPDF>();

  weeks.forEach((week) => {
    week.days.forEach((day) => {
      if (!dayMap.has(day.dayNumber)) {
        dayMap.set(day.dayNumber, {
          name: day.name || `Day ${day.dayNumber}`,
          exercises: [],
        });
      }

      const daySlot = dayMap.get(day.dayNumber)!;

      day.exercises.forEach((exercise) => {
        let exerciseSlot = daySlot.exercises.find(
          (e) => e.exerciseId === exercise.exerciseId
        );

        if (!exerciseSlot) {
          exerciseSlot = {
            exerciseId: exercise.exerciseId,
            exerciseName: exercise.exerciseName,
            exerciseType: exercise.exerciseType,
            weekPrescriptions: new Map(),
          };
          daySlot.exercises.push(exerciseSlot);
        }

        const repsDisplay = exercise.rawNotation ||
          `${exercise.sets}x${exercise.repsMin}${exercise.repsMax !== exercise.repsMin ? `-${exercise.repsMax}` : ''}`;

        exerciseSlot.weekPrescriptions.set(week.weekNumber, {
          reps: repsDisplay,
          percentageRM: exercise.percentageRM,
          rpeTarget: exercise.rpeTarget,
          weight: exercise.weight,
        });
      });
    });
  });

  return dayMap;
}

/**
 * Get 1RM for an exercise from max lifts array.
 */
export function getExercise1RM(exerciseId: string, maxLifts?: MaxLift[]): number | null {
  if (!maxLifts) return null;
  const lift = maxLifts.find((l) => l.exerciseId === exerciseId);
  return lift?.weight ?? null;
}

/**
 * Format weight for display in PDF.
 */
export function formatWeight(weight: number): string {
  const rounded = Math.round(weight * 10) / 10;
  return `${rounded}kg`;
}

/**
 * Calculate weight suggestion for a prescription given 1RM.
 */
export function calculateWeightForPrescription(
  percentageRM: number | undefined,
  oneRM: number | null,
  roundTo: number = 2.5
): number | null {
  if (!percentageRM || !oneRM) return null;
  return calculateWorkingWeight(oneRM, percentageRM, roundTo);
}

/**
 * Build a condensed day summary for the PDF (exercise count, focus).
 */
export function getDaySummary(dayData: DayDataForPDF): string {
  const exerciseCount = dayData.exercises.length;
  return `${exerciseCount} exercise${exerciseCount !== 1 ? 's' : ''}`;
}
