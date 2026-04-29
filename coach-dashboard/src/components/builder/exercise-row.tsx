'use client';

import React, { useState, useRef, useEffect, KeyboardEvent } from 'react';
import { GripVertical, Copy, Trash2, TrendingUp } from 'lucide-react';
import { Input } from '@/components/ui/input';
import { Button } from '@/components/ui/button';
import { BuilderExercise } from '@/types/builder';
import { cn } from '@/utils/cn';

interface ExerciseRowProps {
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
  onViewProgression?: () => void;
  dragHandleProps?: React.HTMLAttributes<HTMLDivElement>;
}

export function ExerciseRow({
  exercise,
  isActive,
  onUpdate,
  onDelete,
  onDuplicate,
  onActivate,
  onMoveToNext,
  onMoveToPrev,
  onViewProgression,
  dragHandleProps,
}: ExerciseRowProps) {
  const [isHovered, setIsHovered] = useState(false);
  const inputRefs = useRef<(HTMLInputElement | null)[]>([]);

  // Field definitions
  const fields = [
    { key: 'exerciseName', type: 'text' as const, width: 'flex-1 min-w-[200px]' },
    { key: 'sets', type: 'number' as const, width: 'w-16' },
    { key: 'repsMin', type: 'number' as const, width: 'w-16' },
    { key: 'repsMax', type: 'number' as const, width: 'w-16' },
    { key: 'rpeTarget', type: 'number' as const, width: 'w-16' },
    { key: 'restSeconds', type: 'number' as const, width: 'w-20' },
    { key: 'notes', type: 'text' as const, width: 'w-32' },
  ];

  // Handle keyboard navigation
  const handleKeyDown = (index: number, e: KeyboardEvent<HTMLInputElement>) => {
    if (e.key === 'Tab') {
      if (e.shiftKey) {
        // Shift+Tab: move to previous field or previous row
        if (index > 0) {
          e.preventDefault();
          inputRefs.current[index - 1]?.focus();
        } else {
          onMoveToPrev();
        }
      } else {
        // Tab: move to next field or next row
        if (index < fields.length - 1) {
          e.preventDefault();
          inputRefs.current[index + 1]?.focus();
        } else {
          e.preventDefault();
          onMoveToNext();
        }
      }
    } else if (e.key === 'Enter') {
      e.preventDefault();
      onMoveToNext();
    } else if (e.key === 'Escape') {
      e.currentTarget.blur();
    } else if (e.key === 'ArrowDown') {
      e.preventDefault();
      onMoveToNext();
    } else if (e.key === 'ArrowUp') {
      e.preventDefault();
      onMoveToPrev();
    }
  };

  // Auto-focus first field when becoming active
  useEffect(() => {
    if (isActive && inputRefs.current[0]) {
      inputRefs.current[0].focus();
    }
  }, [isActive]);

  const getValue = (key: string): string | number => {
    const value = exercise[key as keyof BuilderExercise];
    if (value === undefined || value === null) return '';
    return value as string | number;
  };

  const handleChange = (key: string, value: string, type: 'text' | 'number') => {
    if (type === 'number') {
      const numValue = value === '' ? undefined : parseInt(value, 10);
      onUpdate({ [key]: numValue });
    } else {
      onUpdate({ [key]: value });
    }
  };

  return (
    <div
      className={cn(
        'group flex items-center gap-2 py-1.5 px-2 rounded-md transition-colors',
        'hover:bg-muted/50',
        isActive && 'bg-muted/70 ring-1 ring-primary/20',
        exercise.isNew && 'animate-in fade-in slide-in-from-left-2 duration-200'
      )}
      onMouseEnter={() => setIsHovered(true)}
      onMouseLeave={() => setIsHovered(false)}
      onClick={onActivate}
    >
      {/* Drag Handle */}
      <div
        {...dragHandleProps}
        className={cn(
          'cursor-grab active:cursor-grabbing p-1 rounded hover:bg-muted',
          'opacity-0 group-hover:opacity-100 transition-opacity',
          isHovered && 'opacity-100'
        )}
      >
        <GripVertical className="h-4 w-4 text-muted-foreground" />
      </div>

      {/* Fields */}
      <div className="flex items-center gap-2 flex-1">
        {fields.map((field, index) => (
          <Input
            key={field.key}
            ref={(el) => {
              inputRefs.current[index] = el;
            }}
            type={field.type}
            value={getValue(field.key)}
            onChange={(e) => handleChange(field.key, e.target.value, field.type)}
            onKeyDown={(e) => handleKeyDown(index, e)}
            onFocus={onActivate}
            className={cn(
              field.width,
              'h-8 text-sm bg-transparent border-transparent',
              'hover:border-input focus:border-input focus:ring-1 focus:ring-primary/30',
              'placeholder:text-muted-foreground/50'
            )}
            placeholder={
              field.key === 'exerciseName'
                ? 'Exercise name...'
                : field.key === 'notes'
                ? 'Notes...'
                : undefined
            }
          />
        ))}
      </div>

      {/* Actions */}
      <div
        className={cn(
          'flex items-center gap-0.5 opacity-0 group-hover:opacity-100 transition-opacity',
          isHovered && 'opacity-100'
        )}
      >
        <Button
          variant="ghost"
          size="icon"
          className="h-7 w-7"
          onClick={(e) => {
            e.stopPropagation();
            onDuplicate();
          }}
          title="Duplicate exercise"
        >
          <Copy className="h-3.5 w-3.5 text-muted-foreground" />
        </Button>
        {onViewProgression && (
          <Button
            variant="ghost"
            size="icon"
            className="h-7 w-7 hover:text-primary"
            onClick={(e) => {
              e.stopPropagation();
              onViewProgression();
            }}
            title="Ver progresión"
          >
            <TrendingUp className="h-3.5 w-3.5" />
          </Button>
        )}
        <Button
          variant="ghost"
          size="icon"
          className="h-7 w-7 hover:text-destructive"
          onClick={(e) => {
            e.stopPropagation();
            onDelete();
          }}
          title="Delete exercise"
        >
          <Trash2 className="h-3.5 w-3.5" />
        </Button>
      </div>
    </div>
  );
}
