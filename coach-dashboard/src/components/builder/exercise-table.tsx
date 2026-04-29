'use client';

import React, { useState, useCallback } from 'react';
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
import { Plus, GripVertical } from 'lucide-react';
import { Button } from '@/components/ui/button';
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogTrigger,
} from '@/components/ui/dialog';
import { ExerciseRow } from './exercise-row';
import { ExercisePickerWithHistory } from './exercise-picker-with-history';
import { BuilderExercise, ExerciseOption } from '@/types/builder';
import { useBuilder } from '@/providers/builder-provider';
import { cn } from '@/utils/cn';

interface ExerciseTableProps {
  exercises: BuilderExercise[];
  weekId: string;
  dayId: string;
  activeExerciseId?: string;
  exerciseOptions: ExerciseOption[];
  athleteId?: string | null;
  onAddExercise: (exerciseId: string, exerciseName: string) => void;
  onUpdateExercise: (exerciseId: string, updates: Partial<BuilderExercise>) => void;
  onDeleteExercise: (exerciseId: string) => void;
  onDuplicateExercise: (exerciseId: string) => void;
  onReorderExercises: (exercises: BuilderExercise[]) => void;
  onSetActiveExercise: (exerciseId: string | undefined) => void;
}

// Column headers
const columnHeaders = [
  { label: 'Exercise', width: 'flex-1 min-w-[200px]' },
  { label: 'Sets', width: 'w-16' },
  { label: 'Min', width: 'w-16' },
  { label: 'Max', width: 'w-16' },
  { label: 'RPE', width: 'w-16' },
  { label: 'Rest', width: 'w-20' },
  { label: 'Notes', width: 'w-32' },
];

// Sortable wrapper for ExerciseRow
interface SortableExerciseRowProps {
  exercise: BuilderExercise;
  weekId: string;
  dayId: string;
  isActive: boolean;
  onUpdate: (updates: Partial<BuilderExercise>) => void;
  onDelete: () => void;
  onDuplicate: () => void;
  onActivate: () => void;
  onMoveToNext: () => void;
  onMoveToPrev: () => void;
  onViewProgression: () => void;
}

function SortableExerciseRow(props: SortableExerciseRowProps) {
  const {
    attributes,
    listeners,
    setNodeRef,
    transform,
    transition,
    isDragging,
  } = useSortable({ id: props.exercise.id });

  const style = {
    transform: CSS.Transform.toString(transform),
    transition,
    opacity: isDragging ? 0.5 : 1,
  };

  return (
    <div ref={setNodeRef} style={style}>
      <ExerciseRow
        exercise={props.exercise}
        weekId={props.weekId}
        dayId={props.dayId}
        isActive={props.isActive}
        onUpdate={props.onUpdate}
        onDelete={props.onDelete}
        onDuplicate={props.onDuplicate}
        onActivate={props.onActivate}
        onMoveToNext={props.onMoveToNext}
        onMoveToPrev={props.onMoveToPrev}
        onViewProgression={props.onViewProgression}
        dragHandleProps={{ ...attributes, ...listeners }}
      />
    </div>
  );
}

export function ExerciseTable({
  exercises,
  weekId,
  dayId,
  activeExerciseId,
  exerciseOptions,
  athleteId,
  onAddExercise,
  onUpdateExercise,
  onDeleteExercise,
  onDuplicateExercise,
  onReorderExercises,
  onSetActiveExercise,
}: ExerciseTableProps) {
  const [isDialogOpen, setIsDialogOpen] = useState(false);
  const [activeExerciseDragId, setActiveExerciseDragId] = useState<string | null>(null);
  const { dispatch } = useBuilder();

  // DnD sensors
  const sensors = useSensors(
    useSensor(PointerSensor, {
      activationConstraint: { distance: 5 },
    }),
    useSensor(KeyboardSensor, {
      coordinateGetter: sortableKeyboardCoordinates,
    })
  );

  const handleDragStart = (event: DragStartEvent) => {
    setActiveExerciseDragId(event.active.id as string);
  };

  const handleDragEnd = (event: DragEndEvent) => {
    const { active, over } = event;
    setActiveExerciseDragId(null);

    if (!over || active.id === over.id) return;

    const oldIndex = exercises.findIndex((ex) => ex.id === active.id);
    const newIndex = exercises.findIndex((ex) => ex.id === over.id);

    if (oldIndex !== -1 && newIndex !== -1) {
      const newExercises = [...exercises];
      const [removed] = newExercises.splice(oldIndex, 1);
      newExercises.splice(newIndex, 0, removed);
      // Update order numbers
      const reorderedExercises = newExercises.map((ex, i) => ({ ...ex, order: i + 1 }));
      onReorderExercises(reorderedExercises);
    }
  };

  const activeExerciseForDrag = activeExerciseDragId
    ? exercises.find((ex) => ex.id === activeExerciseDragId)
    : null;

  const handleSelectExercise = useCallback(
    (exercise: ExerciseOption) => {
      onAddExercise(exercise.id, exercise.name);
      setIsDialogOpen(false);
    },
    [onAddExercise]
  );

  const handleAddEmptyExercise = useCallback(() => {
    onAddExercise('', 'New Exercise');
  }, [onAddExercise]);

  const handleMoveToNext = useCallback(
    (currentIndex: number) => {
      if (currentIndex < exercises.length - 1) {
        onSetActiveExercise(exercises[currentIndex + 1].id);
      }
    },
    [exercises, onSetActiveExercise]
  );

  const handleMoveToPrev = useCallback(
    (currentIndex: number) => {
      if (currentIndex > 0) {
        onSetActiveExercise(exercises[currentIndex - 1].id);
      }
    },
    [exercises, onSetActiveExercise]
  );

  return (
    <div className="space-y-1">
      {/* Column Headers */}
      {exercises.length > 0 && (
        <div className="flex items-center gap-2 px-2 py-1 text-xs font-medium text-muted-foreground">
          <div className="w-6" /> {/* Spacer for drag handle */}
          <div className="flex items-center gap-2 flex-1">
            {columnHeaders.map((header) => (
              <div key={header.label} className={cn(header.width, 'px-2')}>
                {header.label}
              </div>
            ))}
          </div>
          <div className="w-[60px]" /> {/* Spacer for actions */}
        </div>
      )}

      {/* Exercise Rows with DnD */}
      <DndContext
        sensors={sensors}
        collisionDetection={closestCenter}
        onDragStart={handleDragStart}
        onDragEnd={handleDragEnd}
      >
        <SortableContext
          items={exercises.map((ex) => ex.id)}
          strategy={verticalListSortingStrategy}
        >
          <div className="space-y-0.5">
            {exercises.map((exercise, index) => (
              <SortableExerciseRow
                key={exercise.id}
                exercise={exercise}
                weekId={weekId}
                dayId={dayId}
                isActive={exercise.id === activeExerciseId}
                onUpdate={(updates) => onUpdateExercise(exercise.id, updates)}
                onDelete={() => onDeleteExercise(exercise.id)}
                onDuplicate={() => onDuplicateExercise(exercise.id)}
                onActivate={() => onSetActiveExercise(exercise.id)}
                onMoveToNext={() => handleMoveToNext(index)}
                onMoveToPrev={() => handleMoveToPrev(index)}
                onViewProgression={() =>
                  dispatch({
                    type: 'OPEN_PROGRESSION_PANEL',
                    payload: { exerciseId: exercise.exerciseId, exerciseName: exercise.exerciseName },
                  })
                }
              />
            ))}
          </div>
        </SortableContext>

        {/* Drag Overlay */}
        <DragOverlay>
          {activeExerciseForDrag ? (
            <div className="opacity-80 shadow-lg rounded-md border bg-background p-2">
              <div className="flex items-center gap-2">
                <GripVertical className="h-4 w-4 text-muted-foreground" />
                <span className="text-sm font-medium">{activeExerciseForDrag.exerciseName}</span>
              </div>
            </div>
          ) : null}
        </DragOverlay>
      </DndContext>

      {/* Empty State & Add Button */}
      {exercises.length === 0 && (
        <div className="py-8 text-center text-sm text-muted-foreground">
          No exercises yet. Add your first exercise to get started.
        </div>
      )}

      {/* Add Exercise Actions */}
      <div className="flex items-center gap-2 pt-2">
        <Dialog open={isDialogOpen} onOpenChange={setIsDialogOpen}>
          <DialogTrigger asChild>
            <Button
              variant="ghost"
              size="sm"
              className="text-muted-foreground hover:text-foreground"
            >
              <Plus className="h-4 w-4 mr-1" />
              Add from library
            </Button>
          </DialogTrigger>
          <DialogContent className="max-w-3xl p-0">
            <DialogHeader className="px-4 pt-4 pb-2">
              <DialogTitle>Add Exercise</DialogTitle>
            </DialogHeader>
            <ExercisePickerWithHistory
              exerciseOptions={exerciseOptions}
              athleteId={athleteId}
              onSelect={handleSelectExercise}
            />
          </DialogContent>
        </Dialog>

        <Button
          variant="ghost"
          size="sm"
          className="text-muted-foreground hover:text-foreground"
          onClick={handleAddEmptyExercise}
        >
          <Plus className="h-4 w-4 mr-1" />
          Add blank row
        </Button>
      </div>
    </div>
  );
}
