'use client';

import { useState } from 'react';
import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogDescription, DialogFooter } from '@/components/ui/dialog';
import { Button } from '@/components/ui/button';
import { Switch } from '@/components/ui/switch';
import { Label } from '@/components/ui/label';
import { Input } from '@/components/ui/input';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select';
import { useBuilder } from '@/providers/builder-provider';

interface CopyWeekDialogProps {
  sourceWeekId: string;
  isOpen: boolean;
  onOpenChange: (open: boolean) => void;
}

export function CopyWeekDialog({ sourceWeekId, isOpen, onOpenChange }: CopyWeekDialogProps) {
  const { state, dispatch } = useBuilder();
  const sourceWeek = state.weeks.find((w) => w.id === sourceWeekId);
  
  const [targetWeekId, setTargetWeekId] = useState<string>('');
  const [includeExercises, setIncludeExercises] = useState(true);
  const [progressionPercent, setProgressionPercent] = useState<number>(0);

  // Available target weeks (all except the source week)
  const targetWeeks = state.weeks.filter((w) => w.id !== sourceWeekId);

  const handleCopy = () => {
    if (!targetWeekId) return;

    dispatch({
      type: 'COPY_WEEK_STRUCTURE',
      payload: {
        sourceWeekId,
        targetWeekIds: [targetWeekId], // Can be expanded to multiple later if needed
        includeExercises,
        progressionPercent: progressionPercent !== 0 ? progressionPercent : undefined,
      },
    });

    onOpenChange(false);
    
    // Reset state
    setTargetWeekId('');
    setIncludeExercises(true);
    setProgressionPercent(0);
  };

  if (!sourceWeek) return null;

  return (
    <Dialog open={isOpen} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-[425px]">
        <DialogHeader>
          <DialogTitle>Copy Week Structure</DialogTitle>
          <DialogDescription>
            Copy the structure from Week {sourceWeek.weekNumber} to another week. This will replace the target week's days and exercises.
          </DialogDescription>
        </DialogHeader>

        <div className="grid gap-4 py-4">
          <div className="grid gap-2">
            <Label htmlFor="target-week">Target Week</Label>
            <Select value={targetWeekId} onValueChange={setTargetWeekId}>
              <SelectTrigger id="target-week">
                <SelectValue placeholder="Select a week to overwrite" />
              </SelectTrigger>
              <SelectContent>
                {targetWeeks.map((week) => (
                  <SelectItem key={week.id} value={week.id}>
                    Week {week.weekNumber} {week.name ? `(${week.name})` : ''}
                  </SelectItem>
                ))}
            </SelectContent>
            </Select>
          </div>

          <div className="flex items-center space-x-2 mt-2">
            <Switch 
              id="include-exercises" 
              checked={includeExercises} 
              onCheckedChange={(checked: boolean) => setIncludeExercises(checked)} 
            />
            <Label htmlFor="include-exercises" className="font-normal cursor-pointer">
              Include Exercises
            </Label>
          </div>

          {includeExercises && (
            <div className="grid gap-2 mt-2 pl-6">
              <Label htmlFor="progression-percent" className="text-muted-foreground text-sm">
                Add Progression (Optional % to add to 1RM percentages)
              </Label>
              <div className="flex items-center gap-2">
                <Input
                  id="progression-percent"
                  type="number"
                  value={progressionPercent || ''}
                  onChange={(e) => setProgressionPercent(Number(e.target.value))}
                  placeholder="e.g. 2.5"
                  className="w-24"
                  step="0.5"
                />
                <span className="text-sm text-muted-foreground">%</span>
              </div>
            </div>
          )}
        </div>

        <DialogFooter>
          <Button variant="outline" onClick={() => onOpenChange(false)}>Cancel</Button>
          <Button onClick={handleCopy} disabled={!targetWeekId}>Copy Structure</Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}
