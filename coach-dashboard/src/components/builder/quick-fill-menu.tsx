'use client';

import React, { useState } from 'react';
import {
  Copy,
  CopyPlus,
  TrendingUp,
  TrendingDown,
  RefreshCw,
  Target,
  Percent,
  ArrowRight,
} from 'lucide-react';
import {
  ContextMenu,
  ContextMenuContent,
  ContextMenuItem,
  ContextMenuSeparator,
  ContextMenuSub,
  ContextMenuSubContent,
  ContextMenuSubTrigger,
  ContextMenuShortcut,
  ContextMenuLabel,
} from '@/components/ui/context-menu';
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from '@/components/ui/dialog';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select';

// ============================================
// Types
// ============================================

export type QuickFillAction =
  | 'copy-to-all'
  | 'copy-to-remaining'
  | 'apply-increment'
  | 'apply-decrement'
  | 'insert-deload'
  | 'repeat-pattern'
  | 'clear-row';

export interface QuickFillConfig {
  /** Increment/decrement percentage per week */
  percentageStep?: number;
  /** Number of weeks before deload */
  weeksBeforeDeload?: number;
  /** Deload reduction percentage */
  deloadReduction?: number;
  /** Pattern to repeat (array of week numbers) */
  patternWeeks?: number[];
}

export interface QuickFillResult {
  action: QuickFillAction;
  config?: QuickFillConfig;
  confirmed: boolean;
}

interface QuickFillMenuProps {
  children: React.ReactNode;
  /** Current week number (1-indexed) */
  currentWeek: number;
  /** Total weeks in the program */
  totalWeeks: number;
  /** Current cell value */
  currentValue: string;
  /** Exercise name for context */
  exerciseName: string;
  /** Day name for context */
  dayName: string;
  /** Callback when an action is selected */
  onAction: (action: QuickFillAction, config?: QuickFillConfig) => void;
  /** Whether the row has values to copy */
  hasValues?: boolean;
}

// ============================================
// Increment Dialog
// ============================================

interface IncrementDialogProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  isIncrement: boolean;
  onConfirm: (step: number) => void;
  exerciseName: string;
  totalWeeks: number;
  currentWeek: number;
}

function IncrementDialog({
  open,
  onOpenChange,
  isIncrement,
  onConfirm,
  exerciseName,
  totalWeeks,
  currentWeek,
}: IncrementDialogProps) {
  const [step, setStep] = useState<string>('2');
  const weeksRemaining = totalWeeks - currentWeek;

  const handleConfirm = () => {
    const value = parseFloat(step);
    if (!isNaN(value) && value > 0) {
      onConfirm(isIncrement ? value : -value);
      onOpenChange(false);
    }
  };

  const previewValues = () => {
    const baseStep = parseFloat(step) || 0;
    const values: string[] = [];
    for (let i = 0; i <= Math.min(weeksRemaining, 4); i++) {
      const change = isIncrement ? baseStep * i : -baseStep * i;
      values.push(`${change >= 0 ? '+' : ''}${change}%`);
    }
    return values;
  };

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-[425px]">
        <DialogHeader>
          <DialogTitle>
            {isIncrement ? '📈 Aplicar Incremento' : '📉 Aplicar Decremento'}
          </DialogTitle>
          <DialogDescription>
            {isIncrement
              ? `Incrementar el porcentaje progresivamente desde S${currentWeek} para ${exerciseName}`
              : `Decrementar el porcentaje progresivamente desde S${currentWeek} para ${exerciseName}`}
          </DialogDescription>
        </DialogHeader>
        <div className="grid gap-4 py-4">
          <div className="grid grid-cols-4 items-center gap-4">
            <Label htmlFor="step" className="text-right">
              % por semana
            </Label>
            <Input
              id="step"
              type="number"
              min="0.5"
              max="10"
              step="0.5"
              value={step}
              onChange={(e) => setStep(e.target.value)}
              className="col-span-3"
            />
          </div>
          <div className="grid grid-cols-4 items-center gap-4">
            <Label className="text-right text-muted-foreground">Preview</Label>
            <div className="col-span-3 flex gap-2 text-sm">
              {previewValues().map((val, i) => (
                <span
                  key={i}
                  className={`px-2 py-1 rounded ${
                    isIncrement
                      ? 'bg-emerald-50 text-emerald-700'
                      : 'bg-amber-50 text-amber-700'
                  }`}
                >
                  S{currentWeek + i}: {val}
                </span>
              ))}
              {weeksRemaining > 4 && <span className="text-muted-foreground">...</span>}
            </div>
          </div>
        </div>
        <DialogFooter>
          <Button variant="outline" onClick={() => onOpenChange(false)}>
            Cancelar
          </Button>
          <Button onClick={handleConfirm}>
            Aplicar a {weeksRemaining + 1} semanas
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}

// ============================================
// Deload Dialog
// ============================================

interface DeloadDialogProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  onConfirm: (reduction: number, targetWeek: number) => void;
  exerciseName: string;
  totalWeeks: number;
  currentWeek: number;
}

function DeloadDialog({
  open,
  onOpenChange,
  onConfirm,
  exerciseName,
  totalWeeks,
  currentWeek,
}: DeloadDialogProps) {
  const [reduction, setReduction] = useState<string>('40');
  const [targetWeek, setTargetWeek] = useState<string>(String(Math.min(currentWeek + 3, totalWeeks)));

  const handleConfirm = () => {
    const reductionVal = parseFloat(reduction);
    const weekVal = parseInt(targetWeek);
    if (!isNaN(reductionVal) && !isNaN(weekVal)) {
      onConfirm(reductionVal, weekVal);
      onOpenChange(false);
    }
  };

  const availableWeeks = Array.from(
    { length: totalWeeks - currentWeek + 1 },
    (_, i) => currentWeek + i
  );

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-[425px]">
        <DialogHeader>
          <DialogTitle>🎯 Insertar Semana de Descarga</DialogTitle>
          <DialogDescription>
            Reduce la intensidad en la semana seleccionada para {exerciseName}
          </DialogDescription>
        </DialogHeader>
        <div className="grid gap-4 py-4">
          <div className="grid grid-cols-4 items-center gap-4">
            <Label htmlFor="targetWeek" className="text-right">
              Semana
            </Label>
            <Select value={targetWeek} onValueChange={setTargetWeek}>
              <SelectTrigger className="col-span-3">
                <SelectValue />
              </SelectTrigger>
              <SelectContent>
                {availableWeeks.map((week) => (
                  <SelectItem key={week} value={String(week)}>
                    Semana {week}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
          </div>
          <div className="grid grid-cols-4 items-center gap-4">
            <Label htmlFor="reduction" className="text-right">
              Reducción %
            </Label>
            <Select value={reduction} onValueChange={setReduction}>
              <SelectTrigger className="col-span-3">
                <SelectValue />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="30">30% (descarga ligera)</SelectItem>
                <SelectItem value="40">40% (descarga moderada)</SelectItem>
                <SelectItem value="50">50% (descarga profunda)</SelectItem>
              </SelectContent>
            </Select>
          </div>
          <div className="bg-muted/50 p-3 rounded-lg text-sm">
            <p className="text-muted-foreground">
              El volumen/intensidad de la S{targetWeek} se reducirá un {reduction}% respecto a la semana anterior.
            </p>
          </div>
        </div>
        <DialogFooter>
          <Button variant="outline" onClick={() => onOpenChange(false)}>
            Cancelar
          </Button>
          <Button onClick={handleConfirm}>
            Insertar Deload
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}

// ============================================
// Main Component
// ============================================

export function QuickFillMenu({
  children,
  currentWeek,
  totalWeeks,
  currentValue,
  exerciseName,
  dayName,
  onAction,
  // eslint-disable-next-line @typescript-eslint/no-unused-vars
  hasValues = true,
}: QuickFillMenuProps) {
  const [incrementDialogOpen, setIncrementDialogOpen] = useState(false);
  const [decrementDialogOpen, setDecrementDialogOpen] = useState(false);
  const [deloadDialogOpen, setDeloadDialogOpen] = useState(false);

  // eslint-disable-next-line @typescript-eslint/no-unused-vars
  const isFirstWeek = currentWeek === 1;
  const isLastWeek = currentWeek === totalWeeks;
  const weeksRemaining = totalWeeks - currentWeek;

  const handleCopyToAll = () => {
    onAction('copy-to-all');
  };

  const handleCopyToRemaining = () => {
    onAction('copy-to-remaining');
  };

  const handleIncrement = (step: number) => {
    onAction('apply-increment', { percentageStep: step });
  };

  const handleDecrement = (step: number) => {
    onAction('apply-decrement', { percentageStep: Math.abs(step) });
  };

  const handleDeload = (reduction: number, targetWeek: number) => {
    onAction('insert-deload', { deloadReduction: reduction, weeksBeforeDeload: targetWeek - currentWeek });
  };

  const handleRepeatPattern = () => {
    onAction('repeat-pattern');
  };

  const handleClearRow = () => {
    onAction('clear-row');
  };

  return (
    <>
      <ContextMenu>
        {children}
        <ContextMenuContent className="w-64">
          <ContextMenuLabel className="text-xs text-muted-foreground">
            {exerciseName} • {dayName}
          </ContextMenuLabel>
          <ContextMenuSeparator />
          
          {/* Copy Actions */}
          <ContextMenuItem onClick={handleCopyToAll} disabled={!currentValue}>
            <Copy className="mr-2 h-4 w-4" />
            Copiar a todas las semanas
            <ContextMenuShortcut>Ctrl+Shift+C</ContextMenuShortcut>
          </ContextMenuItem>
          
          {!isLastWeek && (
            <ContextMenuItem onClick={handleCopyToRemaining} disabled={!currentValue}>
              <CopyPlus className="mr-2 h-4 w-4" />
              Copiar a semanas restantes
              <ContextMenuShortcut>Ctrl+D</ContextMenuShortcut>
            </ContextMenuItem>
          )}
          
          <ContextMenuSeparator />
          
          {/* Progression Actions */}
          <ContextMenuSub>
            <ContextMenuSubTrigger>
              <TrendingUp className="mr-2 h-4 w-4 text-emerald-600" />
              Aplicar progresión
            </ContextMenuSubTrigger>
            <ContextMenuSubContent className="w-52">
              <ContextMenuItem onClick={() => setIncrementDialogOpen(true)} disabled={isLastWeek}>
                <Percent className="mr-2 h-4 w-4" />
                Incrementar % por semana
              </ContextMenuItem>
              <ContextMenuItem onClick={() => setDecrementDialogOpen(true)} disabled={isLastWeek}>
                <TrendingDown className="mr-2 h-4 w-4" />
                Decrementar % por semana
              </ContextMenuItem>
              <ContextMenuSeparator />
              <ContextMenuItem onClick={() => onAction('apply-increment', { percentageStep: 2.5 })} disabled={isLastWeek}>
                <ArrowRight className="mr-2 h-4 w-4" />
                +2.5% por semana
              </ContextMenuItem>
              <ContextMenuItem onClick={() => onAction('apply-increment', { percentageStep: 5 })} disabled={isLastWeek}>
                <ArrowRight className="mr-2 h-4 w-4" />
                +5% por semana
              </ContextMenuItem>
            </ContextMenuSubContent>
          </ContextMenuSub>
          
          {/* Deload */}
          <ContextMenuItem onClick={() => setDeloadDialogOpen(true)} disabled={weeksRemaining < 1}>
            <Target className="mr-2 h-4 w-4 text-amber-600" />
            Insertar deload
          </ContextMenuItem>
          
          {/* Pattern */}
          {totalWeeks >= 8 && (
            <ContextMenuItem onClick={handleRepeatPattern} disabled={currentWeek > totalWeeks / 2}>
              <RefreshCw className="mr-2 h-4 w-4 text-blue-600" />
              Repetir patrón S1-S{Math.min(4, Math.floor(totalWeeks / 2))}
            </ContextMenuItem>
          )}
          
          <ContextMenuSeparator />
          
          {/* Clear */}
          <ContextMenuItem 
            onClick={handleClearRow}
            className="text-destructive focus:text-destructive"
          >
            Limpiar fila completa
          </ContextMenuItem>
        </ContextMenuContent>
      </ContextMenu>

      {/* Dialogs */}
      <IncrementDialog
        open={incrementDialogOpen}
        onOpenChange={setIncrementDialogOpen}
        isIncrement={true}
        onConfirm={handleIncrement}
        exerciseName={exerciseName}
        totalWeeks={totalWeeks}
        currentWeek={currentWeek}
      />
      
      <IncrementDialog
        open={decrementDialogOpen}
        onOpenChange={setDecrementDialogOpen}
        isIncrement={false}
        onConfirm={handleDecrement}
        exerciseName={exerciseName}
        totalWeeks={totalWeeks}
        currentWeek={currentWeek}
      />
      
      <DeloadDialog
        open={deloadDialogOpen}
        onOpenChange={setDeloadDialogOpen}
        onConfirm={handleDeload}
        exerciseName={exerciseName}
        totalWeeks={totalWeeks}
        currentWeek={currentWeek}
      />
    </>
  );
}

export { ContextMenuTrigger } from '@/components/ui/context-menu';
