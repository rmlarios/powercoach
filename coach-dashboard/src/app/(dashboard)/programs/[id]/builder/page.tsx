'use client';

import React, { useEffect, useMemo, useCallback, useState } from 'react';
import { useParams, useRouter } from 'next/navigation';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import {
  DndContext,
  DragEndEvent,
  DragOverlay,
  DragStartEvent,
  KeyboardSensor,
  PointerSensor,
  closestCenter,
  useSensor,
  useSensors,
} from '@dnd-kit/core';
import {
  SortableContext,
  sortableKeyboardCoordinates,
  useSortable,
  verticalListSortingStrategy,
} from '@dnd-kit/sortable';
import { CSS } from '@dnd-kit/utilities';
import {
  ArrowLeft,
  Save,
  MoreVertical,
  Plus,
  Loader2,
  Clock,
  CheckCircle2,
  GripVertical,
  User,
  Eye,
  Undo2,
  FileDown,
  FileSpreadsheet,
} from 'lucide-react';
import { Button } from '@/components/ui/button';
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuTrigger,
} from '@/components/ui/dropdown-menu';
import { Skeleton } from '@/components/ui/skeleton';
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogFooter,
} from '@/components/ui/dialog';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Textarea } from '@/components/ui/textarea';
import { WeekBlock, ExerciseProgressionView, ViewModeSelector, DayCentricView, CalendarView, ExerciseProgressionPanel } from '@/components/builder';
import { NotationHelpModal } from '@/components/builder/notation-help-modal';
import { ExportPDFDialog } from '@/components/pdf';
import { ExportExcelDialog } from '@/components/excel';
import { BuilderProvider, useBuilder } from '@/providers/builder-provider';
import { useCoach } from '@/providers/coach-provider';
import { useAuth } from '@/providers/auth-provider';
import { programsApi, repsToString, type SaveProgramFullData } from '@/lib/api/programs-api';
import { exercisesApi } from '@/lib/api/exercises-api';
import { useAthletes, useAthleteMaxLifts, useRegisterMaxLift } from '@/hooks/use-athletes';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select';
import { mockProgram, mockExerciseOptions, mockAthletes } from '@/lib/mock-data';
import { ExerciseOption, BuilderWeek, BuilderDay, BuilderExercise, BuilderViewMode } from '@/types/builder';
import { formatDate } from '@/utils/format-date';

// Mock data is used only as a fallback when API calls fail or return no data

// Sortable wrapper for WeekBlock
interface SortableWeekBlockProps {
  week: BuilderWeek;
  activeExerciseId?: string;
  exerciseOptions: ExerciseOption[];
  athleteId?: string | null;
  onUpdate: (updates: Partial<BuilderWeek>) => void;
  onDelete: () => void;
  onDuplicate: () => void;
  onToggleCollapse: () => void;
  onAddDay: () => void;
  onUpdateDay: (dayId: string, updates: Partial<BuilderDay>) => void;
  onDeleteDay: (dayId: string) => void;
  onDuplicateDay: (dayId: string) => void;
  onReorderDays: (days: BuilderDay[]) => void;
  onToggleDayCollapse: (dayId: string) => void;
  onAddExercise: (dayId: string, exerciseId: string, exerciseName: string) => void;
  onUpdateExercise: (dayId: string, exerciseId: string, updates: Partial<BuilderExercise>) => void;
  onDeleteExercise: (dayId: string, exerciseId: string) => void;
  onDuplicateExercise: (dayId: string, exerciseId: string) => void;
  onReorderExercises: (dayId: string, exercises: BuilderExercise[]) => void;
  onSetActiveExercise: (exerciseId: string | undefined) => void;
}

function SortableWeekBlock(props: SortableWeekBlockProps) {
  const {
    attributes,
    listeners,
    setNodeRef,
    transform,
    transition,
    isDragging,
  } = useSortable({ id: props.week.id });

  const style = {
    transform: CSS.Transform.toString(transform),
    transition,
    opacity: isDragging ? 0.5 : 1,
  };

  return (
    <div ref={setNodeRef} style={style} className="relative group">
      {/* Drag handle overlay */}
      <div
        {...attributes}
        {...listeners}
        className="absolute -left-10 top-4 hidden md:flex items-center justify-center w-8 h-8 rounded-md cursor-grab opacity-0 group-hover:opacity-100 transition-opacity hover:bg-accent"
      >
        <GripVertical className="h-4 w-4 text-muted-foreground" />
      </div>
      <WeekBlock
        week={props.week}
        activeExerciseId={props.activeExerciseId}
        exerciseOptions={props.exerciseOptions}
        athleteId={props.athleteId}
        onUpdate={props.onUpdate}
        onDelete={props.onDelete}
        onDuplicate={props.onDuplicate}
        onToggleCollapse={props.onToggleCollapse}
        onAddDay={props.onAddDay}
        onUpdateDay={props.onUpdateDay}
        onDeleteDay={props.onDeleteDay}
        onDuplicateDay={props.onDuplicateDay}
        onReorderDays={props.onReorderDays}
        onToggleDayCollapse={props.onToggleDayCollapse}
        onAddExercise={props.onAddExercise}
        onUpdateExercise={props.onUpdateExercise}
        onDeleteExercise={props.onDeleteExercise}
        onDuplicateExercise={props.onDuplicateExercise}
        onReorderExercises={props.onReorderExercises}
        onSetActiveExercise={props.onSetActiveExercise}
      />
    </div>
  );
}

function ProgramBuilderContent() {
  const params = useParams();
  const router = useRouter();
  const queryClient = useQueryClient();
  const { coach } = useCoach();
  const { user } = useAuth();
  const programId = params.id as string;
  const coachId = user?.coachId ?? coach?.id ?? '';

  // View mode state
  const [viewMode, setViewMode] = useState<BuilderViewMode>('weekly');

  // Preview athlete state (for showing weight suggestions)
  const [previewAthleteId, setPreviewAthleteId] = useState<string | null>(null);

  // Program start date for calendar view
  const [programStartDate, setProgramStartDate] = useState<Date>(() => {
    // Default to next Monday
    const today = new Date();
    const daysUntilMonday = (8 - today.getDay()) % 7 || 7;
    const nextMonday = new Date(today);
    nextMonday.setDate(today.getDate() + daysUntilMonday);
    nextMonday.setHours(0, 0, 0, 0);
    return nextMonday;
  });

  // Weight rounding configuration (kg increments)
  const [weightRoundTo, setWeightRoundTo] = useState<number>(2.5);

  // Export dialog states
  const [showExportPDF, setShowExportPDF] = useState(false);
  const [showExportExcel, setShowExportExcel] = useState(false);

  // Program settings dialog state
  const [showSettings, setShowSettings] = useState(false);
  const [settingsForm, setSettingsForm] = useState({ name: '', description: '', durationWeeks: 4 });

  // Drag state for weeks
  const [activeWeekId, setActiveWeekId] = useState<string | null>(null);

  const {
    state,
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
    dispatch,
  } = useBuilder();

  // DnD sensors for weeks
  const weekSensors = useSensors(
    useSensor(PointerSensor, {
      activationConstraint: { distance: 8 },
    }),
    useSensor(KeyboardSensor, {
      coordinateGetter: sortableKeyboardCoordinates,
    })
  );

  const handleWeekDragStart = (event: DragStartEvent) => {
    setActiveWeekId(event.active.id as string);
  };

  const handleWeekDragEnd = (event: DragEndEvent) => {
    const { active, over } = event;
    setActiveWeekId(null);

    if (!over || active.id === over.id) return;

    const oldIndex = state.weeks.findIndex((week) => week.id === active.id);
    const newIndex = state.weeks.findIndex((week) => week.id === over.id);

    if (oldIndex !== -1 && newIndex !== -1) {
      const newWeeks = [...state.weeks];
      const [removed] = newWeeks.splice(oldIndex, 1);
      newWeeks.splice(newIndex, 0, removed);
      // Update week numbers
      const reorderedWeeks = newWeeks.map((w, i) => ({ ...w, weekNumber: i + 1 }));
      reorderWeeks(reorderedWeeks);
    }
  };

  const activeWeek = activeWeekId ? state.weeks.find((w) => w.id === activeWeekId) : null;

  // Fetch program data (with fallback to mock data on error)
  const { data: program, isLoading: isProgramLoading, isError: isProgramError } = useQuery({
    queryKey: ['program', programId, coachId],
    queryFn: () => programsApi.getById(programId, coachId),
    enabled: !!programId && !!coachId,
    retry: false,
  });

  // Fetch exercises for autocomplete (with fallback to mock data on error)
  const { data: exercisesData, isError: isExercisesError } = useQuery({
    queryKey: ['exercises'],
    queryFn: () => exercisesApi.getAll(),
    retry: false,
  });

  // Fetch athletes for preview selector
  const { data: athletesData, isError: isAthletesError } = useAthletes({
    coachId,
    status: 'active',
    pageSize: 100, // Support up to 100 athletes in selector
  });

  // Fetch selected athlete's max lifts for weight preview
  const { data: athleteMaxLifts } = useAthleteMaxLifts(previewAthleteId || '');

  // Real API data with mock fallback on error
  const effectiveProgram = isProgramError ? mockProgram : program;

  // Effective athletes (with mock data fallback)
  const effectiveAthletes = useMemo(() => {
    if (isAthletesError || !athletesData?.items?.length) {
      return mockAthletes;
    }
    return athletesData.items;
  }, [athletesData, isAthletesError]);

  // Effective max lifts — use real API data, no mock override
  const effectiveMaxLifts = useMemo(() => {
    if (athleteMaxLifts) {
      return athleteMaxLifts.maxLifts;
    }
    return undefined;
  }, [athleteMaxLifts]);

  // Register max lift mutation
  const registerMaxLiftMutation = useRegisterMaxLift();

  // Handler to register a new 1RM
  const handleRegister1RM = useCallback((exerciseId: string, exerciseName: string, weight: number) => {
    if (!previewAthleteId) {
      console.warn('No preview athlete selected for 1RM registration');
      return;
    }
    
    registerMaxLiftMutation.mutate({
      athleteId: previewAthleteId,
      data: { exerciseId, weight, isTested: false }
    });
  }, [previewAthleteId, registerMaxLiftMutation]);

  // Handler for calendar drag-to-reschedule
  const handleCalendarDayMove = useCallback((
    sourceWeekId: string,
    sourceDayId: string,
    targetWeekNumber: number,
    targetDayOffset: number
  ) => {
    pushUndo();
    
    // Find source week and day  
    const sourceWeek = state.weeks.find(w => w.id === sourceWeekId);
    const sourceDay = sourceWeek?.days.find(d => d.id === sourceDayId);
    if (!sourceWeek || !sourceDay) return;
    
    // Find target week
    const targetWeek = state.weeks.find(w => w.weekNumber === targetWeekNumber);
    
    if (!targetWeek) {
      // Target is outside existing weeks — no-op
      console.warn('Cannot move day: target week does not exist');
      return;
    }
    
    if (sourceWeek.id === targetWeek.id) {
      // Same week: just reorder days based on new offset
      // Remove day from current position and recalculate order
      const otherDays = sourceWeek.days.filter(d => d.id !== sourceDayId);
      
      // Determine insertion index based on dayOffset
      // Lower offset = earlier in the week
      const sortedOthers = [...otherDays].sort((a, b) => a.dayNumber - b.dayNumber);
      
      // Find the right insertion point
      // Since getTrainingDayOffset maps dayIndex -> weekday offset, we need 
      // to figure out where in the sorted order this day should go
      let insertIdx = sortedOthers.length; // default: append
      for (let i = 0; i < sortedOthers.length; i++) {
        const existingOffset = getTrainingDayOffsetForWeek(sortedOthers.length + 1, i >= insertIdx ? i + 1 : i);
        if (targetDayOffset <= existingOffset) {
          insertIdx = i;
          break;
        }
      }
      
      sortedOthers.splice(insertIdx, 0, sourceDay);
      
      // Renumber days
      const renumbered = sortedOthers.map((d, i) => ({ ...d, dayNumber: i + 1 }));
      reorderDays(sourceWeek.id, renumbered);
    } else {
      // Cross-week move: remove from source, add to target
      // Remove from source week
      deleteDay(sourceWeek.id, sourceDayId);
      
      // Add to target week with a new dayNumber
      const newDayNumber = targetWeek.days.length + 1;
      const movedDay: BuilderDay = {
        ...sourceDay,
        dayNumber: newDayNumber,
      };
      
      // Use dispatch to add the day to the target week
      dispatch({ type: 'ADD_DAY', payload: { weekId: targetWeek.id, day: movedDay } });
    }
  }, [state.weeks, pushUndo, reorderDays, deleteDay, dispatch]);

  // Helper: get the day-of-week offset for a given position in a week
  function getTrainingDayOffsetForWeek(totalDays: number, index: number): number {
    const splits: Record<number, number[]> = {
      1: [0], 2: [0, 3], 3: [0, 2, 4], 4: [0, 1, 3, 4],
      5: [0, 1, 2, 3, 4], 6: [0, 1, 2, 3, 4, 5], 7: [0, 1, 2, 3, 4, 5, 6],
    };
    return splits[totalDays]?.[index] ?? index;
  }

  // Convert exercises to options format — uses real API data, falls back to mock on error
  const exerciseOptions: ExerciseOption[] = useMemo(() => {
    if (isExercisesError || !exercisesData) return mockExerciseOptions;
    return exercisesData.map((ex) => ({
      id: ex.id,
      name: ex.name,
      category: ex.category,
      muscleGroup: ex.primaryMuscleGroup,
    }));
  }, [exercisesData, isExercisesError]);

  // Initialize builder state when program loads
  useEffect(() => {
    if (effectiveProgram) {
      initializeFromProgram(effectiveProgram);

      // Apply preset from localStorage if set during creation
      const presetKey = `program-preset-${programId}`;
      const preset = localStorage.getItem(presetKey);
      if (preset && (preset === 'pl3' || preset === 'ul4' || preset === 'ppl6')) {
        dispatch({ type: 'APPLY_PRESET', payload: { preset } });
        localStorage.removeItem(presetKey);
      }
    }
  }, [effectiveProgram, initializeFromProgram, programId, dispatch]);

  // Save mutation
  const saveMutation = useMutation({
    mutationFn: async () => {
      dispatch({ type: 'SET_SAVING', payload: true });

      // Convert builder state → backend DTO
      const payload: SaveProgramFullData = {
        name: state.programName,
        description: state.description,
        durationWeeks: state.durationWeeks,
        weeks: state.weeks.map((week) => ({
          weekNumber: week.weekNumber,
          name: week.name,
          notes: week.notes,
          days: (week.days || []).map((day) => ({
            dayNumber: day.dayNumber,
            name: day.name,
            focus: day.focus || null,
            notes: day.notes,
            exercises: (day.exercises || []).map((ex) => ({
              exerciseId: ex.exerciseId,
              sets: ex.sets,
              reps: ex.rawNotation || repsToString(ex.repsMin, ex.repsMax),
              targetRpe: ex.rpeTarget,
              restSeconds: ex.restSeconds,
              notes: ex.notes,
              order: ex.order,
              exerciseType: ex.exerciseType || 'Standard',
              percentageRM: ex.percentageRM,
              rawNotation: ex.rawNotation,
              weight: ex.weight,
              emomConfigJson: ex.emomConfig ? JSON.stringify(ex.emomConfig) : undefined,
              tempoConfigJson: ex.tempoConfig ? JSON.stringify(ex.tempoConfig) : undefined,
              supersetConfigJson: ex.supersetConfig ? JSON.stringify(ex.supersetConfig) : undefined,
            })),
          })),
        })),
      };

      return programsApi.saveFull(programId, payload, coachId);
    },
    onSuccess: () => {
      dispatch({ type: 'MARK_SAVED' });
      queryClient.invalidateQueries({ queryKey: ['program', programId] });
    },
    onError: () => {
      dispatch({ type: 'SET_SAVING', payload: false });
    },
  });

  const handleSave = useCallback(() => {
    saveMutation.mutate();
  }, [saveMutation]);

  // Duplicate program handler
  const handleDuplicate = useCallback(async () => {
    try {
      const newId = await programsApi.create({
        coachId,
        name: `${state.programName} (Copy)`,
        description: state.description,
        durationWeeks: state.durationWeeks,
      });
      // Save the current structure to the new program
      const payload: SaveProgramFullData = {
        name: `${state.programName} (Copy)`,
        description: state.description,
        durationWeeks: state.durationWeeks,
        weeks: state.weeks.map((week) => ({
          weekNumber: week.weekNumber,
          name: week.name,
          notes: week.notes,
          days: (week.days || []).map((day) => ({
            dayNumber: day.dayNumber,
            name: day.name,
            focus: day.focus || null,
            notes: day.notes,
            exercises: (day.exercises || []).map((ex) => ({
              exerciseId: ex.exerciseId,
              sets: ex.sets,
              reps: ex.rawNotation || repsToString(ex.repsMin, ex.repsMax),
              targetRpe: ex.rpeTarget,
              restSeconds: ex.restSeconds,
              notes: ex.notes,
              order: ex.order,
              exerciseType: ex.exerciseType || 'Standard',
              percentageRM: ex.percentageRM,
              rawNotation: ex.rawNotation,
              weight: ex.weight,
              emomConfigJson: ex.emomConfig ? JSON.stringify(ex.emomConfig) : undefined,
              tempoConfigJson: ex.tempoConfig ? JSON.stringify(ex.tempoConfig) : undefined,
              supersetConfigJson: ex.supersetConfig ? JSON.stringify(ex.supersetConfig) : undefined,
            })),
          })),
        })),
      };
      await programsApi.saveFull(newId, payload, coachId);
      queryClient.invalidateQueries({ queryKey: ['programs'] });
      router.push(`/programs/${newId}/builder`);
    } catch {
      // error handled silently
    }
  }, [coachId, state, queryClient, router, programId]);

  // Program settings handler
  const handleOpenSettings = useCallback(() => {
    setSettingsForm({
      name: state.programName,
      description: state.description || '',
      durationWeeks: state.durationWeeks,
    });
    setShowSettings(true);
  }, [state.programName, state.description, state.durationWeeks]);

  const handleSaveSettings = useCallback(() => {
    dispatch({ type: 'SET_PROGRAM', payload: {
      programId: state.programId,
      programName: settingsForm.name,
      description: settingsForm.description,
      durationWeeks: settingsForm.durationWeeks,
      weeks: state.weeks,
    }});
    setShowSettings(false);
  }, [settingsForm, dispatch, state.programId, state.weeks]);

  // Keyboard shortcut for save
  useEffect(() => {
    const handleKeyDown = (e: KeyboardEvent) => {
      if ((e.ctrlKey || e.metaKey) && e.key === 's') {
        e.preventDefault();
        if (state.hasUnsavedChanges && !state.isSaving) {
          handleSave();
        }
      }
    };

    window.addEventListener('keydown', handleKeyDown);
    return () => window.removeEventListener('keydown', handleKeyDown);
  }, [handleSave, state.hasUnsavedChanges, state.isSaving]);

  // Keyboard shortcut for undo (Ctrl+Z)
  useEffect(() => {
    const handleKeyDown = (e: KeyboardEvent) => {
      if ((e.ctrlKey || e.metaKey) && e.key === 'z' && !e.shiftKey) {
        e.preventDefault();
        if (canUndo) {
          undo();
        }
      }
    };

    window.addEventListener('keydown', handleKeyDown);
    return () => window.removeEventListener('keydown', handleKeyDown);
  }, [undo, canUndo]);

  // Warn before leaving with unsaved changes
  useEffect(() => {
    const handleBeforeUnload = (e: BeforeUnloadEvent) => {
      if (state.hasUnsavedChanges) {
        e.preventDefault();
        e.returnValue = '';
      }
    };

    window.addEventListener('beforeunload', handleBeforeUnload);
    return () => window.removeEventListener('beforeunload', handleBeforeUnload);
  }, [state.hasUnsavedChanges]);

  // Show skeleton while loading from API
  if (isProgramLoading || state.isLoading) {
    return <ProgramBuilderSkeleton />;
  }

  return (
    <div className="min-h-screen bg-background">
      {/* Header */}
      <header className="sticky top-0 z-50 border-b bg-background/95 backdrop-blur supports-[backdrop-filter]:bg-background/60">
        <div className="container flex items-center justify-between h-16 px-4">
          <div className="flex items-center gap-4">
            <Button
              variant="ghost"
              size="icon"
              onClick={() => router.push('/programs')}
            >
              <ArrowLeft className="h-5 w-5" />
            </Button>
            <div>
              <h1 className="text-lg font-semibold">{state.programName}</h1>
              <p className="text-sm text-muted-foreground">
                {state.durationWeeks} weeks • {state.weeks.length} active weeks
              </p>
            </div>
          </div>

          <div className="flex items-center gap-3">
            {/* Save Status */}
            {state.lastSavedAt && !state.hasUnsavedChanges && (
              <div className="flex items-center gap-1.5 text-sm text-muted-foreground">
                <CheckCircle2 className="h-4 w-4 text-green-500" />
                <span>Saved {formatDate(state.lastSavedAt)}</span>
              </div>
            )}
            {state.hasUnsavedChanges && !state.isSaving && (
              <div className="flex items-center gap-1.5 text-sm text-muted-foreground">
                <Clock className="h-4 w-4" />
                <span>Unsaved changes</span>
              </div>
            )}

            {/* Undo Button */}
            <Button
              variant="outline"
              size="icon"
              onClick={undo}
              disabled={!canUndo}
              title="Undo (Ctrl+Z)"
            >
              <Undo2 className="h-4 w-4" />
            </Button>

            {/* Save Button */}
            <Button
              onClick={handleSave}
              disabled={!state.hasUnsavedChanges || state.isSaving}
              className="min-w-[100px]"
              title="Save (Ctrl+S)"
            >
              {state.isSaving ? (
                <>
                  <Loader2 className="h-4 w-4 mr-2 animate-spin" />
                  Saving...
                </>
              ) : (
                <>
                  <Save className="h-4 w-4 mr-2" />
                  Save
                </>
              )}
            </Button>

            {/* More Actions */}
            <DropdownMenu>
              <DropdownMenuTrigger asChild>
                <Button variant="outline" size="icon">
                  <MoreVertical className="h-4 w-4" />
                </Button>
              </DropdownMenuTrigger>
              <DropdownMenuContent align="end">
                <DropdownMenuItem onClick={() => setShowExportPDF(true)}>
                  <FileDown className="h-4 w-4 mr-2" />
                  Exportar a PDF
                </DropdownMenuItem>
                <DropdownMenuItem onClick={() => setShowExportExcel(true)}>
                  <FileSpreadsheet className="h-4 w-4 mr-2" />
                  Exportar a Excel
                </DropdownMenuItem>
                <DropdownMenuItem onClick={handleDuplicate}>Duplicate program</DropdownMenuItem>
                <DropdownMenuItem onClick={handleOpenSettings}>Program settings</DropdownMenuItem>
              </DropdownMenuContent>
            </DropdownMenu>
          </div>
        </div>
      </header>

      {/* Main Content */}
      <main className="container px-4 py-8">
        <div className="max-w-5xl mx-auto space-y-6">
          {/* View Mode Selector & Preview Controls */}
          <div className="flex items-center justify-between">
            <div className="flex items-center gap-2">
              <ViewModeSelector mode={viewMode} onChange={setViewMode} />
              <NotationHelpModal variant="icon" />
            </div>
            
            {/* Athlete Preview Selector - shows weight suggestions based on selected athlete's 1RM */}
            {viewMode === 'day-centric' && (
              <div className="flex items-center gap-4">
                {/* Weight Rounding Selector */}
                <div className="flex items-center gap-2">
                  <span className="text-sm text-muted-foreground">Redondeo:</span>
                  <Select
                    value={weightRoundTo.toString()}
                    onValueChange={(value) => setWeightRoundTo(parseFloat(value))}
                  >
                    <SelectTrigger className="w-[100px]">
                      <SelectValue />
                    </SelectTrigger>
                    <SelectContent>
                      <SelectItem value="1">1 kg</SelectItem>
                      <SelectItem value="2.5">2.5 kg</SelectItem>
                      <SelectItem value="5">5 kg</SelectItem>
                    </SelectContent>
                  </Select>
                </div>

                {/* Athlete Preview Selector */}
                <div className="flex items-center gap-2">
                  <Eye className="h-4 w-4 text-muted-foreground" />
                  <Select
                    value={previewAthleteId || 'none'}
                    onValueChange={(value) => setPreviewAthleteId(value === 'none' ? null : value)}
                  >
                    <SelectTrigger className="w-[220px]">
                      <SelectValue placeholder="Preview como atleta..." />
                    </SelectTrigger>
                    <SelectContent>
                      <SelectItem value="none">
                        <span className="text-muted-foreground">Sin preview de pesos</span>
                      </SelectItem>
                      {effectiveAthletes.map((athlete) => (
                        <SelectItem key={athlete.id} value={athlete.id}>
                          <div className="flex items-center gap-2">
                            <User className="h-3.5 w-3.5" />
                            {athlete.fullName}
                          </div>
                        </SelectItem>
                      ))}
                    </SelectContent>
                  </Select>
                </div>
              </div>
            )}

            {/* Calendar View Controls */}
            {viewMode === 'calendar' && (
              <div className="flex items-center gap-4">
                <div className="flex items-center gap-2">
                  <span className="text-sm text-muted-foreground">Fecha inicio:</span>
                  <input
                    type="date"
                    value={programStartDate.toISOString().split('T')[0]}
                    onChange={(e) => {
                      const date = new Date(e.target.value);
                      if (!isNaN(date.getTime())) {
                        setProgramStartDate(date);
                      }
                    }}
                    className="px-3 py-1.5 border rounded-md text-sm bg-background"
                  />
                </div>
              </div>
            )}
          </div>

          {/* Weekly Builder View */}
          {viewMode === 'weekly' && (
            <div className="pl-10 space-y-6">
              {/* Week Blocks with DnD */}
              <DndContext
                sensors={weekSensors}
                collisionDetection={closestCenter}
                onDragStart={handleWeekDragStart}
                onDragEnd={handleWeekDragEnd}
              >
                <SortableContext
                  items={state.weeks.map((w) => w.id)}
                  strategy={verticalListSortingStrategy}
                >
                  {state.weeks.map((week) => (
                    <SortableWeekBlock
                      key={week.id}
                      week={week}
                      activeExerciseId={state.activeExerciseId}
                      exerciseOptions={exerciseOptions}
                      athleteId={previewAthleteId}
                      onUpdate={(updates) => updateWeek(week.id, updates)}
                      onDelete={() => deleteWeek(week.id)}
                      onDuplicate={() => duplicateWeek(week.id)}
                      onToggleCollapse={() => toggleWeekCollapse(week.id)}
                      onAddDay={() => addDay(week.id)}
                      onUpdateDay={(dayId, updates) => updateDay(week.id, dayId, updates)}
                      onDeleteDay={(dayId) => deleteDay(week.id, dayId)}
                      onDuplicateDay={(dayId) => duplicateDay(week.id, dayId)}
                      onReorderDays={(days) => reorderDays(week.id, days)}
                      onToggleDayCollapse={(dayId) => toggleDayCollapse(week.id, dayId)}
                      onAddExercise={(dayId, exerciseId, exerciseName) =>
                        addExercise(week.id, dayId, exerciseId, exerciseName)
                      }
                      onUpdateExercise={(dayId, exerciseId, updates) =>
                        updateExercise(week.id, dayId, exerciseId, updates)
                      }
                      onDeleteExercise={(dayId, exerciseId) =>
                        deleteExercise(week.id, dayId, exerciseId)
                      }
                      onDuplicateExercise={(dayId, exerciseId) =>
                        duplicateExercise(week.id, dayId, exerciseId)
                      }
                      onReorderExercises={(dayId, exercises) =>
                        reorderExercises(week.id, dayId, exercises)
                      }
                      onSetActiveExercise={(exerciseId) =>
                        dispatch({
                          type: 'SET_ACTIVE_EXERCISE',
                          payload: { exerciseId },
                        })
                      }
                    />
                  ))}
                </SortableContext>

                {/* Drag Overlay for Weeks */}
                <DragOverlay>
                  {activeWeek ? (
                    <div className="opacity-80 shadow-xl rounded-xl border bg-background">
                      <div className="p-4 flex items-center gap-3">
                        <GripVertical className="h-5 w-5 text-muted-foreground" />
                        <div>
                          <h3 className="font-semibold">{activeWeek.name}</h3>
                          <p className="text-sm text-muted-foreground">
                            {activeWeek.days.length} days
                          </p>
                        </div>
                      </div>
                    </div>
                  ) : null}
                </DragOverlay>
              </DndContext>

              {/* Empty State */}
              {state.weeks.length === 0 && (
                <div className="py-20 text-center border-2 border-dashed rounded-xl">
                  <h3 className="text-lg font-medium mb-2">No weeks yet</h3>
                  <p className="text-muted-foreground mb-6">
                    Start building your program by adding the first week.
                  </p>
                  <Button onClick={addWeek}>
                    <Plus className="h-4 w-4 mr-2" />
                    Add Week
                  </Button>
                </div>
              )}

              {/* Add Week Button */}
              {state.weeks.length > 0 && (
                <Button
                  variant="outline"
                  className="w-full h-14 border-dashed"
                  onClick={addWeek}
                >
                  <Plus className="h-5 w-5 mr-2" />
                  Add Week
                </Button>
              )}
            </div>
          )}

          {/* Exercise Progression View */}
          {viewMode === 'progression' && (
            <ExerciseProgressionView
              weeks={state.weeks}
              totalWeeks={state.durationWeeks}
              onUpdateExercise={updateExercise}
            />
          )}

          {/* Day-Centric View */}
          {viewMode === 'day-centric' && (
            <DayCentricView
              weeks={state.weeks}
              totalWeeks={state.durationWeeks}
              exerciseOptions={exerciseOptions}
              athleteId={previewAthleteId}
              athleteMaxLifts={effectiveMaxLifts}
              weightRoundTo={weightRoundTo}
              onUpdateExercise={updateExercise}
              onAddExercise={(weekId, dayId, exerciseId, exerciseName, initialValues) =>
                addExercise(weekId, dayId, exerciseId, exerciseName, initialValues)
              }
              onDeleteExercise={deleteExercise}
              onAddDay={addDay}
              onPushUndo={pushUndo}
              onRegister1RM={previewAthleteId ? handleRegister1RM : undefined}
            />
          )}

          {/* Calendar View */}
          {viewMode === 'calendar' && (
            <CalendarView
              weeks={state.weeks}
              startDate={programStartDate}
              onDayClick={(event) => {
                // Optional: switch to day-centric view and focus on the day
                console.log('Day clicked:', event);
              }}
              onDayMove={handleCalendarDayMove}
            />
          )}
        </div>
      </main>

      {/* Export PDF Dialog */}
      <ExportPDFDialog
        open={showExportPDF}
        onOpenChange={setShowExportPDF}
        pdfData={{
          programName: state.programName,
          description: state.description,
          athleteName: previewAthleteId 
            ? (() => {
                const athlete = effectiveAthletes.find(a => a.id === previewAthleteId);
                return athlete ? athlete.fullName : undefined;
              })()
            : undefined,
          coachName: coach?.name,
          logoUrl: coach?.logoUrl,
          durationWeeks: state.durationWeeks,
          weeks: state.weeks,
          athleteMaxLifts: effectiveMaxLifts,
          includeWeights: !!previewAthleteId,
          weightRoundTo,
        }}
        onExported={() => setShowExportPDF(false)}
      />

      {/* Export Excel Dialog */}
      <ExportExcelDialog
        open={showExportExcel}
        onOpenChange={setShowExportExcel}
        excelData={{
          programName: state.programName,
          description: state.description,
          athleteName: previewAthleteId 
            ? (() => {
                const athlete = effectiveAthletes.find(a => a.id === previewAthleteId);
                return athlete ? athlete.fullName : undefined;
              })()
            : undefined,
          coachName: coach?.name,
          durationWeeks: state.durationWeeks,
          weeks: state.weeks,
          athleteMaxLifts: effectiveMaxLifts,
          includeWeights: !!previewAthleteId,
          weightRoundTo,
        }}
        onExported={() => setShowExportExcel(false)}
      />

      {/* Exercise Progression Panel (Sheet) */}
      <ExerciseProgressionPanel />

      {/* Program Settings Dialog */}
      <Dialog open={showSettings} onOpenChange={setShowSettings}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Program Settings</DialogTitle>
          </DialogHeader>
          <div className="space-y-4">
            <div className="space-y-2">
              <Label>Program Name</Label>
              <Input
                value={settingsForm.name}
                onChange={(e) => setSettingsForm({ ...settingsForm, name: e.target.value })}
              />
            </div>
            <div className="space-y-2">
              <Label>Description</Label>
              <Textarea
                value={settingsForm.description}
                onChange={(e) => setSettingsForm({ ...settingsForm, description: e.target.value })}
                rows={3}
              />
            </div>
            <div className="space-y-2">
              <Label>Duration (weeks)</Label>
              <Input
                type="number"
                min={1}
                max={52}
                value={settingsForm.durationWeeks}
                onChange={(e) => setSettingsForm({ ...settingsForm, durationWeeks: parseInt(e.target.value) || 1 })}
              />
            </div>
          </div>
          <DialogFooter>
            <Button variant="outline" onClick={() => setShowSettings(false)}>Cancel</Button>
            <Button onClick={handleSaveSettings} disabled={!settingsForm.name.trim()}>
              Save Settings
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </div>
  );
}

function ProgramBuilderSkeleton() {
  return (
    <div className="min-h-screen bg-background">
      {/* Header Skeleton */}
      <header className="sticky top-0 z-50 border-b bg-background">
        <div className="container flex items-center justify-between h-16 px-4">
          <div className="flex items-center gap-4">
            <Skeleton className="h-10 w-10 rounded-md" />
            <div>
              <Skeleton className="h-5 w-48 mb-1" />
              <Skeleton className="h-4 w-32" />
            </div>
          </div>
          <div className="flex items-center gap-3">
            <Skeleton className="h-10 w-24" />
            <Skeleton className="h-10 w-10" />
          </div>
        </div>
      </header>

      {/* Content Skeleton */}
      <main className="container px-4 py-8">
        <div className="max-w-5xl mx-auto space-y-6">
          {[1, 2].map((i) => (
            <Skeleton key={i} className="h-64 w-full rounded-xl" />
          ))}
        </div>
      </main>
    </div>
  );
}

export default function ProgramBuilderPage() {
  return (
    <BuilderProvider>
      <ProgramBuilderContent />
    </BuilderProvider>
  );
}
