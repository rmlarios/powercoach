'use client';

import React, { useMemo, useState, useCallback, useRef, useEffect } from 'react';
import { Plus, GripVertical, Trash2, Copy, MoreHorizontal, Settings2 } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogTrigger,
} from '@/components/ui/dialog';
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
} from '@/components/ui/dropdown-menu';
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
  arrayMove,
} from '@dnd-kit/sortable';
import { CSS } from '@dnd-kit/utilities';
import {
  BuilderWeek,
  BuilderExercise,
  ExerciseOption,
  DaySlot,
  DaySlotExercise,
  ExerciseType,
  EMOMConfig,
  TempoConfig,
  SupersetConfig,
} from '@/types/builder';
import { MaxLift } from '@/types/athlete';
import { cn } from '@/utils/cn';
import { parseSetNotation, notationToBuilderFields } from '@/utils/set-notation-parser';
import { 
  calculateWorkingWeight, 
  extractPercentageFromNotation, 
  formatWeight 
} from '@/utils/weight-calculator';
import { ExerciseTypeBadge, ExerciseTypeSelector } from './exercise-type-selector';
import { QuickFillMenu, ContextMenuTrigger, QuickFillAction, QuickFillConfig } from './quick-fill-menu';
import { ExercisePickerWithHistory } from './exercise-picker-with-history';
import { transformToDaySlots, getNotationTooltip } from './day-centric-utils';

interface DayCentricViewProps {
  weeks: BuilderWeek[];
  totalWeeks: number;
  exerciseOptions: ExerciseOption[];
  /** Athlete ID for fetching exercise history */
  athleteId?: string | null;
  /** Max lifts for the selected athlete (for weight suggestions) */
  athleteMaxLifts?: MaxLift[];
  /** Weight rounding increment (default: 2.5) */
  weightRoundTo?: number;
  onUpdateExercise: (
    weekId: string,
    dayId: string,
    exerciseId: string,
    updates: Partial<BuilderExercise>
  ) => void;
  onAddExercise: (weekId: string, dayId: string, exerciseId: string, exerciseName: string, initialValues?: Partial<BuilderExercise>) => void;
  onDeleteExercise: (weekId: string, dayId: string, exerciseId: string) => void;
  onAddDay: (weekId: string) => void;
  /** Save state before bulk actions for undo */
  onPushUndo?: () => void;
  /** Register a new 1RM for an exercise */
  onRegister1RM?: (exerciseId: string, exerciseName: string, weight: number) => void;
}

// transformToDaySlots and getNotationTooltip imported from './day-centric-utils'

// ============================================
// Editable Cell Component
// ============================================
interface EditableCellProps {
  value: string;
  onChange: (value: string) => void;
  placeholder?: string;
  className?: string;
  onFocus?: () => void;
  onKeyDown?: (e: React.KeyboardEvent) => void;
}

function EditableCell({
  value,
  onChange,
  placeholder = '—',
  className,
  onFocus,
  onKeyDown,
}: EditableCellProps) {
  const [isEditing, setIsEditing] = useState(false);
  const [editValue, setEditValue] = useState(value);
  const inputRef = useRef<HTMLInputElement>(null);

  useEffect(() => {
    setEditValue(value);
  }, [value]);

  const handleBlur = () => {
    setIsEditing(false);
    if (editValue !== value) {
      onChange(editValue);
    }
  };

  const handleKeyDown = (e: React.KeyboardEvent<HTMLInputElement>) => {
    if (e.key === 'Enter') {
      handleBlur();
      onKeyDown?.(e);
    } else if (e.key === 'Escape') {
      setEditValue(value);
      setIsEditing(false);
    } else if (e.key === 'Tab' || e.key === 'ArrowRight' || e.key === 'ArrowLeft' || 
               e.key === 'ArrowUp' || e.key === 'ArrowDown') {
      handleBlur();
      onKeyDown?.(e);
    }
  };

  const handleClick = () => {
    setIsEditing(true);
    onFocus?.();
    setTimeout(() => inputRef.current?.select(), 0);
  };

  // Detect if value is compound notation (multiple groups like "1x3 78% 1x3 82%")
  const isCompound = value ? /\d+x\d+.*\s+\d+x\d+/.test(value) : false;

  return (
    <div className={cn('min-w-[80px]', className)}>
      {isEditing ? (
        <Input
          ref={inputRef}
          value={editValue}
          onChange={(e) => setEditValue(e.target.value)}
          onBlur={handleBlur}
          onKeyDown={handleKeyDown}
          className="h-8 text-sm text-center px-1"
          autoFocus
        />
      ) : (
        <div
          onClick={handleClick}
          title={value ? getNotationTooltip(value) : undefined}
          className={cn(
            'flex items-center justify-center cursor-text rounded px-1',
            'hover:bg-muted/50 transition-colors',
            isCompound ? 'min-h-[28px] py-0.5' : 'h-8',
            !value && 'text-muted-foreground'
          )}
        >
          {isCompound ? (
            <span className="text-[11px] leading-tight text-center">
              {value.split(/\s+(?=\d+x)/i).map((group, i) => (
                <span key={i}>
                  {i > 0 && <br />}
                  {group}
                </span>
              ))}
            </span>
          ) : (
            <span className="text-sm">{value || placeholder}</span>
          )}
        </div>
      )}
    </div>
  );
}

// ============================================
// Weight Suggestion Component (Editable)
// ============================================
interface WeightSuggestionProps {
  notation: string;              // The prescription notation (e.g., "3x8 @75%")
  oneRepMax: number | null;      // The 1RM for this exercise
  roundTo?: number;              // Rounding increment (default: 2.5)
  manualWeight?: number;         // Manual weight override
  onWeightChange?: (weight: number | undefined) => void; // Callback when weight is changed
  exerciseId?: string;           // For registering 1RM
  exerciseName?: string;         // For registering 1RM
  onRegister1RM?: (exerciseId: string, exerciseName: string, weight: number) => void;
}

function WeightSuggestion({ 
  notation, 
  oneRepMax, 
  roundTo = 2.5, 
  manualWeight,
  onWeightChange,
  exerciseId,
  exerciseName,
  onRegister1RM
}: WeightSuggestionProps) {
  const [isEditing, setIsEditing] = useState(false);
  const [is1RMDialogOpen, setIs1RMDialogOpen] = useState(false);
  const [editValue, setEditValue] = useState('');
  const [oneRMValue, setOneRMValue] = useState('');
  const inputRef = useRef<HTMLInputElement>(null);
  
  const percentage = extractPercentageFromNotation(notation);
  
  // Calculate suggested weight
  const suggestedWeight = percentage && oneRepMax 
    ? calculateWorkingWeight(oneRepMax, percentage, roundTo)
    : null;
  
  // Display weight: manual override > calculated
  const displayWeight = manualWeight ?? suggestedWeight;
  const hasOverride = manualWeight !== undefined && manualWeight !== suggestedWeight;
  
  // No percentage in notation
  if (!percentage) {
    return null;
  }
  
  // No 1RM available and no manual weight
  if (!oneRepMax && !manualWeight) {
    return (
      <>
        <div 
          className="h-5 flex items-center justify-center text-[10px] text-amber-600/80 italic cursor-pointer hover:bg-muted/50 rounded"
          onClick={() => {
            if (onRegister1RM && exerciseId && exerciseName) {
              setIs1RMDialogOpen(true);
            } else if (onWeightChange) {
              setEditValue('');
              setIsEditing(true);
              setTimeout(() => inputRef.current?.focus(), 0);
            }
          }}
          title={onRegister1RM ? "Click para registrar 1RM" : "Click para ingresar peso manualmente"}
        >
          No 1RM
        </div>
        
        {/* 1RM Registration Dialog */}
        {onRegister1RM && exerciseId && exerciseName && (
          <Dialog open={is1RMDialogOpen} onOpenChange={setIs1RMDialogOpen}>
            <DialogContent className="sm:max-w-[350px]" onKeyDown={(e) => e.stopPropagation()}>
              <DialogHeader>
                <DialogTitle className="text-base">Registrar 1RM</DialogTitle>
              </DialogHeader>
              <div className="space-y-4 py-2">
                <div>
                  <label className="text-sm text-muted-foreground">Ejercicio</label>
                  <p className="font-medium">{exerciseName}</p>
                </div>
                <div className="space-y-2">
                  <label className="text-sm font-medium">1RM (kg)</label>
                  <Input
                    type="number"
                    step="0.5"
                    placeholder="Ej: 100"
                    value={oneRMValue}
                    onChange={(e) => setOneRMValue(e.target.value)}
                    autoFocus
                  />
                  <p className="text-xs text-muted-foreground">
                    Este valor se usará para calcular el peso de trabajo
                  </p>
                </div>
                <div className="flex justify-end gap-2">
                  <Button 
                    variant="outline" 
                    size="sm"
                    onClick={() => {
                      setIs1RMDialogOpen(false);
                      setOneRMValue('');
                    }}
                  >
                    Cancelar
                  </Button>
                  <Button 
                    size="sm"
                    onClick={() => {
                      const weight = parseFloat(oneRMValue);
                      if (!isNaN(weight) && weight > 0) {
                        onRegister1RM(exerciseId, exerciseName, weight);
                        setIs1RMDialogOpen(false);
                        setOneRMValue('');
                      }
                    }}
                    disabled={!oneRMValue || isNaN(parseFloat(oneRMValue)) || parseFloat(oneRMValue) <= 0}
                  >
                    Guardar
                  </Button>
                </div>
              </div>
            </DialogContent>
          </Dialog>
        )}
        
        {/* Manual weight input when no onRegister1RM */}
        {isEditing && (
          <input
            ref={inputRef}
            type="number"
            step="0.5"
            value={editValue}
            onChange={(e) => setEditValue(e.target.value)}
            onBlur={() => {
              setIsEditing(false);
              if (editValue.trim() !== '') {
                const numValue = parseFloat(editValue);
                if (!isNaN(numValue) && numValue > 0) {
                  onWeightChange?.(numValue);
                }
              }
            }}
            onKeyDown={(e) => {
              if (e.key === 'Enter') {
                (e.target as HTMLInputElement).blur();
              } else if (e.key === 'Escape') {
                setIsEditing(false);
                setEditValue('');
              }
            }}
            className="h-5 w-full text-[10px] text-center bg-background border rounded px-1 absolute inset-0"
            placeholder="kg"
            autoFocus
          />
        )}
      </>
    );
  }
  
  const handleBlur = () => {
    setIsEditing(false);
    if (editValue.trim() === '') {
      // Clear manual weight, use calculated
      onWeightChange?.(undefined);
    } else {
      const numValue = parseFloat(editValue);
      if (!isNaN(numValue) && numValue > 0) {
        onWeightChange?.(numValue);
      }
    }
  };

  const handleKeyDown = (e: React.KeyboardEvent) => {
    if (e.key === 'Enter') {
      handleBlur();
    } else if (e.key === 'Escape') {
      setIsEditing(false);
      setEditValue('');
    }
  };

  if (isEditing) {
    return (
      <input
        ref={inputRef}
        type="number"
        step="0.5"
        value={editValue}
        onChange={(e) => setEditValue(e.target.value)}
        onBlur={handleBlur}
        onKeyDown={handleKeyDown}
        className="h-5 w-full text-[10px] text-center bg-background border rounded px-1"
        placeholder={suggestedWeight?.toString() || ''}
        autoFocus
      />
    );
  }
  
  return (
    <div 
      className={cn(
        "h-5 flex items-center justify-center text-[10px] font-medium cursor-pointer rounded transition-colors",
        "hover:bg-muted/50",
        hasOverride 
          ? "text-blue-600 bg-blue-50" 
          : "text-emerald-600"
      )}
      onClick={() => {
        if (onWeightChange) {
          setEditValue(displayWeight?.toString() || '');
          setIsEditing(true);
        }
      }}
      title={hasOverride 
        ? `Override: ${manualWeight}kg (calculado: ${suggestedWeight}kg) - Click para editar` 
        : `Calculado: ${displayWeight}kg - Click para override`}
    >
      {formatWeight(displayWeight!)}
      {hasOverride && <span className="ml-0.5 text-[8px]">✎</span>}
    </div>
  );
}

// ============================================
// Compound Weight Suggestion (handles multiple % groups)
// ============================================
interface CompoundWeightSuggestionProps {
  notation: string;
  singlePercentageRM?: number;
  oneRepMax: number | null;
  roundTo?: number;
  manualWeight?: number;
  onWeightChange?: (weight: number | undefined) => void;
  exerciseId?: string;
  exerciseName?: string;
  onRegister1RM?: (exerciseId: string, exerciseName: string, weight: number) => void;
}

function CompoundWeightSuggestion({
  notation,
  singlePercentageRM,
  oneRepMax,
  roundTo = 2.5,
  manualWeight,
  onWeightChange,
  exerciseId,
  exerciseName,
  onRegister1RM,
}: CompoundWeightSuggestionProps) {
  // Parse notation to extract all percentage groups
  const parsed = parseSetNotation(notation);
  const groups = parsed.isValid ? parsed.groups : [];

  // Collect all percentages: from parsed groups or from single field
  const percentages: number[] = [];
  for (const g of groups) {
    if (g.percentage) percentages.push(g.percentage);
  }
  // If no percentages from notation but we have a single percentageRM field, use that
  if (percentages.length === 0 && singlePercentageRM && singlePercentageRM > 0) {
    percentages.push(singlePercentageRM);
  }

  // No percentages at all -> nothing to show
  if (percentages.length === 0) {
    return null;
  }

  // No 1RM available — delegate to the single WeightSuggestion for 1RM registration flow
  if (!oneRepMax && !manualWeight) {
    return (
      <WeightSuggestion
        notation={singlePercentageRM ? `${singlePercentageRM}%` : notation}
        oneRepMax={oneRepMax}
        roundTo={roundTo}
        manualWeight={manualWeight}
        onWeightChange={onWeightChange}
        exerciseId={exerciseId}
        exerciseName={exerciseName}
        onRegister1RM={onRegister1RM}
      />
    );
  }

  // Single percentage — use existing WeightSuggestion (supports editing)
  if (percentages.length === 1) {
    return (
      <WeightSuggestion
        notation={singlePercentageRM ? `${singlePercentageRM}%` : notation}
        oneRepMax={oneRepMax}
        roundTo={roundTo}
        manualWeight={manualWeight}
        onWeightChange={onWeightChange}
        exerciseId={exerciseId}
        exerciseName={exerciseName}
        onRegister1RM={onRegister1RM}
      />
    );
  }

  // Multiple percentages — show all weights
  const weights = percentages.map((pct) =>
    oneRepMax ? calculateWorkingWeight(oneRepMax, pct, roundTo) : null
  );

  const weightLabels = weights.map((w, i) =>
    w !== null ? formatWeight(w) : `${percentages[i]}%`
  );

  // Build detailed tooltip
  const tooltipLines = groups.map((g, i) => {
    const pct = percentages[i] ?? g.percentage;
    const repsStr = `${g.sets}x${g.repsMin}${g.repsMax && g.repsMax !== g.repsMin ? `-${g.repsMax}` : ''}`;
    return pct ? `${repsStr} @${pct}% → ${weightLabels[i]}` : repsStr;
  });

  // Compact display: use arrow notation for ascending/descending series
  const allWeights = weights.filter((w): w is number => w !== null);
  const isAscending = allWeights.length > 1 && allWeights.every((w, i) => i === 0 || w >= allWeights[i - 1]);
  const isDescending = allWeights.length > 1 && allWeights.every((w, i) => i === 0 || w <= allWeights[i - 1]);

  let displayStr: string;
  if (allWeights.length <= 2) {
    displayStr = weightLabels.join(' / ');
  } else if (isAscending || isDescending) {
    // Show first → last for ascending/descending
    const arrow = isAscending ? '→' : '→';
    displayStr = `${weightLabels[0]} ${arrow} ${weightLabels[weightLabels.length - 1]}`;
  } else {
    // Mixed: show stacked
    displayStr = weightLabels.join(' / ');
  }

  return (
    <div
      className="min-h-[20px] flex items-center justify-center text-[10px] font-medium text-emerald-600 leading-tight text-center px-0.5 cursor-help"
      title={tooltipLines.join('\n')}
    >
      {displayStr}
    </div>
  );
}

// ============================================
// Exercise Row (Sortable)
// ============================================
interface ExerciseRowProps {
  exercise: DaySlotExercise;
  totalWeeks: number;
  weeks: BuilderWeek[];
  dayNumber: number;
  dayName: string;
  /** 1RM for this exercise (null if not available) */
  exerciseOneRM: number | null;
  /** Weight rounding increment */
  roundTo: number;
  /** Whether this exercise is part of a superset group (for visual styling) */
  supersetGroupPosition?: 'first' | 'middle' | 'last' | 'only';
  /** Color for superset grouping */
  supersetColor?: string;
  /** Available superset groups to join */
  availableSupersetGroups: { groupId: string; name: string; exerciseCount: number }[];
  onUpdateCell: (weekNumber: number, field: string, value: string) => void;
  onUpdateType: (type: ExerciseType, emomConfig?: EMOMConfig, tempoConfig?: TempoConfig, supersetConfig?: SupersetConfig) => void;
  onDelete: () => void;
  onDuplicate: () => void;
  onQuickFill: (action: QuickFillAction, config?: QuickFillConfig) => void;
  /** Register a new 1RM for an exercise */
  onRegister1RM?: (exerciseId: string, exerciseName: string, weight: number) => void;
}

function SortableExerciseRow({
  exercise,
  totalWeeks,
  // eslint-disable-next-line @typescript-eslint/no-unused-vars
  weeks,
  // eslint-disable-next-line @typescript-eslint/no-unused-vars
  dayNumber,
  dayName,
  exerciseOneRM,
  roundTo,
  supersetGroupPosition,
  availableSupersetGroups,
  supersetColor,
  onUpdateCell,
  onUpdateType,
  onDelete,
  onDuplicate,
  onQuickFill,
  onRegister1RM,
}: ExerciseRowProps) {
  const [showTypeSelector, setShowTypeSelector] = useState(false);
  const {
    attributes,
    listeners,
    setNodeRef,
    transform,
    transition,
    isDragging,
  } = useSortable({ id: exercise.exerciseId });

  const style = {
    transform: CSS.Transform.toString(transform),
    transition,
  };

  const weekNumbers = Array.from({ length: totalWeeks }, (_, i) => i + 1);
  
  // Get current week value for context menu (use first non-empty)
  const firstPrescription = Array.from(exercise.weekPrescriptions.values())[0];
  const currentValue = firstPrescription?.reps || '';

  return (
    <QuickFillMenu
      currentWeek={1}
      totalWeeks={totalWeeks}
      currentValue={currentValue}
      exerciseName={exercise.exerciseName}
      dayName={dayName}
      onAction={onQuickFill}
    >
      <ContextMenuTrigger asChild>
        <div
          ref={setNodeRef}
      style={style}
      className={cn(
        'flex items-center border-b border-border/50 bg-background group relative',
        isDragging && 'opacity-50 bg-muted',
        // Superset visual grouping - left border indicator
        supersetGroupPosition && 'pl-1',
        supersetGroupPosition === 'first' && 'rounded-tl-md',
        supersetGroupPosition === 'last' && 'rounded-bl-md',
        supersetGroupPosition === 'only' && 'rounded-l-md'
      )}
    >
      {/* Superset Group Indicator */}
      {supersetGroupPosition && (
        <div
          className={cn(
            'absolute left-0 top-0 bottom-0 w-1 rounded-l',
            supersetColor || 'bg-orange-400'
          )}
          style={supersetColor ? { backgroundColor: supersetColor } : undefined}
        />
      )}
      
      {/* Drag Handle */}
      <div
        {...attributes}
        {...listeners}
        className="w-8 flex items-center justify-center cursor-grab opacity-0 group-hover:opacity-100 transition-opacity"
      >
        <GripVertical className="h-4 w-4 text-muted-foreground" />
      </div>

      {/* Exercise Name (sticky) */}
      <div className="w-48 min-w-[192px] px-3 py-2 font-medium text-sm border-r border-border/50 sticky left-0 bg-background z-10">
        <div className="flex items-center justify-between gap-1">
          <div className="flex items-center gap-1 min-w-0 flex-1">
            <span className="truncate flex-1 min-w-0" title={exercise.exerciseName}>{exercise.exerciseName}</span>
            {exercise.exerciseType && exercise.exerciseType !== 'standard' && (
              <ExerciseTypeBadge 
                type={exercise.exerciseType} 
                emomConfig={exercise.emomConfig}
                tempoConfig={exercise.tempoConfig}
                onClick={() => setShowTypeSelector(true)}
                compact
              />
            )}
          </div>
          <DropdownMenu>
            <DropdownMenuTrigger asChild>
              <Button
                variant="ghost"
                size="icon"
                className="h-6 w-6 opacity-0 group-hover:opacity-100 flex-shrink-0"
              >
                <MoreHorizontal className="h-3.5 w-3.5" />
              </Button>
            </DropdownMenuTrigger>
            <DropdownMenuContent align="end">
              <DropdownMenuItem onClick={() => setShowTypeSelector(true)}>
                <Settings2 className="h-4 w-4 mr-2" />
                Change Type
              </DropdownMenuItem>
              <DropdownMenuItem onClick={onDuplicate}>
                <Copy className="h-4 w-4 mr-2" />
                Duplicate
              </DropdownMenuItem>
              <DropdownMenuSeparator />
              <DropdownMenuItem onClick={onDelete} className="text-destructive">
                <Trash2 className="h-4 w-4 mr-2" />
                Delete
              </DropdownMenuItem>
            </DropdownMenuContent>
          </DropdownMenu>
          
          {/* Exercise Type Dialog */}
          <Dialog open={showTypeSelector} onOpenChange={setShowTypeSelector}>
            <DialogContent 
              className="max-w-md" 
              onKeyDown={(e) => {
                // Prevent dialog from closing when pressing keys inside inputs
                if (e.target instanceof HTMLInputElement || e.target instanceof HTMLButtonElement) {
                  e.stopPropagation();
                }
              }}
            >
              <DialogHeader>
                <DialogTitle>Change Exercise Type</DialogTitle>
              </DialogHeader>
              <ExerciseTypeSelector
                inline
                value={exercise.exerciseType || 'standard'}
                onChange={(type, config) => {
                  onUpdateType(type, config?.emomConfig, config?.tempoConfig, config?.supersetConfig);
                  // Only close dialog for standard type, keep open for others to allow configuration
                  if (type === 'standard') {
                    setShowTypeSelector(false);
                  }
                }}
                emomConfig={exercise.emomConfig}
                tempoConfig={exercise.tempoConfig}
                supersetConfig={exercise.supersetConfig}
                availableSupersetGroups={availableSupersetGroups.filter(
                  // Exclude current exercise's group from available groups
                  g => g.groupId !== exercise.supersetConfig?.groupId
                )}
              />
              {/* Close button for when configuration is done */}
              {(exercise.exerciseType === 'emom' || exercise.exerciseType === 'tempo' || exercise.exerciseType === 'superset') && (
                <div className="flex justify-end pt-2 border-t">
                  <Button size="sm" onClick={() => setShowTypeSelector(false)}>
                    Done
                  </Button>
                </div>
              )}
            </DialogContent>
          </Dialog>
        </div>
      </div>

      {/* Week Cells */}
      {weekNumbers.map((weekNum) => {
        const prescription = exercise.weekPrescriptions.get(weekNum);
        const notation = prescription?.reps || '';
        const hasPercentage = notation.includes('%') || (prescription?.percentageRM && prescription.percentageRM > 0);
        
        return (
          <div
            key={weekNum}
            className="w-32 min-w-[128px] border-r border-border/30 flex flex-col"
          >
            {/* Main prescription (sets x reps) */}
            <EditableCell
              value={notation}
              onChange={(val) => onUpdateCell(weekNum, 'reps', val)}
              placeholder="—"
              className="border-b border-border/20"
            />
            {/* Secondary info (RPE or %) - fixed height */}
            <EditableCell
              value={prescription?.rpeTarget ? `@${prescription.rpeTarget}` : (prescription?.percentageRM ? `${prescription.percentageRM}%` : '')}
              onChange={(val) => {
                // Parse @X format for RPE or X% for percentage
                if (val.includes('%')) {
                  const pct = val.replace('%', '');
                  onUpdateCell(weekNum, 'percentage', pct);
                } else {
                  const rpe = val.replace('@', '');
                  onUpdateCell(weekNum, 'rpe', rpe);
                }
              }}
              placeholder=""
              className="text-xs text-muted-foreground h-6"
            />
            {/* Weight suggestion area - always reserve space for consistent row height */}
            <div className="min-h-[20px] flex items-center justify-center">
              {hasPercentage ? (
                <CompoundWeightSuggestion
                  notation={notation}
                  singlePercentageRM={prescription?.percentageRM}
                  oneRepMax={exerciseOneRM}
                  roundTo={roundTo}
                  manualWeight={prescription?.weight}
                  onWeightChange={(weight) => 
                    onUpdateCell(weekNum, 'weight', weight?.toString() ?? '')
                  }
                  exerciseId={exercise.exerciseId}
                  exerciseName={exercise.exerciseName}
                  onRegister1RM={onRegister1RM}
                />
              ) : (
                <span className="text-[10px] text-muted-foreground/50">—</span>
              )}
            </div>
          </div>
        );
      })}
        </div>
      </ContextMenuTrigger>
    </QuickFillMenu>
  );
}

// ============================================
// Day Tab Component
// ============================================
interface DayTabProps {
  daySlot: DaySlot;
  isActive: boolean;
  onClick: () => void;
}

function DayTab({ daySlot, isActive, onClick }: DayTabProps) {
  return (
    <button
      onClick={onClick}
      className={cn(
        'px-4 py-2 text-sm font-medium rounded-t-lg transition-colors whitespace-nowrap',
        'border-b-2 -mb-[2px]',
        isActive
          ? 'bg-background border-primary text-foreground'
          : 'bg-muted/30 border-transparent text-muted-foreground hover:text-foreground hover:bg-muted/50'
      )}
    >
      <span>{daySlot.name}</span>
      <span className="ml-2 text-xs text-muted-foreground">
        ({daySlot.exercises.length})
      </span>
    </button>
  );
}

// ============================================
// Main Day-Centric View Component
// ============================================
export function DayCentricView({
  weeks,
  totalWeeks,
  exerciseOptions,
  athleteId,
  athleteMaxLifts = [],
  weightRoundTo = 2.5,
  onUpdateExercise,
  onAddExercise,
  onDeleteExercise,
  onAddDay,
  onPushUndo,
  onRegister1RM,
}: DayCentricViewProps) {
  const [activeDayNumber, setActiveDayNumber] = useState<number>(1);
  const [draggedExerciseId, setDraggedExerciseId] = useState<string | null>(null);
  const [isAddExerciseOpen, setIsAddExerciseOpen] = useState(false);

  // Create a map of exerciseId -> 1RM for quick lookup
  const exerciseOneRMMap = useMemo(() => {
    const map = new Map<string, number>();
    athleteMaxLifts.forEach((lift) => {
      map.set(lift.exerciseId, lift.weight);
    });
    return map;
  }, [athleteMaxLifts]);

  // Transform data to day-centric structure
  const daySlots = useMemo(() => transformToDaySlots(weeks), [weeks]);

  // Get active day slot
  const activeDaySlot = useMemo(
    () => daySlots.find((d) => d.dayNumber === activeDayNumber) || daySlots[0],
    [daySlots, activeDayNumber]
  );

  // Calculate superset groupings for visual display
  const supersetGroups = useMemo(() => {
    if (!activeDaySlot) return new Map<string, { position: 'first' | 'middle' | 'last' | 'only'; color: string }>();
    
    const groups = new Map<string, { position: 'first' | 'middle' | 'last' | 'only'; color: string }>();
    const supersetExercises = activeDaySlot.exercises.filter(ex => 
      ex.exerciseType === 'superset' && ex.supersetConfig?.groupId
    );
    
    // Group by groupId
    const groupMap = new Map<string, DaySlotExercise[]>();
    supersetExercises.forEach(ex => {
      const groupId = ex.supersetConfig!.groupId;
      if (!groupMap.has(groupId)) {
        groupMap.set(groupId, []);
      }
      groupMap.get(groupId)!.push(ex);
    });
    
    // Colors for different superset groups
    const colors = ['bg-orange-400', 'bg-blue-400', 'bg-green-400', 'bg-purple-400', 'bg-pink-400', 'bg-yellow-400'];
    let colorIndex = 0;
    
    // Calculate positions for each exercise in superset
    groupMap.forEach((exercises) => {
      const color = colors[colorIndex % colors.length];
      colorIndex++;
      
      // Sort by position if available
      exercises.sort((a, b) => (a.supersetConfig?.position || 0) - (b.supersetConfig?.position || 0));
      
      exercises.forEach((ex, idx) => {
        let position: 'first' | 'middle' | 'last' | 'only' = 'middle';
        if (exercises.length === 1) {
          position = 'only';
        } else if (idx === 0) {
          position = 'first';
        } else if (idx === exercises.length - 1) {
          position = 'last';
        }
        groups.set(ex.exerciseId, { position, color });
      });
    });
    
    return groups;
  }, [activeDaySlot]);

  // Get available superset groups for joining
  const availableSupersetGroups = useMemo(() => {
    if (!activeDaySlot) return [];
    
    const groupMap = new Map<string, { groupId: string; name: string; exerciseCount: number }>();
    
    activeDaySlot.exercises.forEach(ex => {
      if (ex.exerciseType === 'superset' && ex.supersetConfig?.groupId) {
        const groupId = ex.supersetConfig.groupId;
        const existing = groupMap.get(groupId);
        if (existing) {
          existing.exerciseCount++;
          existing.name += ` + ${ex.exerciseName}`;
        } else {
          groupMap.set(groupId, {
            groupId,
            name: ex.exerciseName,
            exerciseCount: 1,
          });
        }
      }
    });
    
    return Array.from(groupMap.values()).map(g => ({
      ...g,
      name: g.exerciseCount > 1 ? `Superset: ${g.name.split(' + ').slice(0, 2).join(' + ')}${g.exerciseCount > 2 ? '...' : ''}` : `Superset: ${g.name}`,
    }));
  }, [activeDaySlot]);

  // Handle adding exercise to active day (across all weeks)
  const handleAddExerciseToDay = useCallback(
    (exercise: ExerciseOption) => {
      // Add exercise to each week for this day
      weeks.forEach((week) => {
        const day = week.days.find((d) => d.dayNumber === activeDayNumber);
        if (day) {
          onAddExercise(week.id, day.id, exercise.id, exercise.name);
        }
      });
      setIsAddExerciseOpen(false);
    },
    [weeks, activeDayNumber, onAddExercise]
  );

  // DnD sensors
  const sensors = useSensors(
    useSensor(PointerSensor, { activationConstraint: { distance: 8 } }),
    useSensor(KeyboardSensor, { coordinateGetter: sortableKeyboardCoordinates })
  );

  // Handle cell update with Smart Notation Parser
  const handleCellUpdate = useCallback(
    (exercise: DaySlotExercise, weekNumber: number, field: string, value: string) => {
      const prescription = exercise.weekPrescriptions.get(weekNumber);
      if (!prescription) return;

      if (field === 'reps') {
        // Use Smart Notation Parser
        const parsed = parseSetNotation(value);
        if (parsed.isValid) {
          const fields = notationToBuilderFields(parsed);
          onUpdateExercise(prescription.weekId, prescription.dayId, prescription.exerciseEntryId, {
            sets: fields.sets,
            repsMin: fields.repsMin,
            repsMax: fields.repsMax,
            rpeTarget: fields.rpeTarget,
            percentageRM: fields.percentageRM,
            notes: fields.notes,
            rawNotation: fields.rawNotation, // Store raw notation for compound sets
          });
        } else {
          // Store raw value in notes if can't parse
          onUpdateExercise(prescription.weekId, prescription.dayId, prescription.exerciseEntryId, {
            notes: value,
          });
        }
      } else if (field === 'rpe') {
        const rpe = parseFloat(value.replace('@', ''));
        if (!isNaN(rpe) && rpe >= 1 && rpe <= 10) {
          onUpdateExercise(prescription.weekId, prescription.dayId, prescription.exerciseEntryId, {
            rpeTarget: rpe,
          });
        }
      } else if (field === 'percentage') {
        const pct = parseFloat(value.replace('%', ''));
        if (!isNaN(pct) && pct > 0 && pct <= 120) {
          onUpdateExercise(prescription.weekId, prescription.dayId, prescription.exerciseEntryId, {
            percentageRM: pct,
          });
        }
      } else if (field === 'weight') {
        // Handle manual weight override
        if (value === '' || value === 'undefined') {
          // Clear manual weight (use calculated)
          onUpdateExercise(prescription.weekId, prescription.dayId, prescription.exerciseEntryId, {
            weight: undefined,
          });
        } else {
          const weight = parseFloat(value);
          if (!isNaN(weight) && weight > 0) {
            onUpdateExercise(prescription.weekId, prescription.dayId, prescription.exerciseEntryId, {
              weight: weight,
            });
          }
        }
      }
    },
    [onUpdateExercise]
  );

  // Handle exercise delete (across all weeks)
  const handleDeleteExercise = useCallback(
    (exercise: DaySlotExercise) => {
      // Delete from all weeks where this exercise exists
      exercise.weekPrescriptions.forEach((prescription) => {
        onDeleteExercise(prescription.weekId, prescription.dayId, prescription.exerciseEntryId);
      });
    },
    [onDeleteExercise]
  );

  // Handle exercise type change (across all weeks)
  const handleUpdateType = useCallback(
    (exercise: DaySlotExercise, type: ExerciseType, emomConfig?: EMOMConfig, tempoConfig?: TempoConfig, supersetConfig?: SupersetConfig) => {
      // Update type in all weeks where this exercise exists
      exercise.weekPrescriptions.forEach((prescription) => {
        onUpdateExercise(prescription.weekId, prescription.dayId, prescription.exerciseEntryId, {
          exerciseType: type,
          emomConfig: type === 'emom' ? emomConfig : undefined,
          tempoConfig: type === 'tempo' ? tempoConfig : undefined,
          supersetConfig: type === 'superset' ? supersetConfig : undefined,
        });
      });
    },
    [onUpdateExercise]
  );

  // Handle Quick Fill Actions
  const handleQuickFillAction = useCallback(
    (exercise: DaySlotExercise, action: QuickFillAction, config?: QuickFillConfig) => {
      // Save state for undo before bulk action
      onPushUndo?.();
      
      const prescriptions = Array.from(exercise.weekPrescriptions.entries()).sort(
        ([a], [b]) => a - b
      );
      
      // Get first week's prescription as source
      const [firstWeekNum, firstPrescription] = prescriptions[0] || [1, null];
      if (!firstPrescription) return;

      // Parse the source notation to get structured fields
      const sourceParsed = parseSetNotation(firstPrescription.reps || '');
      const sourceFields = sourceParsed.isValid ? notationToBuilderFields(sourceParsed) : null;
      
      // Build initial values for adding exercises to missing weeks
      const initialValues: Partial<BuilderExercise> = {
        sets: sourceFields?.sets ?? firstPrescription.sets,
        repsMin: sourceFields?.repsMin ?? 8,
        repsMax: sourceFields?.repsMax ?? sourceFields?.repsMin ?? 8,
        rpeTarget: firstPrescription.rpeTarget ?? sourceFields?.rpeTarget,
        percentageRM: firstPrescription.percentageRM ?? sourceFields?.percentageRM,
        rawNotation: sourceFields?.rawNotation,
        exerciseType: exercise.exerciseType,
        emomConfig: exercise.emomConfig,
        tempoConfig: exercise.tempoConfig,
        supersetConfig: exercise.supersetConfig,
      };
      
      // Get all weekNumbers where exercise exists
      const existingWeekNums = new Set(prescriptions.map(([num]) => num));

      switch (action) {
        case 'copy-to-all': {
          // First, add exercise to weeks where it doesn't exist
          for (let weekNum = 1; weekNum <= totalWeeks; weekNum++) {
            if (!existingWeekNums.has(weekNum)) {
              // Find the week and day
              const week = weeks.find(w => w.weekNumber === weekNum);
              const day = week?.days.find(d => d.dayNumber === activeDayNumber);
              if (week && day) {
                onAddExercise(week.id, day.id, exercise.exerciseId, exercise.exerciseName, initialValues);
              }
            }
          }
          
          // Then update existing prescriptions (skip source week)
          prescriptions.forEach(([weekNum, prescription]) => {
            if (weekNum === firstWeekNum) return; // Skip source week
            onUpdateExercise(prescription.weekId, prescription.dayId, prescription.exerciseEntryId, {
              sets: sourceFields?.sets ?? firstPrescription.sets,
              repsMin: sourceFields?.repsMin,
              repsMax: sourceFields?.repsMax,
              rpeTarget: firstPrescription.rpeTarget ?? sourceFields?.rpeTarget,
              percentageRM: firstPrescription.percentageRM ?? sourceFields?.percentageRM,
              rawNotation: sourceFields?.rawNotation, // Copy compound notation
            });
          });
          break;
        }
        
        case 'copy-to-remaining': {
          // Add exercise to weeks after first that don't have it
          for (let weekNum = firstWeekNum + 1; weekNum <= totalWeeks; weekNum++) {
            if (!existingWeekNums.has(weekNum)) {
              // Find the week and day
              const week = weeks.find(w => w.weekNumber === weekNum);
              const day = week?.days.find(d => d.dayNumber === activeDayNumber);
              if (week && day) {
                onAddExercise(week.id, day.id, exercise.exerciseId, exercise.exerciseName, initialValues);
              }
            }
          }
          
          // Copy from current week to all following weeks that already have the exercise
          prescriptions.forEach(([weekNum, prescription]) => {
            if (weekNum <= firstWeekNum) return; // Skip current and previous weeks
            onUpdateExercise(prescription.weekId, prescription.dayId, prescription.exerciseEntryId, {
              sets: sourceFields?.sets ?? firstPrescription.sets,
              repsMin: sourceFields?.repsMin,
              repsMax: sourceFields?.repsMax,
              rpeTarget: firstPrescription.rpeTarget ?? sourceFields?.rpeTarget,
              percentageRM: firstPrescription.percentageRM ?? sourceFields?.percentageRM,
              rawNotation: sourceFields?.rawNotation, // Copy compound notation
            });
          });
          break;
        }
        
        case 'apply-increment':
        case 'apply-decrement': {
          // Apply progressive increment/decrement
          const step = config?.percentageStep || 2.5;
          const basePercentage = firstPrescription.percentageRM || 0;
          
          prescriptions.forEach(([weekNum, prescription]) => {
            const weekOffset = weekNum - firstWeekNum;
            const newPercentage = action === 'apply-increment'
              ? basePercentage + (step * weekOffset)
              : basePercentage - (step * weekOffset);
            
            if (newPercentage > 0 && newPercentage <= 120) {
              onUpdateExercise(prescription.weekId, prescription.dayId, prescription.exerciseEntryId, {
                percentageRM: Math.round(newPercentage * 10) / 10,
              });
            }
          });
          break;
        }
        
        case 'insert-deload': {
          // Insert deload at specified week
          const targetWeekOffset = config?.weeksBeforeDeload || 3;
          const reduction = config?.deloadReduction || 40;
          const targetWeekNum = firstWeekNum + targetWeekOffset;
          
          const targetPrescription = exercise.weekPrescriptions.get(targetWeekNum);
          const prevPrescription = exercise.weekPrescriptions.get(targetWeekNum - 1);
          
          if (targetPrescription && prevPrescription) {
            // Reduce volume (sets) and/or intensity (percentage)
            const newSets = Math.max(1, Math.round((prevPrescription.sets || 3) * (1 - reduction / 100)));
            const newPercentage = prevPrescription.percentageRM 
              ? Math.round(prevPrescription.percentageRM * (1 - reduction / 100) * 10) / 10
              : undefined;
            
            onUpdateExercise(targetPrescription.weekId, targetPrescription.dayId, targetPrescription.exerciseEntryId, {
              sets: newSets,
              percentageRM: newPercentage,
              notes: 'Deload',
            });
          }
          break;
        }
        
        case 'repeat-pattern': {
          // Repeat first 4 weeks pattern for remaining weeks
          const patternLength = Math.min(4, Math.floor(totalWeeks / 2));
          
          prescriptions.forEach(([weekNum, prescription]) => {
            if (weekNum <= patternLength) return; // Skip pattern source weeks
            
            const sourceWeekNum = ((weekNum - 1) % patternLength) + 1;
            const patternSource = exercise.weekPrescriptions.get(sourceWeekNum);
            
            if (patternSource) {
              const patternParsed = parseSetNotation(patternSource.reps || '');
              const patternFields = patternParsed.isValid ? notationToBuilderFields(patternParsed) : null;
              
              onUpdateExercise(prescription.weekId, prescription.dayId, prescription.exerciseEntryId, {
                sets: patternFields?.sets ?? patternSource.sets,
                repsMin: patternFields?.repsMin,
                repsMax: patternFields?.repsMax,
                rpeTarget: patternSource.rpeTarget ?? patternFields?.rpeTarget,
                percentageRM: patternSource.percentageRM ?? patternFields?.percentageRM,
                rawNotation: patternFields?.rawNotation, // Copy compound notation
              });
            }
          });
          break;
        }
        
        case 'clear-row': {
          // Clear all values for this exercise
          prescriptions.forEach(([, prescription]) => {
            onUpdateExercise(prescription.weekId, prescription.dayId, prescription.exerciseEntryId, {
              sets: 0,
              repsMin: 0,
              repsMax: 0,
              rpeTarget: undefined,
              percentageRM: undefined,
              notes: '',
              rawNotation: undefined, // Clear compound notation
            });
          });
          break;
        }
      }
    },
    [onUpdateExercise, onAddExercise, weeks, totalWeeks, activeDayNumber, onPushUndo]
  );

  // Handle drag end for reordering
  const handleDragEnd = (event: DragEndEvent) => {
    setDraggedExerciseId(null);
    const { active, over } = event;
    if (!over || active.id === over.id || !activeDaySlot) return;

    const oldIndex = activeDaySlot.exercises.findIndex((e) => e.exerciseId === active.id);
    const newIndex = activeDaySlot.exercises.findIndex((e) => e.exerciseId === over.id);
    if (oldIndex === -1 || newIndex === -1) return;

    // Save undo state before bulk reorder
    onPushUndo?.();

    // Build new order: compute the reordered list and assign sequential order values
    const reordered = arrayMove(activeDaySlot.exercises, oldIndex, newIndex);

    // Update order for every affected exercise across ALL weeks
    reordered.forEach((exercise, idx) => {
      const newOrder = idx + 1;
      if (exercise.order !== newOrder) {
        exercise.weekPrescriptions.forEach((prescription) => {
          onUpdateExercise(prescription.weekId, prescription.dayId, prescription.exerciseEntryId, {
            order: newOrder,
          });
        });
      }
    });
  };

  const handleDragStart = (event: DragStartEvent) => {
    setDraggedExerciseId(event.active.id as string);
  };

  const weekNumbers = Array.from({ length: totalWeeks }, (_, i) => i + 1);

  // Auto-select first day if none active
  useEffect(() => {
    if (daySlots.length > 0 && !daySlots.find((d) => d.dayNumber === activeDayNumber)) {
      setActiveDayNumber(daySlots[0].dayNumber);
    }
  }, [daySlots, activeDayNumber]);

  if (daySlots.length === 0) {
    return (
      <div className="flex flex-col items-center justify-center h-[400px] border rounded-xl bg-muted/10">
        <div className="text-center">
          <h3 className="text-lg font-semibold mb-2">No training days yet</h3>
          <p className="text-sm text-muted-foreground mb-4">
            Add weeks and days in the Weekly Builder view first.
          </p>
          <Button variant="outline">
            Switch to Weekly Builder
          </Button>
        </div>
      </div>
    );
  }

  return (
    <div className="flex flex-col h-[calc(100vh-200px)] border rounded-xl overflow-hidden bg-background">
      {/* Day Tabs */}
      <div className="flex items-center gap-1 px-4 pt-3 border-b bg-muted/20 overflow-x-auto">
        {daySlots.map((daySlot) => (
          <DayTab
            key={daySlot.dayNumber}
            daySlot={daySlot}
            isActive={daySlot.dayNumber === activeDayNumber}
            onClick={() => setActiveDayNumber(daySlot.dayNumber)}
          />
        ))}
        <Button
          variant="ghost"
          size="sm"
          className="ml-2 text-muted-foreground"
          onClick={() => {
            // Add day to all weeks
            weeks.forEach(week => onAddDay(week.id));
            // Switch to the new day
            const newDayNumber = daySlots.length + 1;
            setActiveDayNumber(newDayNumber);
          }}
        >
          <Plus className="h-4 w-4 mr-1" />
          Add Day
        </Button>
      </div>

      {/* Grid Container */}
      <div className="flex-1 overflow-auto">
        {/* Header Row */}
        <div className="flex items-center border-b border-border sticky top-0 bg-muted/50 z-20">
          {/* Spacer for drag handle */}
          <div className="w-8" />
          {/* Exercise column header */}
          <div className="w-48 min-w-[192px] px-3 py-2 font-semibold text-sm border-r border-border sticky left-0 bg-muted/50 z-10">
            Exercise
          </div>
          {/* Week headers */}
          {weekNumbers.map((weekNum) => (
            <div
              key={weekNum}
              className="w-32 min-w-[128px] py-2 text-center font-semibold text-sm border-r border-border/50"
            >
              S{weekNum}
            </div>
          ))}
        </div>

        {/* Exercise Rows */}
        {activeDaySlot && (
          <DndContext
            sensors={sensors}
            collisionDetection={closestCenter}
            onDragStart={handleDragStart}
            onDragEnd={handleDragEnd}
          >
            <SortableContext
              items={activeDaySlot.exercises.map((e) => e.exerciseId)}
              strategy={verticalListSortingStrategy}
            >
              {activeDaySlot.exercises.map((exercise) => {
                const supersetInfo = supersetGroups.get(exercise.exerciseId);
                return (
                <SortableExerciseRow
                  key={exercise.exerciseId}
                  exercise={exercise}
                  totalWeeks={totalWeeks}
                  weeks={weeks}
                  dayNumber={activeDayNumber}
                  dayName={activeDaySlot.name}
                  exerciseOneRM={exerciseOneRMMap.get(exercise.exerciseId) || null}
                  roundTo={weightRoundTo}
                  supersetGroupPosition={supersetInfo?.position}
                  supersetColor={supersetInfo?.color}
                  availableSupersetGroups={availableSupersetGroups}
                  onUpdateCell={(weekNum, field, value) =>
                    handleCellUpdate(exercise, weekNum, field, value)
                  }
                  onUpdateType={(type, emomConfig, tempoConfig, supersetConfig) =>
                    handleUpdateType(exercise, type, emomConfig, tempoConfig, supersetConfig)
                  }
                  onDelete={() => handleDeleteExercise(exercise)}
                  onDuplicate={() => {
                    // TODO: Implement duplicate
                  }}
                  onQuickFill={(action, config) =>
                    handleQuickFillAction(exercise, action, config)
                  }
                  onRegister1RM={onRegister1RM}
                />
              );
              })}
            </SortableContext>

            {/* Drag Overlay */}
            <DragOverlay>
              {draggedExerciseId ? (
                <div className="bg-background border rounded-md shadow-lg px-4 py-2 opacity-90">
                  {activeDaySlot.exercises.find((e) => e.exerciseId === draggedExerciseId)?.exerciseName}
                </div>
              ) : null}
            </DragOverlay>
          </DndContext>
        )}

        {/* Add Exercise Button with Dialog */}
        {activeDaySlot && (
          <div className="flex items-center border-b border-border/50 bg-muted/10">
            <div className="w-8" />
            <Dialog open={isAddExerciseOpen} onOpenChange={setIsAddExerciseOpen}>
              <DialogTrigger asChild>
                <Button
                  variant="ghost"
                  size="sm"
                  className="w-48 min-w-[192px] justify-start px-3 py-2 text-muted-foreground hover:text-foreground"
                >
                  <Plus className="h-4 w-4 mr-2" />
                  Add Exercise
                </Button>
              </DialogTrigger>
              <DialogContent className="max-w-3xl p-0">
                <DialogHeader className="px-4 pt-4 pb-2">
                  <DialogTitle>Add Exercise to {activeDaySlot.name}</DialogTitle>
                  <p className="text-xs text-muted-foreground">
                    Exercise will be added to all {totalWeeks} weeks
                  </p>
                </DialogHeader>
                <ExercisePickerWithHistory
                  exerciseOptions={exerciseOptions}
                  athleteId={athleteId}
                  onSelect={handleAddExerciseToDay}
                />
              </DialogContent>
            </Dialog>
          </div>
        )}
      </div>

      {/* Footer with instructions */}
      <div className="px-4 py-2 border-t bg-muted/20 text-xs text-muted-foreground">
        <span className="mr-4">💡 Click any cell to edit</span>
        <span className="mr-4">Tab to move between cells</span>
        <span>Drag to reorder exercises</span>
      </div>
    </div>
  );
}
