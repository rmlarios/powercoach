import { ProgramTemplate } from '@/types/program';
import { ExerciseOption } from '@/types/builder';
import { Athlete, MaxLift, AthleteMaxLifts } from '@/types/athlete';

// Mock program template for development without backend
export const mockProgram: ProgramTemplate = {
  id: 'mock-program-1',
  coachId: '00000000-0000-0000-0000-000000000001',
  name: 'Programa de Fuerza 12 Semanas',
  description: 'Programa de powerlifting enfocado en los tres grandes levantamientos',
  durationWeeks: 12,
  isActive: true,
  createdAt: '2026-03-01T00:00:00Z',
  weeks: [
    {
      id: 'week-1',
      weekNumber: 1,
      name: 'Semana 1 - Adaptación',
      notes: 'Semana de introducción con cargas moderadas',
      days: [
        {
          id: 'day-1-1',
          dayNumber: 1,
          name: 'Push Day',
          focus: 'Push',
          notes: 'Enfoque en pecho y hombros',
          exercises: [
            {
              id: 'ex-1-1-1',
              exerciseId: 'bench-press',
              exerciseName: 'Bench Press',
              order: 1,
              sets: 4,
              reps: '6-8',
              restSeconds: 180,
              targetRpe: 7,
              notes: 'Pausa en el pecho',
            },
            {
              id: 'ex-1-1-2',
              exerciseId: 'ohp',
              exerciseName: 'Overhead Press',
              order: 2,
              sets: 3,
              reps: '8-10',
              restSeconds: 120,
              targetRpe: 7,
            },
            {
              id: 'ex-1-1-3',
              exerciseId: 'dips',
              exerciseName: 'Dips',
              order: 3,
              sets: 3,
              reps: '10-12',
              restSeconds: 90,
              targetRpe: 8,
            },
          ],
        },
        {
          id: 'day-1-2',
          dayNumber: 2,
          name: 'Pull Day',
          focus: 'Pull',
          notes: 'Enfoque en espalda',
          exercises: [
            {
              id: 'ex-1-2-1',
              exerciseId: 'deadlift',
              exerciseName: 'Deadlift',
              order: 1,
              sets: 4,
              reps: '5-6',
              restSeconds: 240,
              targetRpe: 7,
              notes: 'Convencional',
            },
            {
              id: 'ex-1-2-2',
              exerciseId: 'barbell-row',
              exerciseName: 'Barbell Row',
              order: 2,
              sets: 4,
              reps: '8-10',
              restSeconds: 120,
              targetRpe: 8,
            },
            {
              id: 'ex-1-2-3',
              exerciseId: 'pull-ups',
              exerciseName: 'Pull-ups',
              order: 3,
              sets: 3,
              reps: '6-10',
              restSeconds: 90,
              targetRpe: 8,
            },
          ],
        },
        {
          id: 'day-1-3',
          dayNumber: 3,
          name: 'Legs Day',
          focus: 'Legs',
          notes: 'Enfoque en sentadilla',
          exercises: [
            {
              id: 'ex-1-3-1',
              exerciseId: 'squat',
              exerciseName: 'Back Squat',
              order: 1,
              sets: 4,
              reps: '6-8',
              restSeconds: 180,
              targetRpe: 7,
              notes: 'High bar',
            },
            {
              id: 'ex-1-3-2',
              exerciseId: 'leg-press',
              exerciseName: 'Leg Press',
              order: 2,
              sets: 3,
              reps: '10-12',
              restSeconds: 120,
              targetRpe: 8,
            },
            {
              id: 'ex-1-3-3',
              exerciseId: 'rdl',
              exerciseName: 'Romanian Deadlift',
              order: 3,
              sets: 3,
              reps: '10-12',
              restSeconds: 90,
              targetRpe: 7,
            },
          ],
        },
      ],
    },
    {
      id: 'week-2',
      weekNumber: 2,
      name: 'Semana 2 - Volumen',
      notes: 'Incremento de volumen',
      days: [
        {
          id: 'day-2-1',
          dayNumber: 1,
          name: 'Upper Body',
          focus: 'UpperBody',
          exercises: [
            {
              id: 'ex-2-1-1',
              exerciseId: 'bench-press',
              exerciseName: 'Bench Press',
              order: 1,
              sets: 5,
              reps: '5-6',
              restSeconds: 180,
              targetRpe: 8,
            },
          ],
        },
      ],
    },
  ],
};

// Mock exercises for autocomplete
export const mockExerciseOptions: ExerciseOption[] = [
  { id: 'bench-press', name: 'Bench Press', category: 'Strength', muscleGroup: 'Chest' },
  { id: 'squat', name: 'Back Squat', category: 'Strength', muscleGroup: 'Legs' },
  { id: 'deadlift', name: 'Deadlift', category: 'Strength', muscleGroup: 'Back' },
  { id: 'ohp', name: 'Overhead Press', category: 'Strength', muscleGroup: 'Shoulders' },
  { id: 'barbell-row', name: 'Barbell Row', category: 'Strength', muscleGroup: 'Back' },
  { id: 'pull-ups', name: 'Pull-ups', category: 'Strength', muscleGroup: 'Back' },
  { id: 'dips', name: 'Dips', category: 'Strength', muscleGroup: 'Chest' },
  { id: 'leg-press', name: 'Leg Press', category: 'Strength', muscleGroup: 'Legs' },
  { id: 'rdl', name: 'Romanian Deadlift', category: 'Strength', muscleGroup: 'Legs' },
  { id: 'incline-bench', name: 'Incline Bench Press', category: 'Strength', muscleGroup: 'Chest' },
  { id: 'front-squat', name: 'Front Squat', category: 'Strength', muscleGroup: 'Legs' },
  { id: 'sumo-deadlift', name: 'Sumo Deadlift', category: 'Strength', muscleGroup: 'Back' },
  { id: 'lat-pulldown', name: 'Lat Pulldown', category: 'Strength', muscleGroup: 'Back' },
  { id: 'cable-row', name: 'Cable Row', category: 'Strength', muscleGroup: 'Back' },
  { id: 'leg-curl', name: 'Leg Curl', category: 'Strength', muscleGroup: 'Legs' },
  { id: 'leg-extension', name: 'Leg Extension', category: 'Strength', muscleGroup: 'Legs' },
  { id: 'tricep-pushdown', name: 'Tricep Pushdown', category: 'Strength', muscleGroup: 'Arms' },
  { id: 'bicep-curl', name: 'Bicep Curl', category: 'Strength', muscleGroup: 'Arms' },
  { id: 'face-pull', name: 'Face Pull', category: 'Strength', muscleGroup: 'Shoulders' },
  { id: 'lateral-raise', name: 'Lateral Raise', category: 'Strength', muscleGroup: 'Shoulders' },
];

// Mock athletes for development
export const mockAthletes: Athlete[] = [
  {
    id: 'athlete-1',
    coachId: '00000000-0000-0000-0000-000000000001',
    firstName: 'Carlos',
    lastName: 'García',
    fullName: 'Carlos García',
    email: 'carlos@example.com',
    phone: '+598 99 123 456',
    dateOfBirth: '1995-03-15',
    country: 'Uruguay',
    status: 'Active',
    goals: 'Powerlifting competitivo',
    notes: 'Powerlifter intermedio',
    startDate: '2024-01-15T00:00:00Z',
    height: 178,
    weight: 85,
    createdAt: '2024-01-15T00:00:00Z',
    updatedAt: '2024-01-15T00:00:00Z',
  },
  {
    id: 'athlete-2',
    coachId: '00000000-0000-0000-0000-000000000001',
    firstName: 'María',
    lastName: 'López',
    fullName: 'María López',
    email: 'maria@example.com',
    phone: '+598 99 234 567',
    dateOfBirth: '1998-07-22',
    country: 'Uruguay',
    status: 'Active',
    goals: 'Fuerza general',
    notes: 'Competidora de fuerza',
    startDate: '2024-06-10T00:00:00Z',
    height: 165,
    weight: 62,
    createdAt: '2024-06-10T00:00:00Z',
    updatedAt: '2024-06-10T00:00:00Z',
  },
  {
    id: 'athlete-3',
    coachId: '00000000-0000-0000-0000-000000000001',
    firstName: 'Juan',
    lastName: 'Rodríguez',
    fullName: 'Juan Rodríguez',
    email: 'juan@example.com',
    phone: '+598 99 345 678',
    dateOfBirth: '1992-11-08',
    country: 'Uruguay',
    status: 'Active',
    goals: 'Máxima fuerza',
    notes: 'Avanzado',
    startDate: '2023-09-20T00:00:00Z',
    height: 182,
    weight: 95,
    createdAt: '2023-09-20T00:00:00Z',
    updatedAt: '2023-09-20T00:00:00Z',
  },
];

// Mock max lifts for athletes
export const mockMaxLifts: Record<string, MaxLift[]> = {
  'athlete-1': [
    { id: 'ml-1-1', exerciseId: 'bench-press', exerciseName: 'Bench Press', weight: 100, isTested: true, recordedAt: '2026-03-01T00:00:00Z' },
    { id: 'ml-1-2', exerciseId: 'squat', exerciseName: 'Back Squat', weight: 140, isTested: true, recordedAt: '2026-03-01T00:00:00Z' },
    { id: 'ml-1-3', exerciseId: 'deadlift', exerciseName: 'Deadlift', weight: 180, isTested: true, recordedAt: '2026-02-25T00:00:00Z' },
    { id: 'ml-1-4', exerciseId: 'ohp', exerciseName: 'Overhead Press', weight: 65, isTested: false, recordedAt: '2026-02-20T00:00:00Z', estimationDetails: '3x60kg' },
  ],
  'athlete-2': [
    { id: 'ml-2-1', exerciseId: 'bench-press', exerciseName: 'Bench Press', weight: 60, isTested: true, recordedAt: '2026-02-28T00:00:00Z' },
    { id: 'ml-2-2', exerciseId: 'squat', exerciseName: 'Back Squat', weight: 95, isTested: true, recordedAt: '2026-02-28T00:00:00Z' },
    { id: 'ml-2-3', exerciseId: 'deadlift', exerciseName: 'Deadlift', weight: 115, isTested: true, recordedAt: '2026-02-28T00:00:00Z' },
  ],
  'athlete-3': [
    { id: 'ml-3-1', exerciseId: 'bench-press', exerciseName: 'Bench Press', weight: 140, isTested: true, recordedAt: '2026-03-10T00:00:00Z' },
    { id: 'ml-3-2', exerciseId: 'squat', exerciseName: 'Back Squat', weight: 200, isTested: true, recordedAt: '2026-03-10T00:00:00Z' },
    { id: 'ml-3-3', exerciseId: 'deadlift', exerciseName: 'Deadlift', weight: 230, isTested: true, recordedAt: '2026-03-10T00:00:00Z' },
    { id: 'ml-3-4', exerciseId: 'ohp', exerciseName: 'Overhead Press', weight: 90, isTested: true, recordedAt: '2026-03-10T00:00:00Z' },
    { id: 'ml-3-5', exerciseId: 'barbell-row', exerciseName: 'Barbell Row', weight: 120, isTested: false, recordedAt: '2026-03-05T00:00:00Z' },
  ],
};

// Helper to get mock max lifts for an athlete
export function getMockAthleteMaxLifts(athleteId: string): AthleteMaxLifts | null {
  const athlete = mockAthletes.find(a => a.id === athleteId);
  const maxLifts = mockMaxLifts[athleteId];
  
  if (!athlete || !maxLifts) return null;
  
  return {
    athleteId: athlete.id,
    athleteName: `${athlete.firstName} ${athlete.lastName}`,
    maxLifts,
  };
}
