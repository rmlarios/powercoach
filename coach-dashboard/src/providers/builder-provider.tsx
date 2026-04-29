'use client';

import React, { createContext, useContext, useReducer, useCallback } from 'react';
import { 
  BuilderState, 
  BuilderAction, 
  BuilderWeek, 
  BuilderDay, 
  BuilderExercise,
  ExerciseType,
} from '@/types/builder';
import { DayFocus, ProgramTemplate } from '@/types/program';
import { parseReps } from '@/lib/api/programs-api';
import { v4 as uuidv4 } from 'uuid';

// Initial state
const initialState: BuilderState = {
  programId: '',
  programName: '',
  durationWeeks: 0,
  weeks: [],
  isLoading: true,
  isSaving: false,
  hasUnsavedChanges: false,
};

// Reducer
export function builderReducer(state: BuilderState, action: BuilderAction): BuilderState {
  switch (action.type) {
    case 'SET_PROGRAM':
      return {
        ...state,
        ...action.payload,
        isLoading: false,
        hasUnsavedChanges: false,
      };

    case 'SET_LOADING':
      return { ...state, isLoading: action.payload };

    case 'SET_SAVING':
      return { ...state, isSaving: action.payload };

    case 'ADD_WEEK':
      return {
        ...state,
        weeks: [...state.weeks, action.payload.week],
        hasUnsavedChanges: true,
      };

    case 'UPDATE_WEEK':
      return {
        ...state,
        weeks: state.weeks.map((week) =>
          week.id === action.payload.weekId
            ? { ...week, ...action.payload.updates, isDirty: true }
            : week
        ),
        hasUnsavedChanges: true,
      };

    case 'DELETE_WEEK':
      return {
        ...state,
        weeks: state.weeks.filter((week) => week.id !== action.payload.weekId),
        hasUnsavedChanges: true,
      };

    case 'DUPLICATE_WEEK':
      const weekIndex = state.weeks.findIndex((w) => w.id === action.payload.weekId);
      const newWeeks = [...state.weeks];
      newWeeks.splice(weekIndex + 1, 0, action.payload.newWeek);
      return {
        ...state,
        weeks: newWeeks.map((w, i) => ({ ...w, weekNumber: i + 1 })),
        hasUnsavedChanges: true,
      };

    case 'REORDER_WEEKS':
      return {
        ...state,
        weeks: action.payload.weeks.map((w, i) => ({ ...w, weekNumber: i + 1 })),
        hasUnsavedChanges: true,
      };

    case 'ADD_DAY':
      return {
        ...state,
        weeks: state.weeks.map((week) =>
          week.id === action.payload.weekId
            ? { ...week, days: [...week.days, action.payload.day], isDirty: true }
            : week
        ),
        hasUnsavedChanges: true,
      };

    case 'UPDATE_DAY':
      return {
        ...state,
        weeks: state.weeks.map((week) =>
          week.id === action.payload.weekId
            ? {
                ...week,
                days: week.days.map((day) =>
                  day.id === action.payload.dayId
                    ? { ...day, ...action.payload.updates, isDirty: true }
                    : day
                ),
              }
            : week
        ),
        hasUnsavedChanges: true,
      };

    case 'DELETE_DAY':
      return {
        ...state,
        weeks: state.weeks.map((week) =>
          week.id === action.payload.weekId
            ? { ...week, days: week.days.filter((day) => day.id !== action.payload.dayId) }
            : week
        ),
        hasUnsavedChanges: true,
      };

    case 'DUPLICATE_DAY': {
      return {
        ...state,
        weeks: state.weeks.map((week) => {
          if (week.id !== action.payload.weekId) return week;
          const dayIndex = week.days.findIndex((d) => d.id === action.payload.dayId);
          const newDays = [...week.days];
          newDays.splice(dayIndex + 1, 0, action.payload.newDay);
          return {
            ...week,
            days: newDays.map((d, i) => ({ ...d, dayNumber: i + 1 })),
          };
        }),
        hasUnsavedChanges: true,
      };
    }

    case 'REORDER_DAYS':
      return {
        ...state,
        weeks: state.weeks.map((week) =>
          week.id === action.payload.weekId
            ? {
                ...week,
                days: action.payload.days.map((d, i) => ({ ...d, dayNumber: i + 1 })),
              }
            : week
        ),
        hasUnsavedChanges: true,
      };

    case 'ADD_EXERCISE':
      return {
        ...state,
        weeks: state.weeks.map((week) =>
          week.id === action.payload.weekId
            ? {
                ...week,
                days: week.days.map((day) =>
                  day.id === action.payload.dayId
                    ? { ...day, exercises: [...day.exercises, action.payload.exercise] }
                    : day
                ),
              }
            : week
        ),
        hasUnsavedChanges: true,
      };

    case 'UPDATE_EXERCISE':
      return {
        ...state,
        weeks: state.weeks.map((week) =>
          week.id === action.payload.weekId
            ? {
                ...week,
                days: week.days.map((day) =>
                  day.id === action.payload.dayId
                    ? {
                        ...day,
                        exercises: day.exercises.map((ex) =>
                          ex.id === action.payload.exerciseId
                            ? { ...ex, ...action.payload.updates, isDirty: true }
                            : ex
                        ),
                      }
                    : day
                ),
              }
            : week
        ),
        hasUnsavedChanges: true,
      };

    case 'DELETE_EXERCISE':
      return {
        ...state,
        weeks: state.weeks.map((week) =>
          week.id === action.payload.weekId
            ? {
                ...week,
                days: week.days.map((day) =>
                  day.id === action.payload.dayId
                    ? {
                        ...day,
                        exercises: day.exercises.filter(
                          (ex) => ex.id !== action.payload.exerciseId
                        ),
                      }
                    : day
                ),
              }
            : week
        ),
        hasUnsavedChanges: true,
      };

    case 'DUPLICATE_EXERCISE': {
      return {
        ...state,
        weeks: state.weeks.map((week) => {
          if (week.id !== action.payload.weekId) return week;
          return {
            ...week,
            days: week.days.map((day) => {
              if (day.id !== action.payload.dayId) return day;
              const exIndex = day.exercises.findIndex(
                (e) => e.id === action.payload.exerciseId
              );
              const newExercises = [...day.exercises];
              newExercises.splice(exIndex + 1, 0, action.payload.newExercise);
              return {
                ...day,
                exercises: newExercises.map((e, i) => ({ ...e, order: i + 1 })),
              };
            }),
          };
        }),
        hasUnsavedChanges: true,
      };
    }

    case 'REORDER_EXERCISES':
      return {
        ...state,
        weeks: state.weeks.map((week) =>
          week.id === action.payload.weekId
            ? {
                ...week,
                days: week.days.map((day) =>
                  day.id === action.payload.dayId
                    ? {
                        ...day,
                        exercises: action.payload.exercises.map((e, i) => ({
                          ...e,
                          order: i + 1,
                        })),
                      }
                    : day
                ),
              }
            : week
        ),
        hasUnsavedChanges: true,
      };

    case 'TOGGLE_WEEK_COLLAPSE':
      return {
        ...state,
        weeks: state.weeks.map((week) =>
          week.id === action.payload.weekId
            ? { ...week, isCollapsed: !week.isCollapsed }
            : week
        ),
      };

    case 'TOGGLE_DAY_COLLAPSE':
      return {
        ...state,
        weeks: state.weeks.map((week) =>
          week.id === action.payload.weekId
            ? {
                ...week,
                days: week.days.map((day) =>
                  day.id === action.payload.dayId
                    ? { ...day, isCollapsed: !day.isCollapsed }
                    : day
                ),
              }
            : week
        ),
      };

    case 'SET_ACTIVE_WEEK':
      return { ...state, activeWeekId: action.payload.weekId };

    case 'SET_ACTIVE_DAY':
      return { ...state, activeDayId: action.payload.dayId };

    case 'SET_ACTIVE_EXERCISE':
      return { ...state, activeExerciseId: action.payload.exerciseId };

    case 'MARK_SAVED':
      return {
        ...state,
        hasUnsavedChanges: false,
        isSaving: false,
        lastSavedAt: new Date().toISOString(),
        weeks: state.weeks.map((week) => ({
          ...week,
          isNew: false,
          isDirty: false,
          days: week.days.map((day) => ({
            ...day,
            isNew: false,
            isDirty: false,
            exercises: day.exercises.map((ex) => ({
              ...ex,
              isNew: false,
              isDirty: false,
            })),
          })),
        })),
      };

    case 'PUSH_UNDO': {
      // Save current state to history (max 10 snapshots)
      const history = state.history || [];
      const newHistory = [...history, JSON.parse(JSON.stringify(state.weeks))];
      if (newHistory.length > 10) {
        newHistory.shift(); // Remove oldest
      }
      return { ...state, history: newHistory };
    }

    case 'UNDO': {
      const history = state.history || [];
      if (history.length === 0) return state;
      
      const newHistory = [...history];
      const previousWeeks = newHistory.pop();
      
      return {
        ...state,
        weeks: previousWeeks || state.weeks,
        history: newHistory,
        hasUnsavedChanges: true,
      };
    }

    case 'APPLY_PRESET': {
      const presetMaps: Record<string, { name: string; focus: DayFocus }[]> = {
        pl3: [
          { name: 'Squat', focus: 'Squat' },
          { name: 'Bench', focus: 'Bench' },
          { name: 'Deadlift', focus: 'Deadlift' },
        ],
        ul4: [
          { name: 'Upper A', focus: 'UpperBody' },
          { name: 'Lower A', focus: 'LowerBody' },
          { name: 'Upper B', focus: 'UpperBody' },
          { name: 'Lower B', focus: 'LowerBody' },
        ],
        ppl6: [
          { name: 'Push', focus: 'Push' },
          { name: 'Pull', focus: 'Pull' },
          { name: 'Legs', focus: 'Legs' },
          { name: 'Push', focus: 'Push' },
          { name: 'Pull', focus: 'Pull' },
          { name: 'Legs', focus: 'Legs' },
        ],
      };

      const preset = presetMaps[action.payload.preset];
      if (!preset) return state;

      // Apply preset names & focus to week 1 days only
      return {
        ...state,
        weeks: state.weeks.map((week) => {
          if (week.weekNumber !== 1) return week;
          return {
            ...week,
            days: week.days.map((day, idx) => {
              const config = preset[idx];
              if (!config) return day;
              return { ...day, name: config.name, focus: config.focus, isDirty: true };
            }),
          };
        }),
        hasUnsavedChanges: true,
      };
    }

    case 'COPY_WEEK_STRUCTURE': {
      const { sourceWeekId, targetWeekIds, includeExercises, progressionPercent } = action.payload;
      const sourceWeek = state.weeks.find((w) => w.id === sourceWeekId);
      if (!sourceWeek) return state;

      const multiplier = progressionPercent ? 1 + progressionPercent / 100 : 1;

      return {
        ...state,
        weeks: state.weeks.map((week) => {
          if (!targetWeekIds.includes(week.id)) return week;

          // Replace days entirely with source week structure
          return {
            ...week,
            isDirty: true,
            days: sourceWeek.days.map((sourceDay) => ({
              ...sourceDay,
              id: uuidv4(), // New ID for the copied day
              isNew: true,
              isDirty: true,
              exercises: includeExercises
                ? sourceDay.exercises.map((ex) => {
                    let newPercentage = ex.percentageRM;
                    if (newPercentage && progressionPercent) {
                      newPercentage = Math.round(newPercentage * multiplier * 2) / 2; // Round to nearest 0.5%
                    }
                    return {
                      ...ex,
                      id: uuidv4(), // New ID for copied exercise
                      isNew: true,
                      isDirty: true,
                      percentageRM: newPercentage,
                    };
                  })
                : [], // Empty exercises if not included
            })),
          };
        }),
        hasUnsavedChanges: true,
      };
    }

    case 'GENERATE_DELOAD_WEEK': {
      const { afterWeekId, volumePercent } = action.payload;
      const sourceWeekIndex = state.weeks.findIndex((w) => w.id === afterWeekId);
      if (sourceWeekIndex === -1) return state;

      const sourceWeek = state.weeks[sourceWeekIndex];
      const multiplier = volumePercent / 100;

      const newWeek: BuilderWeek = {
        ...sourceWeek,
        id: uuidv4(),
        weekNumber: 0, // Set temporarily, reordered below
        name: 'Deload',
        notes: undefined,
        isNew: true,
        isDirty: true,
        days: sourceWeek.days.map((sourceDay) => ({
          ...sourceDay,
          id: uuidv4(),
          isNew: true,
          isDirty: true,
          exercises: sourceDay.exercises.map((ex) => {
            let newPercentage = ex.percentageRM;
            if (newPercentage) {
              newPercentage = Math.round(newPercentage * multiplier * 2) / 2;
            }
            return {
              ...ex,
              id: uuidv4(),
              isNew: true,
              isDirty: true,
              percentageRM: newPercentage,
            };
          }),
        })),
      };

      const newWeeks = [...state.weeks];
      newWeeks.splice(sourceWeekIndex + 1, 0, newWeek);

      return {
        ...state,
        weeks: newWeeks.map((w, i) => ({ ...w, weekNumber: i + 1 })),
        hasUnsavedChanges: true,
      };
    }

    case 'OPEN_PROGRESSION_PANEL':
      return { ...state, progressionPanel: action.payload };

    case 'CLOSE_PROGRESSION_PANEL':
      return { ...state, progressionPanel: undefined };

    default:
      return state;
  }
}

// Context
interface BuilderContextValue {
  state: BuilderState;
  dispatch: React.Dispatch<BuilderAction>;
  // Helper functions
  initializeFromProgram: (program: ProgramTemplate) => void;
  addWeek: () => void;
  updateWeek: (weekId: string, updates: Partial<BuilderWeek>) => void;
  deleteWeek: (weekId: string) => void;
  duplicateWeek: (weekId: string) => void;
  reorderWeeks: (weeks: BuilderWeek[]) => void;
  addDay: (weekId: string) => void;
  updateDay: (weekId: string, dayId: string, updates: Partial<BuilderDay>) => void;
  deleteDay: (weekId: string, dayId: string) => void;
  duplicateDay: (weekId: string, dayId: string) => void;
  reorderDays: (weekId: string, days: BuilderDay[]) => void;
  addExercise: (weekId: string, dayId: string, exerciseId: string, exerciseName: string, initialValues?: Partial<BuilderExercise>) => void;
  updateExercise: (weekId: string, dayId: string, exerciseId: string, updates: Partial<BuilderExercise>) => void;
  deleteExercise: (weekId: string, dayId: string, exerciseId: string) => void;
  duplicateExercise: (weekId: string, dayId: string, exerciseId: string) => void;
  reorderExercises: (weekId: string, dayId: string, exercises: BuilderExercise[]) => void;
  toggleWeekCollapse: (weekId: string) => void;
  toggleDayCollapse: (weekId: string, dayId: string) => void;
  // Undo functionality
  pushUndo: () => void;
  undo: () => void;
  canUndo: boolean;
}

const BuilderContext = createContext<BuilderContextValue | undefined>(undefined);

// Provider
export function BuilderProvider({ children }: { children: React.ReactNode }) {
  const [state, dispatch] = useReducer(builderReducer, initialState);

  const initializeFromProgram = useCallback((program: ProgramTemplate) => {
    const weeks: BuilderWeek[] = program.weeks.map((week) => ({
      id: week.id,
      weekNumber: week.weekNumber,
      name: week.name,
      notes: week.notes,
      days: week.days.map((day) => ({
        id: day.id,
        dayNumber: day.dayNumber,
        name: day.name,
        focus: day.focus,
        notes: day.notes,
        exercises: day.exercises.map((ex) => {
          const { repsMin, repsMax } = parseReps(ex.reps);
          return {
            id: ex.id,
            exerciseId: ex.exerciseId,
            exerciseName: ex.exerciseName,
            order: ex.order,
            sets: ex.sets,
            repsMin,
            repsMax,
            restSeconds: ex.restSeconds ?? 0,
            rpeTarget: ex.targetRpe,
            notes: ex.notes,
            exerciseType: (ex.exerciseType?.toLowerCase() || 'standard') as ExerciseType,
            percentageRM: ex.percentageRM,
            rawNotation: ex.rawNotation,
            weight: ex.weight,
            emomConfig: ex.emomConfigJson ? JSON.parse(ex.emomConfigJson) : undefined,
            tempoConfig: ex.tempoConfigJson ? JSON.parse(ex.tempoConfigJson) : undefined,
            supersetConfig: ex.supersetConfigJson ? JSON.parse(ex.supersetConfigJson) : undefined,
          };
        }),
      })),
    }));

    dispatch({
      type: 'SET_PROGRAM',
      payload: {
        programId: program.id,
        programName: program.name,
        description: program.description,
        durationWeeks: program.durationWeeks,
        weeks,
      },
    });
  }, []);

  const addWeek = useCallback(() => {
    const newWeekNumber = state.weeks.length + 1;
    const newWeek: BuilderWeek = {
      id: uuidv4(),
      tempId: uuidv4(),
      weekNumber: newWeekNumber,
      name: `Week ${newWeekNumber}`,
      days: [],
      isNew: true,
    };
    dispatch({ type: 'ADD_WEEK', payload: { week: newWeek } });
  }, [state.weeks.length]);

  const updateWeek = useCallback((weekId: string, updates: Partial<BuilderWeek>) => {
    dispatch({ type: 'UPDATE_WEEK', payload: { weekId, updates } });
  }, []);

  const deleteWeek = useCallback((weekId: string) => {
    dispatch({ type: 'DELETE_WEEK', payload: { weekId } });
  }, []);

  const duplicateWeek = useCallback((weekId: string) => {
    const week = state.weeks.find((w) => w.id === weekId);
    if (!week) return;

    const newWeek: BuilderWeek = {
      ...week,
      id: uuidv4(),
      tempId: uuidv4(),
      weekNumber: week.weekNumber + 1,
      name: `${week.name} (Copy)`,
      isNew: true,
      days: week.days.map((day) => ({
        ...day,
        id: uuidv4(),
        tempId: uuidv4(),
        isNew: true,
        exercises: day.exercises.map((ex) => ({
          ...ex,
          id: uuidv4(),
          tempId: uuidv4(),
          isNew: true,
        })),
      })),
    };

    dispatch({ type: 'DUPLICATE_WEEK', payload: { weekId, newWeek } });
  }, [state.weeks]);

  const addDay = useCallback((weekId: string) => {
    const week = state.weeks.find((w) => w.id === weekId);
    const newDayNumber = (week?.days.length || 0) + 1;
    
    const newDay: BuilderDay = {
      id: uuidv4(),
      tempId: uuidv4(),
      dayNumber: newDayNumber,
      name: `Day ${newDayNumber}`,
      focus: 'Custom',
      exercises: [],
      isNew: true,
    };
    dispatch({ type: 'ADD_DAY', payload: { weekId, day: newDay } });
  }, [state.weeks]);

  const updateDay = useCallback(
    (weekId: string, dayId: string, updates: Partial<BuilderDay>) => {
      dispatch({ type: 'UPDATE_DAY', payload: { weekId, dayId, updates } });
    },
    []
  );

  const deleteDay = useCallback((weekId: string, dayId: string) => {
    dispatch({ type: 'DELETE_DAY', payload: { weekId, dayId } });
  }, []);

  const duplicateDay = useCallback((weekId: string, dayId: string) => {
    const week = state.weeks.find((w) => w.id === weekId);
    const day = week?.days.find((d) => d.id === dayId);
    if (!day) return;

    const newDay: BuilderDay = {
      ...day,
      id: uuidv4(),
      tempId: uuidv4(),
      dayNumber: day.dayNumber + 1,
      name: `${day.name} (Copy)`,
      isNew: true,
      exercises: day.exercises.map((ex) => ({
        ...ex,
        id: uuidv4(),
        tempId: uuidv4(),
        isNew: true,
      })),
    };

    dispatch({ type: 'DUPLICATE_DAY', payload: { weekId, dayId, newDay } });
  }, [state.weeks]);

  const reorderDays = useCallback((weekId: string, days: BuilderDay[]) => {
    dispatch({ type: 'REORDER_DAYS', payload: { weekId, days } });
  }, []);

  const reorderWeeks = useCallback((weeks: BuilderWeek[]) => {
    dispatch({ type: 'REORDER_WEEKS', payload: { weeks } });
  }, []);

  const addExercise = useCallback(
    (weekId: string, dayId: string, exerciseId: string, exerciseName: string, initialValues?: Partial<BuilderExercise>) => {
      const week = state.weeks.find((w) => w.id === weekId);
      const day = week?.days.find((d) => d.id === dayId);
      const order = (day?.exercises.length || 0) + 1;

      const newExercise: BuilderExercise = {
        id: uuidv4(),
        tempId: uuidv4(),
        exerciseId,
        exerciseName,
        order,
        sets: initialValues?.sets ?? 3,
        repsMin: initialValues?.repsMin ?? 8,
        repsMax: initialValues?.repsMax ?? 12,
        restSeconds: initialValues?.restSeconds ?? 90,
        rpeTarget: initialValues?.rpeTarget,
        percentageRM: initialValues?.percentageRM,
        rawNotation: initialValues?.rawNotation,
        exerciseType: initialValues?.exerciseType,
        emomConfig: initialValues?.emomConfig,
        tempoConfig: initialValues?.tempoConfig,
        supersetConfig: initialValues?.supersetConfig,
        isNew: true,
      };

      dispatch({ type: 'ADD_EXERCISE', payload: { weekId, dayId, exercise: newExercise } });
    },
    [state.weeks]
  );

  const updateExercise = useCallback(
    (weekId: string, dayId: string, exerciseId: string, updates: Partial<BuilderExercise>) => {
      dispatch({ type: 'UPDATE_EXERCISE', payload: { weekId, dayId, exerciseId, updates } });
    },
    []
  );

  const deleteExercise = useCallback((weekId: string, dayId: string, exerciseId: string) => {
    dispatch({ type: 'DELETE_EXERCISE', payload: { weekId, dayId, exerciseId } });
  }, []);

  const duplicateExercise = useCallback(
    (weekId: string, dayId: string, exerciseId: string) => {
      const week = state.weeks.find((w) => w.id === weekId);
      const day = week?.days.find((d) => d.id === dayId);
      const exercise = day?.exercises.find((e) => e.id === exerciseId);
      if (!exercise) return;

      const newExercise: BuilderExercise = {
        ...exercise,
        id: uuidv4(),
        tempId: uuidv4(),
        order: exercise.order + 1,
        isNew: true,
      };

      dispatch({
        type: 'DUPLICATE_EXERCISE',
        payload: { weekId, dayId, exerciseId, newExercise },
      });
    },
    [state.weeks]
  );

  const reorderExercises = useCallback(
    (weekId: string, dayId: string, exercises: BuilderExercise[]) => {
      dispatch({ type: 'REORDER_EXERCISES', payload: { weekId, dayId, exercises } });
    },
    []
  );

  const toggleWeekCollapse = useCallback((weekId: string) => {
    dispatch({ type: 'TOGGLE_WEEK_COLLAPSE', payload: { weekId } });
  }, []);

  const toggleDayCollapse = useCallback((weekId: string, dayId: string) => {
    dispatch({ type: 'TOGGLE_DAY_COLLAPSE', payload: { weekId, dayId } });
  }, []);

  // Undo functionality
  const pushUndo = useCallback(() => {
    dispatch({ type: 'PUSH_UNDO' });
  }, []);

  const undo = useCallback(() => {
    dispatch({ type: 'UNDO' });
  }, []);

  const canUndo = (state.history?.length || 0) > 0;

  const value: BuilderContextValue = {
    state,
    dispatch,
    initializeFromProgram,
    addWeek,
    updateWeek,
    deleteWeek,
    duplicateWeek,
    reorderWeeks,
    addDay,
    updateDay,
    deleteDay,
    duplicateDay,
    reorderDays,
    addExercise,
    updateExercise,
    deleteExercise,
    duplicateExercise,
    reorderExercises,
    toggleWeekCollapse,
    toggleDayCollapse,
    pushUndo,
    undo,
    canUndo,
  };

  return <BuilderContext.Provider value={value}>{children}</BuilderContext.Provider>;
}

// Hook
export function useBuilder() {
  const context = useContext(BuilderContext);
  if (context === undefined) {
    throw new Error('useBuilder must be used within a BuilderProvider');
  }
  return context;
}
