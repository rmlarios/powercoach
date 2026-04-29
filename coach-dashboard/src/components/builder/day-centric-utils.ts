/**
 * Pure utility functions for the Day-Centric View.
 * Extracted from day-centric-view.tsx for testability.
 */

import {
  BuilderWeek,
  DaySlot,
} from '@/types/builder';
import { parseSetNotation } from '@/utils/set-notation-parser';

/**
 * Transform the weekly structure into day-centric view.
 * Groups exercises by their dayNumber across all weeks.
 */
export function transformToDaySlots(weeks: BuilderWeek[]): DaySlot[] {
  const dayMap = new Map<number, DaySlot>();

  weeks.forEach((week) => {
    week.days.forEach((day) => {
      let daySlot = dayMap.get(day.dayNumber);
      if (!daySlot) {
        daySlot = {
          dayNumber: day.dayNumber,
          name: day.name || `Day ${day.dayNumber}`,
          focus: day.focus ?? '',
          exercises: [],
        };
        dayMap.set(day.dayNumber, daySlot);
      }

      day.exercises.forEach((exercise) => {
        const exerciseKey = exercise.exerciseId || exercise.exerciseName;

        let exerciseSlot = daySlot!.exercises.find(
          (e) => e.exerciseId === exerciseKey || e.exerciseName === exercise.exerciseName
        );

        if (!exerciseSlot) {
          exerciseSlot = {
            exerciseId: exerciseKey,
            exerciseName: exercise.exerciseName,
            order: exercise.order,
            weekPrescriptions: new Map(),
            exerciseType: exercise.exerciseType,
            emomConfig: exercise.emomConfig,
            tempoConfig: exercise.tempoConfig,
            supersetConfig: exercise.supersetConfig,
          };
          daySlot!.exercises.push(exerciseSlot);
        }

        const repsDisplay = exercise.rawNotation ||
          `${exercise.sets}x${exercise.repsMin}${exercise.repsMax !== exercise.repsMin ? `-${exercise.repsMax}` : ''}`;

        exerciseSlot.weekPrescriptions.set(week.weekNumber, {
          weekId: week.id,
          dayId: day.id,
          exerciseEntryId: exercise.id,
          sets: exercise.sets,
          reps: repsDisplay,
          rpeTarget: exercise.rpeTarget,
          percentageRM: exercise.percentageRM,
          weight: exercise.weight,
          restSeconds: exercise.restSeconds,
          notes: exercise.notes,
        });
      });
    });
  });

  const daySlots = Array.from(dayMap.values()).sort((a, b) => a.dayNumber - b.dayNumber);
  daySlots.forEach((slot) => {
    slot.exercises.sort((a, b) => a.order - b.order);
  });

  return daySlots;
}

/**
 * Generate a human-readable tooltip for a set notation string.
 */
export function getNotationTooltip(notation: string): string {
  const parsed = parseSetNotation(notation);

  if (!parsed.isValid) {
    return `Notación: "${notation}" (no válida)`;
  }

  const parts: string[] = [];

  for (let i = 0; i < parsed.groups.length; i++) {
    const group = parsed.groups[i];
    let groupStr = '';

    if (group.isAMRAP) {
      groupStr = 'AMRAP';
    } else if (group.sets === 1 && group.repsMin === 1) {
      groupStr = `Single`;
      if (group.rpe) groupStr += ` @${group.rpe}`;
    } else {
      groupStr = `${group.sets}x${group.repsMin}`;
      if (group.repsMax && group.repsMax !== group.repsMin) {
        groupStr += `-${group.repsMax}`;
      }
      if (group.percentage) {
        groupStr += ` ${group.percentage}%`;
      } else if (group.rpe) {
        groupStr += ` @${group.rpe}`;
      } else if (group.weight) {
        groupStr += ` @${group.weight}kg`;
      }
    }
    parts.push(groupStr);
  }

  let result = parts.join(' + ');

  if (parsed.tempo) {
    result += ` | Tempo: ${parsed.tempo}`;
  }
  if (parsed.emomMinutes) {
    result += ` | EMOM: ${parsed.emomMinutes} min`;
  }

  return result;
}
