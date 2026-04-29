'use client';

import React, { useState, useRef, useEffect } from 'react';
import {
  ChevronDown,
  ChevronRight,
  Copy,
  MoreHorizontal,
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
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select';
import { ExerciseTable } from './exercise-table';
import { BuilderDay, BuilderExercise, ExerciseOption, DAY_FOCUS_OPTIONS } from '@/types/builder';
import { DayFocus } from '@/types/program';
import { cn } from '@/utils/cn';

interface DayBlockProps {
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
  dragHandleProps?: React.HTMLAttributes<HTMLDivElement>;
}

export function DayBlock({
  day,
  weekId,
  activeExerciseId,
  exerciseOptions,
  athleteId,
  onUpdate,
  onDelete,
  onDuplicate,
  onToggleCollapse,
  onAddExercise,
  onUpdateExercise,
  onDeleteExercise,
  onDuplicateExercise,
  onReorderExercises,
  onSetActiveExercise,
  dragHandleProps,
}: DayBlockProps) {
  const [isEditingName, setIsEditingName] = useState(false);
  const [editedName, setEditedName] = useState(day.name || '');
  const nameInputRef = useRef<HTMLInputElement>(null);

  const exerciseCount = day.exercises.length;

  // Focus input when editing starts
  useEffect(() => {
    if (isEditingName && nameInputRef.current) {
      nameInputRef.current.focus();
      nameInputRef.current.select();
    }
  }, [isEditingName]);

  const handleNameSubmit = () => {
    setIsEditingName(false);
    if (editedName !== day.name) {
      onUpdate({ name: editedName });
    }
  };

  const handleNameKeyDown = (e: React.KeyboardEvent) => {
    if (e.key === 'Enter') {
      handleNameSubmit();
    } else if (e.key === 'Escape') {
      setEditedName(day.name || '');
      setIsEditingName(false);
    }
  };

  const handleFocusChange = (value: string) => {
    onUpdate({ focus: value === '__none__' ? null : value as DayFocus });
  };

  return (
    <div
      className={cn(
        'rounded-lg border bg-card transition-all',
        day.isNew && 'animate-in fade-in slide-in-from-top-2 duration-200',
        day.isCollapsed && 'shadow-sm'
      )}
    >
      {/* Day Header */}
      <div className="flex items-center gap-2 p-3 border-b border-border/50">
        {/* Drag Handle */}
        <div
          {...dragHandleProps}
          className="cursor-grab active:cursor-grabbing p-1 rounded hover:bg-muted opacity-50 hover:opacity-100 transition-opacity"
        >
          <GripVertical className="h-4 w-4 text-muted-foreground" />
        </div>

        {/* Collapse Toggle */}
        <Button
          variant="ghost"
          size="icon"
          className="h-7 w-7"
          onClick={onToggleCollapse}
        >
          {day.isCollapsed ? (
            <ChevronRight className="h-4 w-4" />
          ) : (
            <ChevronDown className="h-4 w-4" />
          )}
        </Button>

        {/* Day Number Badge */}
        <div className="flex items-center justify-center h-6 w-6 rounded bg-primary/10 text-primary text-xs font-medium">
          {day.dayNumber}
        </div>

        {/* Day Name */}
        {isEditingName ? (
          <Input
            ref={nameInputRef}
            value={editedName}
            onChange={(e) => setEditedName(e.target.value)}
            onBlur={handleNameSubmit}
            onKeyDown={handleNameKeyDown}
            className="h-7 w-48 text-sm font-medium"
          />
        ) : (
          <button
            onClick={() => setIsEditingName(true)}
            className="text-sm font-medium hover:underline underline-offset-2"
          >
            {day.name || `Day ${day.dayNumber}`}
          </button>
        )}

        {/* Day Focus Selector */}
        <Select value={day.focus ?? '__none__'} onValueChange={handleFocusChange}>
          <SelectTrigger className="h-7 w-32 text-xs">
            <SelectValue placeholder="Sin foco" />
          </SelectTrigger>
          <SelectContent>
            <SelectItem value="__none__">Sin foco</SelectItem>
            {DAY_FOCUS_OPTIONS.map((option) => (
              <SelectItem key={option.value} value={option.value}>
                {option.label}
              </SelectItem>
            ))}
          </SelectContent>
        </Select>

        {/* Exercise Count */}
        <div className="text-xs text-muted-foreground">
          {exerciseCount} {exerciseCount === 1 ? 'exercise' : 'exercises'}
        </div>

        {/* Spacer */}
        <div className="flex-1" />

        {/* Day Actions Menu */}
        <DropdownMenu>
          <DropdownMenuTrigger asChild>
            <Button variant="ghost" size="icon" className="h-7 w-7">
              <MoreHorizontal className="h-4 w-4" />
            </Button>
          </DropdownMenuTrigger>
          <DropdownMenuContent align="end">
            <DropdownMenuItem onClick={onDuplicate}>
              <Copy className="h-4 w-4 mr-2" />
              Duplicate day
            </DropdownMenuItem>
            <DropdownMenuSeparator />
            <DropdownMenuItem
              onClick={onDelete}
              className="text-destructive focus:text-destructive"
            >
              <Trash2 className="h-4 w-4 mr-2" />
              Delete day
            </DropdownMenuItem>
          </DropdownMenuContent>
        </DropdownMenu>
      </div>

      {/* Day Content (Exercises) */}
      {!day.isCollapsed && (
        <div className="p-3">
          <ExerciseTable
            exercises={day.exercises}
            weekId={weekId}
            dayId={day.id}
            activeExerciseId={activeExerciseId}
            exerciseOptions={exerciseOptions}
            athleteId={athleteId}
            onAddExercise={onAddExercise}
            onUpdateExercise={onUpdateExercise}
            onDeleteExercise={onDeleteExercise}
            onDuplicateExercise={onDuplicateExercise}
            onReorderExercises={onReorderExercises}
            onSetActiveExercise={onSetActiveExercise}
          />
        </div>
      )}

      {/* Collapsed Summary */}
      {day.isCollapsed && day.exercises.length > 0 && (
        <div className="px-3 py-2 text-xs text-muted-foreground">
          {day.exercises.slice(0, 3).map((ex) => ex.exerciseName).join(', ')}
          {day.exercises.length > 3 && ` +${day.exercises.length - 3} more`}
        </div>
      )}
    </div>
  );
}
