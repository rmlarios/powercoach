'use client';

import { useState } from 'react';
import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogDescription, DialogFooter } from '@/components/ui/dialog';
import { Button } from '@/components/ui/button';
import { Label } from '@/components/ui/label';
import { Input } from '@/components/ui/input';
import { useBuilder } from '@/providers/builder-provider';

interface DeloadWeekDialogProps {
  sourceWeekId: string;
  isOpen: boolean;
  onOpenChange: (open: boolean) => void;
}

export function DeloadWeekDialog({ sourceWeekId, isOpen, onOpenChange }: DeloadWeekDialogProps) {
  const { state, dispatch } = useBuilder();
  const sourceWeek = state.weeks.find((w) => w.id === sourceWeekId);
  
  const [volumePercent, setVolumePercent] = useState<number>(50);

  const handleGenerate = () => {
    dispatch({
      type: 'GENERATE_DELOAD_WEEK',
      payload: {
        afterWeekId: sourceWeekId,
        volumePercent,
      },
    });

    onOpenChange(false);
  };

  if (!sourceWeek) return null;

  return (
    <Dialog open={isOpen} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-[425px]">
        <DialogHeader>
          <DialogTitle>Generate Deload Week</DialogTitle>
          <DialogDescription>
            Insert a new week after Week {sourceWeek.weekNumber} with reduced volume.
          </DialogDescription>
        </DialogHeader>

        <div className="grid gap-4 py-4">
          <div className="grid gap-2">
            <Label htmlFor="volume-percent" className="font-normal">
              Volume % (Percentage of current week's 1RMs)
            </Label>
            <div className="flex items-center gap-2">
              <Input
                id="volume-percent"
                type="number"
                value={volumePercent || ''}
                onChange={(e) => setVolumePercent(Number(e.target.value))}
                placeholder="e.g. 50"
                className="w-24"
                min="0"
                max="100"
                step="5"
              />
              <span className="text-sm text-muted-foreground">%</span>
            </div>
            <p className="text-xs text-muted-foreground mt-1">
              Example: If an exercise was 80%, using 50% here will set it to 40% for the deload week.
            </p>
          </div>
        </div>

        <DialogFooter>
          <Button variant="outline" onClick={() => onOpenChange(false)}>Cancel</Button>
          <Button onClick={handleGenerate}>Generate</Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}
