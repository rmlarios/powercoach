'use client';

import React, { useState, useRef, useEffect } from 'react';
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
  ChevronDown,
  ChevronRight,
  Copy,
  MoreHorizontal,
  Plus,
  Trash2,
  GripVertical,
} from 'lucide-react';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
} from '@/components/ui/dropdown-menu';
import { DayBlock } from './day-block';
import { BuilderWeek, BuilderDay, BuilderExercise, ExerciseOption } from '@/types/builder';
import { cn } from '@/utils/cn';

import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogDescription, DialogFooter } from '@/components/ui/dialog';
import { CopyWeekDialog } from './copy-week-dialog';
import { DeloadWeekDialog } from './deload-week-dialog';

interface WeekBlockProps {
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
  onToggleDayCollapse: (dayId: string) => void;
  onReorderDays: (days: BuilderDay[]) => void;
  onAddExercise: (dayId: string, exerciseId: string, exerciseName: string) => void;
  onUpdateExercise: (dayId: string, exerciseId: string, updates: Partial<BuilderExercise>) => void;
  onDeleteExercise: (dayId: string, exerciseId: string) => void;
  onDuplicateExercise: (dayId: string, exerciseId: string) => void;
  onReorderExercises: (dayId: string, exercises: BuilderExercise[]) => void;
  onSetActiveExercise: (exerciseId: string | undefined) => void;
  dragHandleProps?: React.HTMLAttributes<HTMLDivElement>;
}

export function WeekBlock({
  week,
  activeExerciseId,
  exerciseOptions,
  athleteId,
  onUpdate,
  onDelete,
  onDuplicate,
  onToggleCollapse,
  onAddDay,
  onUpdateDay,
  onDeleteDay,
  onDuplicateDay,
  onToggleDayCollapse,
  onReorderDays,
  onAddExercise,
  onUpdateExercise,
  onDeleteExercise,
  onDuplicateExercise,
  onReorderExercises,
  onSetActiveExercise,
  dragHandleProps,
}: WeekBlockProps) {
  const [isEditingName, setIsEditingName] = useState(false);
  const [editedName, setEditedName] = useState(week.name || '');
  const [activeDayId, setActiveDayId] = useState<string | null>(null);
  const [isCopyWeekOpen, setIsCopyWeekOpen] = useState(false);
  const [isDeloadWeekOpen, setIsDeloadWeekOpen] = useState(false);
  const nameInputRef = useRef<HTMLInputElement>(null);

  const dayCount = week.days.length;
  const totalExercises = week.days.reduce((sum, day) => sum + day.exercises.length, 0);

  // DnD sensors
  const sensors = useSensors(
    useSensor(PointerSensor, {
      activationConstraint: {
        distance: 8,
      },
    }),
    useSensor(KeyboardSensor, {
      coordinateGetter: sortableKeyboardCoordinates,
    })
  );

  const handleDragStart = (event: DragStartEvent) => {
    setActiveDayId(event.active.id as string);
  };

  const handleDragEnd = (event: DragEndEvent) => {
    const { active, over } = event;
    setActiveDayId(null);

    if (!over || active.id === over.id) return;

    const oldIndex = week.days.findIndex((day) => day.id === active.id);
    const newIndex = week.days.findIndex((day) => day.id === over.id);

    if (oldIndex !== -1 && newIndex !== -1) {
      const newDays = [...week.days];
      const [removed] = newDays.splice(oldIndex, 1);
      newDays.splice(newIndex, 0, removed);
      // Update day numbers
      const reorderedDays = newDays.map((d, i) => ({ ...d, dayNumber: i + 1 }));
      onReorderDays(reorderedDays);
    }
  };

  const activeDay = activeDayId ? week.days.find((d) => d.id === activeDayId) : null;

  // Focus input when editing starts
  useEffect(() => {
    if (isEditingName && nameInputRef.current) {
      nameInputRef.current.focus();
      nameInputRef.current.select();
    }
  }, [isEditingName]);

  const handleNameSubmit = () => {
    setIsEditingName(false);
    if (editedName !== week.name) {
      onUpdate({ name: editedName });
    }
  };

  const handleNameKeyDown = (e: React.KeyboardEvent) => {
    if (e.key === 'Enter') {
      handleNameSubmit();
    } else if (e.key === 'Escape') {
      setEditedName(week.name || '');
      setIsEditingName(false);
    }
  };

  return (
    <div
      className={cn(
        'rounded-xl border-2 bg-background transition-all',
        week.isNew && 'animate-in fade-in slide-in-from-top-4 duration-300',
        week.isCollapsed ? 'border-border/50' : 'border-border'
      )}
    >
      {/* Week Header */}
      <div className="flex items-center gap-3 p-4 border-b">
        {/* Drag Handle */}
        <div
          {...dragHandleProps}
          className="cursor-grab active:cursor-grabbing p-1.5 rounded-md hover:bg-muted opacity-50 hover:opacity-100 transition-opacity"
        >
          <GripVertical className="h-5 w-5 text-muted-foreground" />
        </div>

        {/* Collapse Toggle */}
        <Button
          variant="ghost"
          size="icon"
          className="h-8 w-8"
          onClick={onToggleCollapse}
        >
          {week.isCollapsed ? (
            <ChevronRight className="h-5 w-5" />
          ) : (
            <ChevronDown className="h-5 w-5" />
          )}
        </Button>

        {/* Week Number Badge */}
        <div className="flex items-center justify-center h-8 w-8 rounded-lg bg-primary text-primary-foreground text-sm font-semibold">
          {week.weekNumber}
        </div>

        {/* Week Name */}
        <div className="flex items-center gap-2">
          {isEditingName ? (
            <Input
              ref={nameInputRef}
              value={editedName}
              onChange={(e) => setEditedName(e.target.value)}
              onBlur={handleNameSubmit}
              onKeyDown={handleNameKeyDown}
              className="h-8 w-64 text-base font-semibold"
            />
          ) : (
            <button
              onClick={() => setIsEditingName(true)}
              className="text-base font-semibold hover:underline underline-offset-2"
            >
              {week.name || `Week ${week.weekNumber}`}
            </button>
          )}

          {/* Phase Shortcuts */}
          <DropdownMenu>
            <DropdownMenuTrigger asChild>
              <Button
                variant="outline"
                size="sm"
                className={cn(
                  "h-6 text-xs px-2 py-0 ml-1 transition-colors border-dashed",
                  ['Acumulación', 'Intensificación', 'Pico', 'Deload'].includes(week.name || '')
                    ? "bg-primary/10 text-primary border-primary/20"
                    : "text-muted-foreground hover:text-foreground"
                )}
                title="Establecer fase de la semana"
              >
                {['Acumulación', 'Intensificación', 'Pico', 'Deload'].includes(week.name || '') ? week.name : 'Fase'}
                <ChevronDown className="h-3 w-3 ml-1" />
              </Button>
            </DropdownMenuTrigger>
            <DropdownMenuContent align="start">
              {['Acumulación', 'Intensificación', 'Pico', 'Deload'].map(phase => (
                <DropdownMenuItem
                  key={phase}
                  onClick={() => {
                    setEditedName(phase); // Update local state if editing
                    onUpdate({ name: phase }); // Apply immediately
                  }}
                >
                  {phase}
                </DropdownMenuItem>
              ))}
              <DropdownMenuSeparator />
              <DropdownMenuItem
                onClick={() => {
                  setEditedName('');
                  onUpdate({ name: `Semana ${week.weekNumber}` });
                }}
              >
                Limpiar nombre
              </DropdownMenuItem>
            </DropdownMenuContent>
          </DropdownMenu>
        </div>

        {/* Stats */}
        <div className="flex items-center gap-4 text-sm text-muted-foreground">
          <span>
            {dayCount} {dayCount === 1 ? 'day' : 'days'}
          </span>
          <span>•</span>
          <span>
            {totalExercises} {totalExercises === 1 ? 'exercise' : 'exercises'}
          </span>
        </div>

        {/* Spacer */}
        <div className="flex-1" />

        {/* Add Day Button (visible when expanded) */}
        {!week.isCollapsed && (
          <Button
            variant="outline"
            size="sm"
            onClick={onAddDay}
            className="h-8"
          >
            <Plus className="h-4 w-4 mr-1" />
            Add Day
          </Button>
        )}

        {/* Week Actions Menu */}
        <DropdownMenu>
          <DropdownMenuTrigger asChild>
            <Button variant="ghost" size="icon" className="h-8 w-8">
              <MoreHorizontal className="h-5 w-5" />
            </Button>
          </DropdownMenuTrigger>
          <DropdownMenuContent align="end">
            <DropdownMenuItem onClick={onAddDay}>
              <Plus className="h-4 w-4 mr-2" />
              Add day
            </DropdownMenuItem>
            <DropdownMenuItem onClick={onDuplicate}>
              <Copy className="h-4 w-4 mr-2" />
              Duplicate week
            </DropdownMenuItem>
            <DropdownMenuItem onClick={() => setIsCopyWeekOpen(true)}>
              <Copy className="h-4 w-4 mr-2" />
              Copy week structure...
            </DropdownMenuItem>
            <DropdownMenuSeparator />
            <DropdownMenuItem onClick={() => setIsDeloadWeekOpen(true)}>
              <ChevronDown className="h-4 w-4 mr-2" />
              Generate Deload Week...
            </DropdownMenuItem>
            <DropdownMenuSeparator />
            <DropdownMenuItem
              onClick={onDelete}
              className="text-destructive focus:text-destructive"
            >
              <Trash2 className="h-4 w-4 mr-2" />
              Delete week
            </DropdownMenuItem>
          </DropdownMenuContent>
        </DropdownMenu>
      </div>

      {/* Week Content (Days) */}
      {!week.isCollapsed && (
        <div className="p-4 space-y-4">
          {week.days.length === 0 ? (
            <div className="py-12 text-center">
              <p className="text-muted-foreground mb-4">
                No days in this week yet. Add your first training day.
              </p>
              <Button variant="outline" onClick={onAddDay}>
                <Plus className="h-4 w-4 mr-2" />
                Add Day
              </Button>
            </div>
          ) : (
            <DndContext
              sensors={sensors}
              collisionDetection={closestCenter}
              onDragStart={handleDragStart}
              onDragEnd={handleDragEnd}
            >
              <SortableContext
                items={week.days.map((d) => d.id)}
                strategy={verticalListSortingStrategy}
              >
                <div className="space-y-4">
                  {week.days.map((day) => (
                    <SortableDayBlock
                      key={day.id}
                      day={day}
                      weekId={week.id}
                      activeExerciseId={activeExerciseId}
                      exerciseOptions={exerciseOptions}
                      athleteId={athleteId}
                      onUpdate={(updates) => onUpdateDay(day.id, updates)}
                      onDelete={() => onDeleteDay(day.id)}
                      onDuplicate={() => onDuplicateDay(day.id)}
                      onToggleCollapse={() => onToggleDayCollapse(day.id)}
                      onAddExercise={(exerciseId, exerciseName) =>
                        onAddExercise(day.id, exerciseId, exerciseName)
                      }
                      onUpdateExercise={(exerciseId, updates) =>
                        onUpdateExercise(day.id, exerciseId, updates)
                      }
                      onDeleteExercise={(exerciseId) => onDeleteExercise(day.id, exerciseId)}
                      onDuplicateExercise={(exerciseId) =>
                        onDuplicateExercise(day.id, exerciseId)
                      }
                      onReorderExercises={(exercises) =>
                        onReorderExercises(day.id, exercises)
                      }
                      onSetActiveExercise={onSetActiveExercise}
                    />
                  ))}
                </div>
              </SortableContext>
              <DragOverlay>
                {activeDay && (
                  <div className="opacity-90 shadow-lg">
                    <DayBlockPreview day={activeDay} />
                  </div>
                )}
              </DragOverlay>
            </DndContext>
          )}
        </div>
      )}

      {/* Collapsed Summary */}
      {week.isCollapsed && week.days.length > 0 && (
        <div className="px-4 py-3 text-sm text-muted-foreground bg-muted/30 rounded-b-xl">
          <div className="flex flex-wrap gap-2">
            {week.days.map((day) => (
              <span
                key={day.id}
                className="inline-flex items-center px-2 py-0.5 rounded-md bg-background border text-xs"
              >
                Day {day.dayNumber}: {day.exercises.length} exercises
              </span>
            ))}
          </div>
        </div>
      )}

      {isCopyWeekOpen && (
        <CopyWeekDialog 
          sourceWeekId={week.id} 
          isOpen={isCopyWeekOpen} 
          onOpenChange={setIsCopyWeekOpen} 
        />
      )}
      
      {isDeloadWeekOpen && (
        <DeloadWeekDialog 
          sourceWeekId={week.id} 
          isOpen={isDeloadWeekOpen} 
          onOpenChange={setIsDeloadWeekOpen} 
        />
      )}
    </div>
  );
}

// Sortable Day Block wrapper
interface SortableDayBlockProps {
  day: BuilderDay;
  weekId: string;
  activeExerciseId?: string;
  exerciseOptions: ExerciseOption[];
  athleteId?: string | null;
  onUpdate: (updates: Partial<BuilderDay>) => void;
  onDelete: () => void;
  onDuplicate: () => void;
  onToggleCollapse: () => void;
  onAddExercise: (exerciseId: string, exerciseName: string) => void;
  onUpdateExercise: (exerciseId: string, updates: Partial<BuilderExercise>) => void;
  onDeleteExercise: (exerciseId: string) => void;
  onDuplicateExercise: (exerciseId: string) => void;
  onReorderExercises: (exercises: BuilderExercise[]) => void;
  onSetActiveExercise: (exerciseId: string | undefined) => void;
}

function SortableDayBlock(props: SortableDayBlockProps) {
  const {
    attributes,
    listeners,
    setNodeRef,
    transform,
    transition,
    isDragging,
  } = useSortable({ id: props.day.id });

  const style = {
    transform: CSS.Transform.toString(transform),
    transition,
  };

  return (
    <div
      ref={setNodeRef}
      style={style}
      className={cn(isDragging && 'opacity-50')}
    >
      <DayBlock
        {...props}
        dragHandleProps={{ ...attributes, ...listeners }}
      />
    </div>
  );
}

// Preview component for drag overlay
function DayBlockPreview({ day }: { day: BuilderDay }) {
  return (
    <div className="rounded-lg border bg-card p-3 shadow-lg">
      <div className="flex items-center gap-2">
        <div className="flex items-center justify-center h-6 w-6 rounded bg-primary/10 text-primary text-xs font-medium">
          {day.dayNumber}
        </div>
        <span className="font-medium text-sm">{day.name || `Day ${day.dayNumber}`}</span>
        <span className="text-xs text-muted-foreground">
          {day.exercises.length} exercises
        </span>
      </div>
    </div>
  );
}
