import { format } from 'date-fns';
import {
  getCalendarDays,
  mapProgramToCalendar,
  getTrainingDayOffset,
} from '@/components/builder/calendar-utils';
import { BuilderWeek, BuilderDay, BuilderExercise } from '@/types/builder';

// Helper to create a mock exercise
function mockExercise(overrides: Partial<BuilderExercise> = {}): BuilderExercise {
  return {
    id: 'ex-1',
    exerciseId: 'squat-1',
    exerciseName: 'Back Squat',
    order: 1,
    sets: 3,
    repsMin: 5,
    repsMax: 5,
    restSeconds: 180,
    ...overrides,
  };
}

// Helper to create a mock day
function mockDay(overrides: Partial<BuilderDay> = {}): BuilderDay {
  return {
    id: `day-${overrides.dayNumber ?? 1}`,
    dayNumber: 1,
    name: 'Day 1',
    focus: 'FullBody',
    exercises: [],
    ...overrides,
  };
}

// Helper to create a mock week
function mockWeek(overrides: Partial<BuilderWeek> = {}): BuilderWeek {
  return {
    id: `week-${overrides.weekNumber ?? 1}`,
    weekNumber: 1,
    name: 'Week 1',
    days: [],
    ...overrides,
  };
}

describe('calendar-view utilities', () => {
  // ========== getCalendarDays ==========
  describe('getCalendarDays', () => {
    it('should return an array of dates', () => {
      const date = new Date(2025, 0, 1); // Jan 2025
      const days = getCalendarDays(date);
      expect(days.length).toBeGreaterThan(0);
      expect(days[0]).toBeInstanceOf(Date);
    });

    it('should start on Monday (weekStartsOn: 1)', () => {
      const date = new Date(2025, 0, 1); // Jan 2025
      const days = getCalendarDays(date);
      // First day should be a Monday (getDay() === 1)
      expect(days[0].getDay()).toBe(1);
    });

    it('should end on Sunday', () => {
      const date = new Date(2025, 0, 1);
      const days = getCalendarDays(date);
      // Last day should be a Sunday (getDay() === 0)
      expect(days[days.length - 1].getDay()).toBe(0);
    });

    it('should always return a multiple of 7 days (complete weeks)', () => {
      // Test several months
      const months = [
        new Date(2025, 0, 1), // Jan
        new Date(2025, 1, 1), // Feb
        new Date(2025, 5, 1), // Jun
        new Date(2025, 11, 1), // Dec
      ];

      months.forEach((date) => {
        const days = getCalendarDays(date);
        expect(days.length % 7).toBe(0);
      });
    });

    it('should include padding days from adjacent months', () => {
      // Jan 2025 starts on Wednesday, so should have Mon Dec 30, Tue Dec 31 as padding
      const date = new Date(2025, 0, 1);
      const days = getCalendarDays(date);
      
      // First day should be from December 2024 (Monday Dec 30)
      expect(days[0].getMonth()).toBe(11); // December = 11
      expect(days[0].getDate()).toBe(30);
    });

    it('should cover all days of the target month', () => {
      const date = new Date(2025, 0, 1); // Jan 2025 has 31 days
      const days = getCalendarDays(date);
      
      const janDays = days.filter((d) => d.getMonth() === 0);
      expect(janDays.length).toBe(31);
    });

    it('should handle February in a non-leap year', () => {
      const date = new Date(2025, 1, 1); // Feb 2025 (not leap)
      const days = getCalendarDays(date);
      
      const febDays = days.filter((d) => d.getMonth() === 1);
      expect(febDays.length).toBe(28);
    });

    it('should handle February in a leap year', () => {
      const date = new Date(2024, 1, 1); // Feb 2024 (leap year)
      const days = getCalendarDays(date);
      
      const febDays = days.filter((d) => d.getMonth() === 1);
      expect(febDays.length).toBe(29);
    });

    it('should return between 28 and 42 days (4-6 rows)', () => {
      const date = new Date(2025, 0, 1);
      const days = getCalendarDays(date);
      expect(days.length).toBeGreaterThanOrEqual(28);
      expect(days.length).toBeLessThanOrEqual(42);
    });
  });

  // ========== getTrainingDayOffset ==========
  describe('getTrainingDayOffset', () => {
    it('should return 0 (Monday) for 1-day split', () => {
      expect(getTrainingDayOffset(1, 0)).toBe(0);
    });

    it('should return Mon/Thu for 2-day split', () => {
      expect(getTrainingDayOffset(2, 0)).toBe(0); // Mon
      expect(getTrainingDayOffset(2, 1)).toBe(3); // Thu
    });

    it('should return Mon/Wed/Fri for 3-day split', () => {
      expect(getTrainingDayOffset(3, 0)).toBe(0); // Mon
      expect(getTrainingDayOffset(3, 1)).toBe(2); // Wed
      expect(getTrainingDayOffset(3, 2)).toBe(4); // Fri
    });

    it('should return Mon/Tue/Thu/Fri for 4-day split', () => {
      expect(getTrainingDayOffset(4, 0)).toBe(0); // Mon
      expect(getTrainingDayOffset(4, 1)).toBe(1); // Tue
      expect(getTrainingDayOffset(4, 2)).toBe(3); // Thu
      expect(getTrainingDayOffset(4, 3)).toBe(4); // Fri
    });

    it('should return Mon-Fri for 5-day split', () => {
      for (let i = 0; i < 5; i++) {
        expect(getTrainingDayOffset(5, i)).toBe(i);
      }
    });

    it('should return Mon-Sat for 6-day split', () => {
      for (let i = 0; i < 6; i++) {
        expect(getTrainingDayOffset(6, i)).toBe(i);
      }
    });

    it('should return Mon-Sun for 7-day split', () => {
      for (let i = 0; i < 7; i++) {
        expect(getTrainingDayOffset(7, i)).toBe(i);
      }
    });

    it('should fallback to index for unknown split counts', () => {
      // 0 days isn't in the splits table
      expect(getTrainingDayOffset(0, 0)).toBe(0);
      expect(getTrainingDayOffset(0, 3)).toBe(3);
      // 8+ days would be clamped to 7 in practice but fallback to index
      expect(getTrainingDayOffset(8, 2)).toBe(2);
    });
  });

  // ========== mapProgramToCalendar ==========
  describe('mapProgramToCalendar', () => {
    // Use a fixed Monday as start date for predictable results
    const startDate = new Date(2025, 0, 6); // Monday Jan 6, 2025

    it('should return an empty map for empty weeks', () => {
      const result = mapProgramToCalendar([], startDate);
      expect(result.size).toBe(0);
    });

    it('should map a single week with 1 training day to Monday', () => {
      const weeks: BuilderWeek[] = [
        mockWeek({
          weekNumber: 1,
          days: [mockDay({ dayNumber: 1, name: 'Squat Day' })],
        }),
      ];

      const result = mapProgramToCalendar(weeks, startDate);
      expect(result.size).toBe(1);
      
      const mondayKey = format(startDate, 'yyyy-MM-dd'); // 2025-01-06
      const event = result.get(mondayKey);
      expect(event).toBeDefined();
      expect(event!.type).toBe('training');
      expect(event!.weekNumber).toBe(1);
      expect(event!.dayNumber).toBe(1);
      expect(event!.dayName).toBe('Squat Day');
    });

    it('should distribute 3 training days as Mon/Wed/Fri', () => {
      const weeks: BuilderWeek[] = [
        mockWeek({
          weekNumber: 1,
          days: [
            mockDay({ dayNumber: 1, name: 'Day 1' }),
            mockDay({ dayNumber: 2, name: 'Day 2' }),
            mockDay({ dayNumber: 3, name: 'Day 3' }),
          ],
        }),
      ];

      const result = mapProgramToCalendar(weeks, startDate);
      expect(result.size).toBe(3);

      // Mon Jan 6
      expect(result.get('2025-01-06')).toBeDefined();
      expect(result.get('2025-01-06')!.dayNumber).toBe(1);
      // Wed Jan 8
      expect(result.get('2025-01-08')).toBeDefined();
      expect(result.get('2025-01-08')!.dayNumber).toBe(2);
      // Fri Jan 10
      expect(result.get('2025-01-10')).toBeDefined();
      expect(result.get('2025-01-10')!.dayNumber).toBe(3);
    });

    it('should distribute 4 training days as Mon/Tue/Thu/Fri', () => {
      const weeks: BuilderWeek[] = [
        mockWeek({
          weekNumber: 1,
          days: [
            mockDay({ dayNumber: 1 }),
            mockDay({ dayNumber: 2 }),
            mockDay({ dayNumber: 3 }),
            mockDay({ dayNumber: 4 }),
          ],
        }),
      ];

      const result = mapProgramToCalendar(weeks, startDate);
      expect(result.size).toBe(4);

      // Mon=0, Tue=1, Thu=3, Fri=4
      expect(result.get('2025-01-06')).toBeDefined(); // Mon
      expect(result.get('2025-01-07')).toBeDefined(); // Tue
      expect(result.get('2025-01-09')).toBeDefined(); // Thu
      expect(result.get('2025-01-10')).toBeDefined(); // Fri
    });

    it('should correctly offset week 2 by 7 days', () => {
      const weeks: BuilderWeek[] = [
        mockWeek({
          weekNumber: 1,
          days: [mockDay({ dayNumber: 1 })],
        }),
        mockWeek({
          weekNumber: 2,
          id: 'week-2',
          days: [mockDay({ id: 'w2d1', dayNumber: 1, name: 'W2 Day 1' })],
        }),
      ];

      const result = mapProgramToCalendar(weeks, startDate);
      
      // Week 1 → Jan 6 (Mon)
      expect(result.get('2025-01-06')).toBeDefined();
      // Week 2 → Jan 13 (Mon, 7 days later)
      const w2Event = result.get('2025-01-13');
      expect(w2Event).toBeDefined();
      expect(w2Event!.weekNumber).toBe(2);
      expect(w2Event!.dayName).toBe('W2 Day 1');
    });

    it('should include exercises in the event', () => {
      const exercises = [
        mockExercise({ exerciseName: 'Squat', sets: 5, repsMin: 5 }),
        mockExercise({ id: 'ex-2', exerciseName: 'Bench', sets: 3, repsMin: 8 }),
      ];

      const weeks: BuilderWeek[] = [
        mockWeek({
          weekNumber: 1,
          days: [mockDay({ dayNumber: 1, exercises })],
        }),
      ];

      const result = mapProgramToCalendar(weeks, startDate);
      const event = result.get('2025-01-06')!;
      
      expect(event.exercises.length).toBe(2);
      expect(event.exercises[0].exerciseName).toBe('Squat');
      expect(event.exercises[1].exerciseName).toBe('Bench');
    });

    it('should mark deload weeks with type "deload"', () => {
      const weeks: BuilderWeek[] = [
        mockWeek({
          weekNumber: 1,
          name: 'Deload Week',
          days: [mockDay({ dayNumber: 1 })],
        }),
      ];

      const result = mapProgramToCalendar(weeks, startDate);
      const event = result.get('2025-01-06')!;
      expect(event.type).toBe('deload');
    });

    it('should mark descarga weeks as deload', () => {
      const weeks: BuilderWeek[] = [
        mockWeek({
          weekNumber: 1,
          name: 'Semana de Descarga',
          days: [mockDay({ dayNumber: 1 })],
        }),
      ];

      const result = mapProgramToCalendar(weeks, startDate);
      const event = result.get('2025-01-06')!;
      expect(event.type).toBe('deload');
    });

    it('should include weekId and dayId in events', () => {
      const weeks: BuilderWeek[] = [
        mockWeek({
          weekNumber: 1,
          id: 'wk-abc',
          days: [mockDay({ id: 'day-xyz', dayNumber: 1 })],
        }),
      ];

      const result = mapProgramToCalendar(weeks, startDate);
      const event = result.get('2025-01-06')!;
      expect(event.weekId).toBe('wk-abc');
      expect(event.dayId).toBe('day-xyz');
    });

    describe('competition overlay', () => {
      it('should add a competition event on an empty date', () => {
        const competitions = [
          { date: '2025-01-11', name: 'Nationals' },
        ];

        const result = mapProgramToCalendar([], startDate, competitions);
        const event = result.get('2025-01-11');
        
        expect(event).toBeDefined();
        expect(event!.type).toBe('competition');
        expect(event!.dayName).toBe('Nationals');
        expect(event!.notes).toBe('Nationals');
        expect(event!.weekId).toBe('');
        expect(event!.dayId).toBe('');
      });

      it('should override existing training day type to competition', () => {
        const weeks: BuilderWeek[] = [
          mockWeek({
            weekNumber: 1,
            days: [mockDay({ dayNumber: 1, name: 'Training' })],
          }),
        ];
        const competitions = [
          { date: '2025-01-06', name: 'Meet Day' },
        ];

        const result = mapProgramToCalendar(weeks, startDate, competitions);
        const event = result.get('2025-01-06')!;
        
        expect(event.type).toBe('competition');
        expect(event.notes).toBe('Meet Day');
        // Original training data should still be there
        expect(event.dayName).toBe('Training');
      });
    });

    describe('multi-week programs', () => {
      it('should correctly map a 4-week program', () => {
        const weeks: BuilderWeek[] = Array.from({ length: 4 }, (_, i) =>
          mockWeek({
            weekNumber: i + 1,
            id: `week-${i + 1}`,
            name: i === 3 ? 'Deload' : `Week ${i + 1}`,
            days: [
              mockDay({ id: `w${i+1}d1`, dayNumber: 1 }),
              mockDay({ id: `w${i+1}d2`, dayNumber: 2 }),
              mockDay({ id: `w${i+1}d3`, dayNumber: 3 }),
            ],
          })
        );

        const result = mapProgramToCalendar(weeks, startDate);
        
        // 4 weeks × 3 days = 12 events
        expect(result.size).toBe(12);
        
        // Week 1: Mon Jan 6, Wed Jan 8, Fri Jan 10
        expect(result.get('2025-01-06')!.weekNumber).toBe(1);
        expect(result.get('2025-01-08')!.weekNumber).toBe(1);
        expect(result.get('2025-01-10')!.weekNumber).toBe(1);
        
        // Week 4 (deload): Mon Jan 27, Wed Jan 29, Fri Jan 31
        expect(result.get('2025-01-27')!.type).toBe('deload');
        expect(result.get('2025-01-29')!.type).toBe('deload');
        expect(result.get('2025-01-31')!.type).toBe('deload');
      });
    });

    it('should set focus field from day focus', () => {
      const weeks: BuilderWeek[] = [
        mockWeek({
          weekNumber: 1,
          days: [mockDay({ dayNumber: 1, focus: 'Push', name: 'Push Day' })],
        }),
      ];

      const result = mapProgramToCalendar(weeks, startDate);
      const event = result.get('2025-01-06')!;
      expect(event.focus).toBe('Push');
    });

    it('should set notes field from day notes', () => {
      const weeks: BuilderWeek[] = [
        mockWeek({
          weekNumber: 1,
          days: [mockDay({ dayNumber: 1, notes: 'Easy session' })],
        }),
      ];

      const result = mapProgramToCalendar(weeks, startDate);
      const event = result.get('2025-01-06')!;
      expect(event.notes).toBe('Easy session');
    });

    it('should use "Day N" as default name when day.name is empty', () => {
      const weeks: BuilderWeek[] = [
        mockWeek({
          weekNumber: 1,
          days: [mockDay({ dayNumber: 3, name: '' })],
        }),
      ];

      const result = mapProgramToCalendar(weeks, startDate);
      // 1 day → offset 0 → Mon Jan 6
      const event = result.get('2025-01-06')!;
      expect(event.dayName).toBe('Day 3');
    });
  });
});
