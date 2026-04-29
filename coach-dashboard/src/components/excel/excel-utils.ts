/**
 * Pure utility functions for Excel generation.
 * Extracted from program-excel-generator.ts for testability.
 */

import type { BuilderWeek } from '@/types/builder';
import type { MaxLift } from '@/types/athlete';
import { calculateWorkingWeight } from '@/utils/weight-calculator';

// ============================================
// Types
// ============================================
export interface DayExerciseForExcel {
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

export interface DayDataForExcel {
  name: string;
  exercises: DayExerciseForExcel[];
}

// ============================================
// Transform Functions
// ============================================

/**
 * Transform weekly builder structure into day-centric structure for Excel export.
 * Groups exercises by dayNumber across all weeks.
 */
export function transformToAllDays(weeks: BuilderWeek[]): Map<number, DayDataForExcel> {
  const dayMap = new Map<number, DayDataForExcel>();

  weeks.forEach((week) => {
    week.days.forEach((day) => {
      if (!dayMap.has(day.dayNumber)) {
        dayMap.set(day.dayNumber, {
          name: day.name || `Day ${day.dayNumber}`,
          exercises: [],
        });
      }

      const dayData = dayMap.get(day.dayNumber)!;

      day.exercises.forEach((exercise) => {
        let exerciseEntry = dayData.exercises.find(
          (e) => e.exerciseId === exercise.exerciseId
        );

        if (!exerciseEntry) {
          exerciseEntry = {
            exerciseId: exercise.exerciseId,
            exerciseName: exercise.exerciseName,
            exerciseType: exercise.exerciseType,
            weekPrescriptions: new Map(),
          };
          dayData.exercises.push(exerciseEntry);
        }

        // Build prescription string
        let repsStr = exercise.rawNotation || '';
        if (!repsStr) {
          const sets = exercise.sets || 0;
          const repsMin = exercise.repsMin || 0;
          const repsMax = exercise.repsMax;
          repsStr = repsMax && repsMax !== repsMin
            ? `${sets}x${repsMin}-${repsMax}`
            : `${sets}x${repsMin}`;
        }

        exerciseEntry.weekPrescriptions.set(week.weekNumber, {
          reps: repsStr,
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
  const lift = maxLifts.find((ml) => ml.exerciseId === exerciseId);
  return lift?.weight || null;
}

/**
 * Format exercise name with type suffix for export.
 */
export function formatExerciseNameForExport(name: string, exerciseType?: string): string {
  if (exerciseType && exerciseType !== 'standard') {
    return `${name} [${exerciseType.toUpperCase()}]`;
  }
  return name;
}

/**
 * Build the overview sheet data as a 2D array.
 */
export function createOverviewData(data: {
  programName: string;
  description?: string;
  athleteName?: string;
  coachName?: string;
  durationWeeks: number;
  startDate?: string;
  athleteMaxLifts?: MaxLift[];
}): (string | number)[][] {
  const rows: (string | number)[][] = [];
  
  rows.push(['PROGRAM OVERVIEW', '']);
  rows.push(['', '']);
  rows.push(['Program Name', data.programName]);
  if (data.description) rows.push(['Description', data.description]);
  if (data.athleteName) rows.push(['Athlete', data.athleteName]);
  if (data.coachName) rows.push(['Coach', data.coachName]);
  rows.push(['Duration', `${data.durationWeeks} weeks`]);
  if (data.startDate) rows.push(['Start Date', data.startDate]);
  rows.push(['', '']);
  rows.push(['Generated', new Date().toLocaleDateString()]);
  
  if (data.athleteMaxLifts && data.athleteMaxLifts.length > 0) {
    rows.push(['', '']);
    rows.push(['ATHLETE MAX LIFTS', '']);
    rows.push(['Exercise', '1RM (kg)']);
    data.athleteMaxLifts.forEach((lift) => {
      rows.push([lift.exerciseName, lift.weight]);
    });
  }
  
  return rows;
}

/**
 * Build a single day sheet as a 2D array.
 */
export function createDaySheetData(
  dayData: DayDataForExcel,
  weekNumbers: number[],
  athleteMaxLifts?: MaxLift[],
  includeWeights: boolean = true,
  weightRoundTo: number = 2.5
): (string | number)[][] {
  const rows: (string | number)[][] = [];
  
  // Header row
  const headerRow: (string | number)[] = ['Exercise'];
  weekNumbers.forEach((weekNum) => headerRow.push(`S${weekNum}`));
  rows.push(headerRow);
  
  // Exercise rows
  dayData.exercises.forEach((exercise) => {
    const oneRM = getExercise1RM(exercise.exerciseId, athleteMaxLifts);
    
    // Main row with prescription
    const displayName = formatExerciseNameForExport(exercise.exerciseName, exercise.exerciseType);
    const exerciseRow: (string | number)[] = [displayName];
    
    weekNumbers.forEach((weekNum) => {
      const prescription = exercise.weekPrescriptions.get(weekNum);
      if (prescription) {
        let cellValue = prescription.reps;
        if (prescription.percentageRM) cellValue += ` ${prescription.percentageRM}%`;
        if (prescription.rpeTarget) cellValue += ` @${prescription.rpeTarget}`;
        exerciseRow.push(cellValue);
      } else {
        exerciseRow.push('');
      }
    });
    rows.push(exerciseRow);
    
    // Weight suggestion row
    if (includeWeights && oneRM) {
      const weightRow: (string | number)[] = [''];
      weekNumbers.forEach((weekNum) => {
        const prescription = exercise.weekPrescriptions.get(weekNum);
        if (prescription?.percentageRM) {
          const weight = calculateWorkingWeight(oneRM, prescription.percentageRM, weightRoundTo);
          weightRow.push(`${weight}kg`);
        } else if (prescription?.weight) {
          weightRow.push(`${prescription.weight}kg`);
        } else {
          weightRow.push('');
        }
      });
      rows.push(weightRow);
    }
    
    // Spacer row
    rows.push(Array(weekNumbers.length + 1).fill(''));
  });
  
  return rows;
}
