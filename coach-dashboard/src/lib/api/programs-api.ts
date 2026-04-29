import apiClient from './client';
import { API_ENDPOINTS } from './endpoints';
import { 
  PagedResult, 
  ProgramTemplateListItem, 
  ProgramTemplate, 
  CreateProgramTemplateRequest,
  AthleteProgram,
  AthleteWorkout,
  LogWorkoutRequest,
} from '@/types';

export interface GetProgramsParams {
  coachId: string;
  isActive?: boolean;
  searchTerm?: string;
  pageNumber?: number;
  pageSize?: number;
}

export interface AddWeekData {
  weekNumber: number;
  name: string;
  notes?: string;
}

export interface AddDayData {
  dayNumber: number;
  name: string;
  focus: string;
  notes?: string;
}

export interface AddExerciseData {
  exerciseId: string;
  order: number;
  sets: number;
  reps: string;
  restSeconds?: number;
  targetRpe?: number;
  notes?: string;
  exerciseType?: string;
  percentageRM?: number;
  rawNotation?: string;
  weight?: number;
  emomConfigJson?: string;
  tempoConfigJson?: string;
  supersetConfigJson?: string;
}

export interface UpdateProgramData {
  name: string;
  description?: string;
  durationWeeks: number;
}

export interface SaveProgramFullData {
  name: string;
  description?: string;
  durationWeeks: number;
  weeks: SaveWeekData[];
}

export interface SaveWeekData {
  weekNumber: number;
  name?: string;
  notes?: string;
  days: SaveDayData[];
}

export interface SaveDayData {
  dayNumber: number;
  name?: string;
  focus: string | null;
  notes?: string;
  exercises: SaveExerciseData[];
}

export interface SaveExerciseData {
  exerciseId: string;
  sets: number;
  reps: string;
  targetRpe?: number;
  restSeconds?: number;
  notes?: string;
  order: number;
  exerciseType?: string;
  percentageRM?: number;
  rawNotation?: string;
  weight?: number;
  emomConfigJson?: string;
  tempoConfigJson?: string;
  supersetConfigJson?: string;
}

// ── Reps conversion helpers ──

/** Convert repsMin/repsMax to a Reps string for the backend */
export function repsToString(min: number, max: number): string {
  if (!min && !max) return '0';
  if (min === max || !max) return String(min);
  return `${min}-${max}`;
}

/** Parse a reps string from the backend into min/max for the builder */
export function parseReps(reps: string): { repsMin: number; repsMax: number } {
  if (!reps) return { repsMin: 0, repsMax: 0 };
  const trimmed = reps.trim();
  // Check for range: "8-12"
  if (trimmed.includes('-')) {
    const [minStr, maxStr] = trimmed.split('-');
    const min = parseInt(minStr, 10) || 0;
    const max = parseInt(maxStr, 10) || min;
    return { repsMin: min, repsMax: max };
  }
  // Single number: "10"
  const num = parseInt(trimmed, 10);
  if (!isNaN(num)) {
    return { repsMin: num, repsMax: num };
  }
  // Non-numeric (e.g., "AMRAP") — return 0s, frontend handles display via rawNotation
  return { repsMin: 0, repsMax: 0 };
}

// ── Program Templates API ──

export const programsApi = {
  getAll: async (params: GetProgramsParams): Promise<PagedResult<ProgramTemplateListItem>> => {
    const response = await apiClient.get(API_ENDPOINTS.programs.list, { params });
    return response.data;
  },

  getById: async (id: string, coachId: string): Promise<ProgramTemplate> => {
    const response = await apiClient.get(API_ENDPOINTS.programs.getById(id), { 
      params: { coachId } 
    });
    return response.data;
  },

  create: async (data: CreateProgramTemplateRequest): Promise<string> => {
    const response = await apiClient.post(API_ENDPOINTS.programs.create, data);
    return response.data;
  },

  update: async (id: string, data: UpdateProgramData, coachId: string): Promise<void> => {
    await apiClient.put(API_ENDPOINTS.programs.update(id), data, { params: { coachId } });
  },

  saveFull: async (id: string, data: SaveProgramFullData, coachId: string): Promise<string> => {
    const response = await apiClient.put(API_ENDPOINTS.programs.saveFull(id), data, { params: { coachId } });
    return response.data;
  },

  addWeek: async (programId: string, data: AddWeekData, coachId?: string): Promise<string> => {
    const response = await apiClient.post(API_ENDPOINTS.programs.addWeek(programId), data, 
      coachId ? { params: { coachId } } : undefined);
    return response.data;
  },

  deleteWeek: async (programId: string, weekId: string, coachId: string): Promise<void> => {
    await apiClient.delete(API_ENDPOINTS.programs.deleteWeek(programId, weekId), { params: { coachId } });
  },

  addDay: async (programId: string, weekId: string, data: AddDayData, coachId?: string): Promise<string> => {
    const response = await apiClient.post(API_ENDPOINTS.programs.addDay(programId, weekId), data,
      coachId ? { params: { coachId } } : undefined);
    return response.data;
  },

  deleteDay: async (programId: string, dayId: string, coachId: string): Promise<void> => {
    await apiClient.delete(API_ENDPOINTS.programs.deleteDay(programId, dayId), { params: { coachId } });
  },

  addExercise: async (
    programId: string, 
    weekId: string, 
    dayId: string, 
    data: AddExerciseData,
    coachId?: string
  ): Promise<string> => {
    const response = await apiClient.post(
      API_ENDPOINTS.programs.addExercise(programId, weekId, dayId), 
      data,
      coachId ? { params: { coachId } } : undefined
    );
    return response.data;
  },

  deleteExercise: async (programId: string, exerciseTemplateId: string, coachId: string): Promise<void> => {
    await apiClient.delete(API_ENDPOINTS.programs.deleteExercise(programId, exerciseTemplateId), { params: { coachId } });
  },

  clone: async (sourceId: string, coachId: string): Promise<string> => {
    // 1. Fetch full structure
    const source = await apiClient.get(API_ENDPOINTS.programs.getById(sourceId), { params: { coachId } });
    const original = source.data;

    // 2. Create new program
    const createResponse = await apiClient.post(API_ENDPOINTS.programs.create, {
      coachId,
      name: `Copia de ${original.name}`,
      description: original.description,
      durationWeeks: original.durationWeeks,
    });
    const newId: string = createResponse.data;

    // 3. Save full structure to the new program
    await apiClient.put(API_ENDPOINTS.programs.saveFull(newId), {
      name: `Copia de ${original.name}`,
      description: original.description,
      durationWeeks: original.durationWeeks,
      weeks: original.weeks?.map((w: Record<string, unknown>) => ({
        weekNumber: w.weekNumber,
        name: w.name,
        notes: w.notes,
        days: (w.days as Record<string, unknown>[])?.map((d: Record<string, unknown>) => ({
          dayNumber: d.dayNumber,
          name: d.name,
          focus: d.focus ?? null,
          notes: d.notes,
          exercises: (d.exercises as Record<string, unknown>[])?.map((e: Record<string, unknown>) => ({
            exerciseId: e.exerciseId,
            order: e.order,
            sets: e.sets,
            reps: e.reps,
            restSeconds: e.restSeconds,
            targetRpe: e.targetRpe,
            notes: e.notes,
            exerciseType: e.exerciseType,
            percentageRM: e.percentageRM,
            rawNotation: e.rawNotation,
            weight: e.weight,
            emomConfigJson: e.emomConfigJson,
            tempoConfigJson: e.tempoConfigJson,
            supersetConfigJson: e.supersetConfigJson,
          })) ?? [],
        })) ?? [],
      })) ?? [],
    }, { params: { coachId } });

    return newId;
  },
};

// ── Athlete Programs API ──

export const athleteProgramsApi = {
  get: async (athleteId: string): Promise<AthleteProgram | null> => {
    try {
      const response = await apiClient.get(API_ENDPOINTS.athletePrograms.get(athleteId));
      return response.data;
    } catch (error: unknown) {
      if ((error as { response?: { status?: number } })?.response?.status === 404) {
        return null;
      }
      throw error;
    }
  },

  assign: async (athleteId: string, data: {
    programTemplateId: string;
    startDate: string;
  }, coachId: string): Promise<string> => {
    const response = await apiClient.post(
      API_ENDPOINTS.athletePrograms.assign(athleteId), 
      data,
      { params: { coachId } }
    );
    return response.data;
  },

  bulkAssign: async (programId: string, data: {
    athleteIds: string[];
    startDate: string;
    notes?: string;
  }, coachId: string): Promise<string[]> => {
    const response = await apiClient.post(
      API_ENDPOINTS.athletePrograms.bulkAssign(programId),
      data,
      { params: { coachId } }
    );
    return response.data;
  },

  getWorkout: async (athleteId: string, workoutId: string): Promise<AthleteWorkout> => {
    const response = await apiClient.get(
      API_ENDPOINTS.athletePrograms.getWorkout(athleteId, workoutId)
    );
    return response.data;
  },

  logWorkout: async (athleteId: string, workoutId: string, data: LogWorkoutRequest): Promise<void> => {
    await apiClient.post(
      API_ENDPOINTS.athletePrograms.logWorkout(athleteId, workoutId), 
      data
    );
  },
};

// Named exports for convenience - Programs
export const getPrograms = programsApi.getAll;
export const getProgramById = programsApi.getById;
export const createProgram = programsApi.create;
export const updateProgram = programsApi.update;
export const saveProgramFull = programsApi.saveFull;
export const addProgramWeek = programsApi.addWeek;
export const addProgramDay = programsApi.addDay;
export const addProgramExercise = programsApi.addExercise;
export const deleteProgramWeek = programsApi.deleteWeek;
export const deleteProgramDay = programsApi.deleteDay;
export const deleteProgramExercise = programsApi.deleteExercise;

// Named exports for convenience - Athlete Programs
export const getAthleteProgram = athleteProgramsApi.get;
export const assignProgramToAthlete = athleteProgramsApi.assign;
export const getAthleteWorkout = athleteProgramsApi.getWorkout;
export const logAthleteWorkout = athleteProgramsApi.logWorkout;
