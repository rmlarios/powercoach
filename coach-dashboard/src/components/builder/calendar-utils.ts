/**
 * Pure utility functions for the Calendar View.
 * Separated from calendar-view.tsx to allow unit testing without JSX dependencies.
 */

import {
  format,
  startOfMonth,
  endOfMonth,
  startOfWeek,
  endOfWeek,
  addDays,
  parseISO,
} from 'date-fns';
import { BuilderWeek, BuilderExercise } from '@/types/builder';

// Types
export interface CalendarEvent {
  date: Date;
  type: 'training' | 'rest' | 'competition' | 'deload';
  weekNumber: number;
  weekId: string;
  dayNumber: number;
  dayId: string;
  dayName: string;
  focus?: string;
  exercises: BuilderExercise[];
  notes?: string;
}

/**
 * Generate the array of dates to display in a calendar month grid.
 * Starts on Monday and ends on Sunday, including padding from adjacent months.
 */
export function getCalendarDays(date: Date): Date[] {
  const start = startOfWeek(startOfMonth(date), { weekStartsOn: 1 }); // Monday start
  const end = endOfWeek(endOfMonth(date), { weekStartsOn: 1 });
  
  const days: Date[] = [];
  let current = start;
  
  while (current <= end) {
    days.push(current);
    current = addDays(current, 1);
  }
  
  return days;
}

/**
 * Map program weeks/days to calendar dates based on a start date.
 * Returns a Map keyed by 'yyyy-MM-dd' date strings.
 */
export function mapProgramToCalendar(
  weeks: BuilderWeek[],
  startDate: Date,
  competitions: Array<{ date: string; name: string }> = []
): Map<string, CalendarEvent> {
  const eventMap = new Map<string, CalendarEvent>();
  
  weeks.forEach((week) => {
    const weekStartDate = addDays(startDate, (week.weekNumber - 1) * 7);
    const trainingDays = week.days.sort((a, b) => a.dayNumber - b.dayNumber);
    
    trainingDays.forEach((day, index) => {
      const dayOffset = getTrainingDayOffset(trainingDays.length, index);
      const trainingDate = addDays(weekStartDate, dayOffset);
      const dateKey = format(trainingDate, 'yyyy-MM-dd');
      
      const isDeload = week.name?.toLowerCase().includes('deload') || 
                       week.name?.toLowerCase().includes('descarga');
      
      eventMap.set(dateKey, {
        date: trainingDate,
        type: isDeload ? 'deload' : 'training',
        weekNumber: week.weekNumber,
        weekId: week.id,
        dayNumber: day.dayNumber,
        dayId: day.id,
        dayName: day.name || `Day ${day.dayNumber}`,
        focus: day.focus ?? undefined,
        exercises: day.exercises,
        notes: day.notes,
      });
    });
  });
  
  // Add competitions
  competitions.forEach((comp) => {
    const dateKey = comp.date;
    const existingEvent = eventMap.get(dateKey);
    
    if (existingEvent) {
      existingEvent.type = 'competition';
      existingEvent.notes = comp.name;
    } else {
      eventMap.set(dateKey, {
        date: parseISO(comp.date),
        type: 'competition',
        weekNumber: 0,
        weekId: '',
        dayNumber: 0,
        dayId: '',
        dayName: comp.name,
        exercises: [],
        notes: comp.name,
      });
    }
  });
  
  return eventMap;
}

/**
 * Get the day-of-week offset (0=Mon, 6=Sun) for a training day at a given index
 * within a week that has `totalDays` training days.
 * 
 * Uses common training split patterns:
 * - 1 day → Mon
 * - 2 days → Mon, Thu
 * - 3 days → Mon, Wed, Fri
 * - 4 days → Mon, Tue, Thu, Fri
 * - 5 days → Mon-Fri
 * - 6 days → Mon-Sat
 * - 7 days → Mon-Sun
 */
export function getTrainingDayOffset(totalDays: number, index: number): number {
  const splits: Record<number, number[]> = {
    1: [0],                     // Mon
    2: [0, 3],                  // Mon, Thu
    3: [0, 2, 4],               // Mon, Wed, Fri
    4: [0, 1, 3, 4],            // Mon, Tue, Thu, Fri
    5: [0, 1, 2, 3, 4],         // Mon-Fri
    6: [0, 1, 2, 3, 4, 5],      // Mon-Sat
    7: [0, 1, 2, 3, 4, 5, 6],   // Every day
  };
  
  return splits[totalDays]?.[index] ?? index;
}
