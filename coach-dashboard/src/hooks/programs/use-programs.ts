import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { programsApi, athleteProgramsApi, type SaveProgramFullData, type UpdateProgramData, type AddExerciseData } from '@/lib/api/programs-api';
import { CreateProgramTemplateRequest, LogWorkoutRequest } from '@/types';

// Query keys
export const programKeys = {
  all: ['programs'] as const,
  lists: () => [...programKeys.all, 'list'] as const,
  list: (params: Record<string, unknown>) => [...programKeys.lists(), params] as const,
  details: () => [...programKeys.all, 'detail'] as const,
  detail: (id: string) => [...programKeys.details(), id] as const,
};

export const athleteProgramKeys = {
  all: ['athlete-programs'] as const,
  detail: (athleteId: string) => [...athleteProgramKeys.all, athleteId] as const,
  workout: (athleteId: string, workoutId: string) => 
    [...athleteProgramKeys.all, athleteId, 'workout', workoutId] as const,
};

// Program Template Hooks
export function usePrograms(params: {
  coachId: string;
  isActive?: boolean;
  searchTerm?: string;
  pageNumber?: number;
  pageSize?: number;
}) {
  return useQuery({
    queryKey: programKeys.list(params),
    queryFn: () => programsApi.getAll(params),
    enabled: !!params.coachId,
  });
}

export function useProgram(id: string, coachId: string) {
  return useQuery({
    queryKey: programKeys.detail(id),
    queryFn: () => programsApi.getById(id, coachId),
    enabled: !!id && !!coachId,
  });
}

export function useCreateProgram() {
  const queryClient = useQueryClient();
  
  return useMutation({
    mutationFn: (data: CreateProgramTemplateRequest) => programsApi.create(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: programKeys.lists() });
    },
  });
}

export function useAddProgramWeek() {
  const queryClient = useQueryClient();
  
  return useMutation({
    mutationFn: ({ programId, data }: { 
      programId: string; 
      data: { weekNumber: number; name: string; notes?: string } 
    }) => programsApi.addWeek(programId, data),
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({ queryKey: programKeys.detail(variables.programId) });
    },
  });
}

export function useAddProgramDay() {
  const queryClient = useQueryClient();
  
  return useMutation({
    mutationFn: ({ programId, weekId, data }: { 
      programId: string; 
      weekId: string;
      data: { dayNumber: number; name: string; focus: string; notes?: string } 
    }) => programsApi.addDay(programId, weekId, data),
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({ queryKey: programKeys.detail(variables.programId) });
    },
  });
}

export function useAddProgramExercise() {
  const queryClient = useQueryClient();
  
  return useMutation({
    mutationFn: ({ programId, weekId, dayId, data }: { 
      programId: string; 
      weekId: string;
      dayId: string;
      data: AddExerciseData;
    }) => programsApi.addExercise(programId, weekId, dayId, data),
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({ queryKey: programKeys.detail(variables.programId) });
    },
  });
}

export function useUpdateProgram() {
  const queryClient = useQueryClient();
  
  return useMutation({
    mutationFn: ({ id, data, coachId }: { id: string; data: UpdateProgramData; coachId: string }) =>
      programsApi.update(id, data, coachId),
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({ queryKey: programKeys.detail(variables.id) });
      queryClient.invalidateQueries({ queryKey: programKeys.lists() });
    },
  });
}

export function useSaveProgram() {
  const queryClient = useQueryClient();
  
  return useMutation({
    mutationFn: ({ id, data, coachId }: { id: string; data: SaveProgramFullData; coachId: string }) =>
      programsApi.saveFull(id, data, coachId),
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({ queryKey: programKeys.detail(variables.id) });
      queryClient.invalidateQueries({ queryKey: programKeys.lists() });
    },
  });
}

export function useDeleteProgramWeek() {
  const queryClient = useQueryClient();
  
  return useMutation({
    mutationFn: ({ programId, weekId, coachId }: { programId: string; weekId: string; coachId: string }) =>
      programsApi.deleteWeek(programId, weekId, coachId),
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({ queryKey: programKeys.detail(variables.programId) });
    },
  });
}

export function useDeleteProgramDay() {
  const queryClient = useQueryClient();
  
  return useMutation({
    mutationFn: ({ programId, dayId, coachId }: { programId: string; dayId: string; coachId: string }) =>
      programsApi.deleteDay(programId, dayId, coachId),
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({ queryKey: programKeys.detail(variables.programId) });
    },
  });
}

export function useDeleteProgramExercise() {
  const queryClient = useQueryClient();
  
  return useMutation({
    mutationFn: ({ programId, exerciseTemplateId, coachId }: { programId: string; exerciseTemplateId: string; coachId: string }) =>
      programsApi.deleteExercise(programId, exerciseTemplateId, coachId),
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({ queryKey: programKeys.detail(variables.programId) });
    },
  });
}

// Athlete Program Hooks
export function useAthleteProgram(athleteId: string) {
  return useQuery({
    queryKey: athleteProgramKeys.detail(athleteId),
    queryFn: () => athleteProgramsApi.get(athleteId),
    enabled: !!athleteId,
  });
}

export function useAssignProgram() {
  const queryClient = useQueryClient();
  
  return useMutation({
    mutationFn: ({ athleteId, data, coachId }: { 
      athleteId: string; 
      data: { programTemplateId: string; startDate: string };
      coachId: string;
    }) => athleteProgramsApi.assign(athleteId, data, coachId),
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({ queryKey: athleteProgramKeys.detail(variables.athleteId) });
    },
  });
}

export function useAthleteWorkout(athleteId: string, workoutId: string) {
  return useQuery({
    queryKey: athleteProgramKeys.workout(athleteId, workoutId),
    queryFn: () => athleteProgramsApi.getWorkout(athleteId, workoutId),
    enabled: !!athleteId && !!workoutId,
  });
}

export function useLogWorkout() {
  const queryClient = useQueryClient();
  
  return useMutation({
    mutationFn: ({ athleteId, workoutId, data }: { 
      athleteId: string; 
      workoutId: string;
      data: LogWorkoutRequest 
    }) => athleteProgramsApi.logWorkout(athleteId, workoutId, data),
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({ 
        queryKey: athleteProgramKeys.workout(variables.athleteId, variables.workoutId) 
      });
      queryClient.invalidateQueries({ 
        queryKey: athleteProgramKeys.detail(variables.athleteId) 
      });
    },
  });
}

export function useCloneProgram() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ id, coachId }: { id: string; coachId: string }) =>
      programsApi.clone(id, coachId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: programKeys.lists() });
    },
  });
}

export function useBulkAssignProgram() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({
      programId,
      athleteIds,
      startDate,
      notes,
      coachId,
    }: {
      programId: string;
      athleteIds: string[];
      startDate: string;
      notes?: string;
      coachId: string;
    }) => athleteProgramsApi.bulkAssign(programId, { athleteIds, startDate, notes }, coachId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: athleteProgramKeys.all });
    },
  });
}
