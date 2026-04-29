'use client';

import React, { useState, useCallback, useMemo, useEffect } from 'react';
import {
  TrendingUp,
  Sparkles,
  Dumbbell,
  Calculator,
  Zap,
  ChevronRight,
  Check,
  Activity,
} from 'lucide-react';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Switch } from '@/components/ui/switch';
import {
  Sheet,
  SheetContent,
  SheetDescription,
  SheetHeader,
  SheetTitle,
  SheetTrigger,
  SheetFooter,
} from '@/components/ui/sheet';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select';
import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/components/ui/tabs';
import { Badge } from '@/components/ui/badge';
import { Slider } from '@/components/ui/slider';
import {
  ProgressionType,
  ProgressionTemplate,
  ProgressionGeneratorSettings,
  GeneratedWeekData,
  PROGRESSION_TEMPLATES,
  PROGRESSION_TYPES,
} from '@/types/builder';
import { cn } from '@/utils/cn';

interface ProgressionGeneratorProps {
  exerciseId: string;
  exerciseName: string;
  totalWeeks: number;
  athleteRM?: number; // 1RM from athlete profile
  onGenerate: (weeks: GeneratedWeekData[]) => void;
}

// Progression calculation functions
function generateLinearProgression(
  totalWeeks: number,
  startPercent: number,
  endPercent: number
): number[] {
  if (totalWeeks <= 1) return [startPercent];
  const increment = (endPercent - startPercent) / (totalWeeks - 1);
  return Array.from({ length: totalWeeks }, (_, i) =>
    Math.round(startPercent + increment * i)
  );
}

function generateWaveProgression(
  totalWeeks: number,
  startPercent: number,
  endPercent: number
): number[] {
  const result: number[] = [];
  const amplitude = (endPercent - startPercent) / 4;
  const trend = (endPercent - startPercent) / totalWeeks;

  for (let i = 0; i < totalWeeks; i++) {
    const base = startPercent + trend * i;
    const wave = i % 2 === 0 ? amplitude : -amplitude * 0.5;
    result.push(Math.round(Math.min(endPercent, Math.max(startPercent, base + wave))));
  }
  return result;
}

function generateStepProgression(
  totalWeeks: number,
  startPercent: number,
  endPercent: number
): number[] {
  const stepsCount = Math.ceil(totalWeeks / 2);
  const stepSize = (endPercent - startPercent) / Math.max(stepsCount - 1, 1);

  return Array.from({ length: totalWeeks }, (_, i) => {
    const step = Math.floor(i / 2);
    return Math.round(startPercent + stepSize * step);
  });
}

function generatePeakProgression(
  totalWeeks: number,
  startPercent: number,
  endPercent: number
): { percentage: number; isDeload: boolean }[] {
  const result: { percentage: number; isDeload: boolean }[] = [];
  const peakWeek = totalWeeks - 1;
  const buildupWeeks = peakWeek;
  const increment = (endPercent - startPercent) / Math.max(buildupWeeks - 1, 1);

  for (let i = 0; i < totalWeeks; i++) {
    if (i < buildupWeeks) {
      result.push({
        percentage: Math.round(startPercent + increment * i),
        isDeload: false,
      });
    } else {
      // Deload week
      result.push({
        percentage: Math.round(startPercent * 0.85),
        isDeload: true,
      });
    }
  }
  return result;
}

function generateRPEProgression(
  totalWeeks: number,
  startRPE: number,
  endRPE: number,
  progressionType: ProgressionType
): number[] {
  if (progressionType === 'peak') {
    // Special handling for peak: RPE builds then drops for deload
    const result: number[] = [];
    const peakWeek = totalWeeks - 1;
    const increment = (endRPE - startRPE) / Math.max(peakWeek - 1, 1);
    for (let i = 0; i < totalWeeks; i++) {
      if (i < peakWeek) {
        result.push(Math.round((startRPE + increment * i) * 10) / 10);
      } else {
        result.push(Math.round(startRPE * 0.8 * 10) / 10);
      }
    }
    return result;
  }
  // Linear RPE progression for other types
  const increment = (endRPE - startRPE) / Math.max(totalWeeks - 1, 1);
  return Array.from({ length: totalWeeks }, (_, i) =>
    Math.round((startRPE + increment * i) * 10) / 10
  );
}

export function ProgressionGenerator({
  exerciseId,
  exerciseName,
  totalWeeks,
  athleteRM,
  onGenerate,
}: ProgressionGeneratorProps) {
  const [isOpen, setIsOpen] = useState(false);
  const [activeTab, setActiveTab] = useState<'templates' | 'custom'>('templates');
  const [selectedTemplate, setSelectedTemplate] = useState<ProgressionTemplate | null>(null);
  const [isGenerating, setIsGenerating] = useState(false);
  const [showPreview, setShowPreview] = useState(false);

  // Settings state
  const [settings, setSettings] = useState<ProgressionGeneratorSettings>({
    exerciseId,
    exerciseName,
    totalWeeks,
    sets: 4,
    repsMin: 8,
    repsMax: 12,
    startPercentage: 60,
    endPercentage: 75,
    startRPE: 6,
    endRPE: 8,
    restSeconds: 120,
    progressionType: 'linear',
    useAthleteRM: !!athleteRM,
    athleteRM,
  });

  // Update when props change
  useEffect(() => {
    setSettings((prev) => ({
      ...prev,
      exerciseId,
      exerciseName,
      totalWeeks,
      athleteRM,
      useAthleteRM: prev.useAthleteRM && !!athleteRM,
    }));
  }, [exerciseId, exerciseName, totalWeeks, athleteRM]);

  // Apply template
  const applyTemplate = useCallback((template: ProgressionTemplate) => {
    setSelectedTemplate(template);
    const templateData = PROGRESSION_TEMPLATES[template];
    if (templateData.defaults) {
      setSettings((prev) => ({
        ...prev,
        ...templateData.defaults,
      }));
    }
    setActiveTab('custom');
    setShowPreview(true);
  }, []);

  // Generate preview data
  const previewData = useMemo<GeneratedWeekData[]>(() => {
    const {
      totalWeeks: weeks,
      sets,
      repsMin,
      repsMax,
      startPercentage,
      endPercentage,
      startRPE,
      endRPE,
      restSeconds,
      progressionType,
      useAthleteRM,
      athleteRM: rm,
    } = settings;

    let percentages: number[];
    let isDeloadWeeks: boolean[] = Array(weeks).fill(false);

    switch (progressionType) {
      case 'wave':
        percentages = generateWaveProgression(weeks, startPercentage, endPercentage);
        break;
      case 'step':
        percentages = generateStepProgression(weeks, startPercentage, endPercentage);
        break;
      case 'peak': {
        const peakData = generatePeakProgression(weeks, startPercentage, endPercentage);
        percentages = peakData.map((d) => d.percentage);
        isDeloadWeeks = peakData.map((d) => d.isDeload);
        break;
      }
      default:
        percentages = generateLinearProgression(weeks, startPercentage, endPercentage);
    }

    const rpeValues = generateRPEProgression(weeks, startRPE, endRPE, progressionType);

    return percentages.map((percentageRM, i) => ({
      weekNumber: i + 1,
      sets: isDeloadWeeks[i] ? Math.max(2, Math.floor(sets * 0.5)) : sets,
      repsMin: isDeloadWeeks[i] ? repsMin : repsMin,
      repsMax: isDeloadWeeks[i] ? repsMax : repsMax,
      percentageRM,
      rpeTarget: rpeValues[i],
      restSeconds: isDeloadWeeks[i] ? Math.floor(restSeconds * 0.75) : restSeconds,
      weight: useAthleteRM && rm ? Math.round((percentageRM / 100) * rm * 2) / 2 : undefined,
      isDeload: isDeloadWeeks[i],
    }));
  }, [settings]);

  // Handle generate
  const handleGenerate = useCallback(async () => {
    setIsGenerating(true);
    // Simulate a small delay for animation
    await new Promise((resolve) => setTimeout(resolve, 300));
    onGenerate(previewData);
    setIsGenerating(false);
    setIsOpen(false);
  }, [previewData, onGenerate]);

  const updateSetting = useCallback(
    <K extends keyof ProgressionGeneratorSettings>(key: K, value: ProgressionGeneratorSettings[K]) => {
      setSettings((prev) => ({ ...prev, [key]: value }));
      setShowPreview(true);
    },
    []
  );

  return (
    <Sheet open={isOpen} onOpenChange={setIsOpen}>
      <SheetTrigger asChild>
        <Button variant="default" size="sm" className="gap-2">
          <Sparkles className="h-4 w-4" />
          Generate Progression
        </Button>
      </SheetTrigger>
      <SheetContent className="w-full sm:max-w-xl overflow-y-auto">
        <SheetHeader>
          <SheetTitle className="flex items-center gap-2">
            <Activity className="h-5 w-5 text-primary" />
            Progression Generator
          </SheetTitle>
          <SheetDescription>
            Generate a complete progression for <strong>{exerciseName}</strong> across{' '}
            {totalWeeks} weeks
          </SheetDescription>
        </SheetHeader>

        <div className="mt-6 space-y-6">
          <Tabs value={activeTab} onValueChange={(v: string) => setActiveTab(v as typeof activeTab)}>
            <TabsList className="grid w-full grid-cols-2">
              <TabsTrigger value="templates" className="gap-2">
                <Zap className="h-4 w-4" />
                Templates
              </TabsTrigger>
              <TabsTrigger value="custom" className="gap-2">
                <Calculator className="h-4 w-4" />
                Custom
              </TabsTrigger>
            </TabsList>

            {/* Templates Tab */}
            <TabsContent value="templates" className="space-y-4 mt-4">
              <div className="grid gap-3">
                {(Object.entries(PROGRESSION_TEMPLATES) as [ProgressionTemplate, typeof PROGRESSION_TEMPLATES[ProgressionTemplate]][])
                  .filter(([key]) => key !== 'custom')
                  .map(([key, template]) => (
                    <button
                      key={key}
                      onClick={() => applyTemplate(key)}
                      className={cn(
                        'flex items-start gap-4 p-4 rounded-lg border text-left transition-all',
                        'hover:border-primary/50 hover:bg-muted/50',
                        selectedTemplate === key && 'border-primary bg-primary/5'
                      )}
                    >
                      <div
                        className={cn(
                          'w-10 h-10 rounded-lg flex items-center justify-center flex-shrink-0',
                          selectedTemplate === key ? 'bg-primary text-primary-foreground' : 'bg-muted'
                        )}
                      >
                        {key === 'hypertrophy' && <Dumbbell className="h-5 w-5" />}
                        {key === 'strength' && <TrendingUp className="h-5 w-5" />}
                        {key === 'peaking' && <Activity className="h-5 w-5" />}
                        {key === 'deload' && <Zap className="h-5 w-5" />}
                      </div>
                      <div className="flex-1 min-w-0">
                        <div className="flex items-center justify-between">
                          <h4 className="font-medium">{template.label}</h4>
                          {selectedTemplate === key && (
                            <Check className="h-4 w-4 text-primary" />
                          )}
                        </div>
                        <p className="text-sm text-muted-foreground mt-0.5">
                          {template.description}
                        </p>
                        <div className="flex gap-2 mt-2 flex-wrap">
                          <Badge variant="secondary" className="text-xs">
                            {template.defaults.sets} sets
                          </Badge>
                          <Badge variant="secondary" className="text-xs">
                            {template.defaults.repsMin}-{template.defaults.repsMax} reps
                          </Badge>
                          <Badge variant="secondary" className="text-xs">
                            {template.defaults.startPercentage}-{template.defaults.endPercentage}%
                          </Badge>
                          <Badge variant="outline" className="text-xs capitalize">
                            {template.defaults.progressionType}
                          </Badge>
                        </div>
                      </div>
                      <ChevronRight className="h-5 w-5 text-muted-foreground flex-shrink-0" />
                    </button>
                  ))}
              </div>
            </TabsContent>

            {/* Custom Tab */}
            <TabsContent value="custom" className="space-y-6 mt-4">
              {/* Progression Type */}
              <div className="space-y-3">
                <Label>Progression Type</Label>
                <div className="grid grid-cols-2 gap-2">
                  {(Object.entries(PROGRESSION_TYPES) as [ProgressionType, typeof PROGRESSION_TYPES[ProgressionType]][]).map(
                    ([type, info]) => (
                      <button
                        key={type}
                        onClick={() => updateSetting('progressionType', type)}
                        className={cn(
                          'p-3 rounded-lg border text-left transition-all',
                          'hover:border-primary/50',
                          settings.progressionType === type && 'border-primary bg-primary/5'
                        )}
                      >
                        <h5 className="font-medium text-sm">{info.label}</h5>
                        <p className="text-xs text-muted-foreground mt-0.5">{info.description}</p>
                      </button>
                    )
                  )}
                </div>
              </div>

              {/* Volume Settings */}
              <div className="grid grid-cols-3 gap-4">
                <div className="space-y-2">
                  <Label htmlFor="sets">Sets</Label>
                  <Input
                    id="sets"
                    type="number"
                    value={settings.sets}
                    onChange={(e) => updateSetting('sets', Number(e.target.value))}
                    min={1}
                    max={10}
                  />
                </div>
                <div className="space-y-2">
                  <Label htmlFor="repsMin">Min Reps</Label>
                  <Input
                    id="repsMin"
                    type="number"
                    value={settings.repsMin}
                    onChange={(e) => updateSetting('repsMin', Number(e.target.value))}
                    min={1}
                    max={30}
                  />
                </div>
                <div className="space-y-2">
                  <Label htmlFor="repsMax">Max Reps</Label>
                  <Input
                    id="repsMax"
                    type="number"
                    value={settings.repsMax}
                    onChange={(e) => updateSetting('repsMax', Number(e.target.value))}
                    min={1}
                    max={30}
                  />
                </div>
              </div>

              {/* Intensity Settings */}
              <div className="space-y-4">
                <Label>Intensity (%RM)</Label>
                <div className="grid grid-cols-2 gap-4">
                  <div className="space-y-2">
                    <div className="flex justify-between text-sm">
                      <span className="text-muted-foreground">Start</span>
                      <span className="font-medium">{settings.startPercentage}%</span>
                    </div>
                    <Slider
                      value={[settings.startPercentage]}
                      onValueChange={([v]: number[]) => updateSetting('startPercentage', v)}
                      min={30}
                      max={100}
                      step={1}
                    />
                  </div>
                  <div className="space-y-2">
                    <div className="flex justify-between text-sm">
                      <span className="text-muted-foreground">End</span>
                      <span className="font-medium">{settings.endPercentage}%</span>
                    </div>
                    <Slider
                      value={[settings.endPercentage]}
                      onValueChange={([v]: number[]) => updateSetting('endPercentage', v)}
                      min={30}
                      max={100}
                      step={1}
                    />
                  </div>
                </div>
              </div>

              {/* RPE Settings */}
              <div className="space-y-4">
                <Label>RPE Range</Label>
                <div className="grid grid-cols-2 gap-4">
                  <div className="space-y-2">
                    <div className="flex justify-between text-sm">
                      <span className="text-muted-foreground">Start</span>
                      <span className="font-medium">{settings.startRPE}</span>
                    </div>
                    <Slider
                      value={[settings.startRPE]}
                      onValueChange={([v]: number[]) => updateSetting('startRPE', v)}
                      min={5}
                      max={10}
                      step={0.5}
                    />
                  </div>
                  <div className="space-y-2">
                    <div className="flex justify-between text-sm">
                      <span className="text-muted-foreground">End</span>
                      <span className="font-medium">{settings.endRPE}</span>
                    </div>
                    <Slider
                      value={[settings.endRPE]}
                      onValueChange={([v]: number[]) => updateSetting('endRPE', v)}
                      min={5}
                      max={10}
                      step={0.5}
                    />
                  </div>
                </div>
              </div>

              {/* Rest Time */}
              <div className="space-y-2">
                <Label htmlFor="rest">Rest Time (seconds)</Label>
                <Select
                  value={String(settings.restSeconds)}
                  onValueChange={(v) => updateSetting('restSeconds', Number(v))}
                >
                  <SelectTrigger>
                    <SelectValue />
                  </SelectTrigger>
                  <SelectContent>
                    <SelectItem value="60">60s (1 min)</SelectItem>
                    <SelectItem value="90">90s (1.5 min)</SelectItem>
                    <SelectItem value="120">120s (2 min)</SelectItem>
                    <SelectItem value="150">150s (2.5 min)</SelectItem>
                    <SelectItem value="180">180s (3 min)</SelectItem>
                    <SelectItem value="240">240s (4 min)</SelectItem>
                    <SelectItem value="300">300s (5 min)</SelectItem>
                  </SelectContent>
                </Select>
              </div>

              {/* 1RM Integration */}
              {athleteRM && (
                <div className="flex items-center justify-between p-4 rounded-lg bg-muted/50 border">
                  <div className="space-y-0.5">
                    <Label htmlFor="use1rm" className="text-base cursor-pointer">
                      Use Athlete 1RM
                    </Label>
                    <p className="text-sm text-muted-foreground">
                      Auto-calculate weight from {athleteRM}kg 1RM
                    </p>
                  </div>
                  <Switch
                    id="use1rm"
                    checked={settings.useAthleteRM}
                    onCheckedChange={(v: boolean) => updateSetting('useAthleteRM', v)}
                  />
                </div>
              )}
            </TabsContent>
          </Tabs>

          {/* Preview Section */}
          {showPreview && (
            <div className="space-y-3">
              <div className="flex items-center justify-between">
                <Label className="text-base">Preview</Label>
                <Badge variant="outline">{previewData.length} weeks</Badge>
              </div>
              <div className="rounded-lg border overflow-hidden">
                <table className="w-full text-sm">
                  <thead className="bg-muted/50">
                    <tr>
                      <th className="px-3 py-2 text-left font-medium">Wk</th>
                      <th className="px-3 py-2 text-center font-medium">Sets</th>
                      <th className="px-3 py-2 text-center font-medium">Reps</th>
                      <th className="px-3 py-2 text-center font-medium">%RM</th>
                      <th className="px-3 py-2 text-center font-medium">RPE</th>
                      {settings.useAthleteRM && athleteRM && (
                        <th className="px-3 py-2 text-center font-medium">Wt</th>
                      )}
                    </tr>
                  </thead>
                  <tbody className="divide-y">
                    {previewData.map((week) => (
                      <tr
                        key={week.weekNumber}
                        className={cn(
                          'transition-colors',
                          week.isDeload && 'bg-amber-50 dark:bg-amber-950/20'
                        )}
                      >
                        <td className="px-3 py-2 font-medium">
                          {week.weekNumber}
                          {week.isDeload && (
                            <span className="ml-1 text-xs text-amber-600">(D)</span>
                          )}
                        </td>
                        <td className="px-3 py-2 text-center">{week.sets}</td>
                        <td className="px-3 py-2 text-center">
                          {week.repsMin}-{week.repsMax}
                        </td>
                        <td className="px-3 py-2 text-center">{week.percentageRM}%</td>
                        <td className="px-3 py-2 text-center">{week.rpeTarget}</td>
                        {settings.useAthleteRM && athleteRM && (
                          <td className="px-3 py-2 text-center font-medium text-primary">
                            {week.weight}kg
                          </td>
                        )}
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            </div>
          )}
        </div>

        <SheetFooter className="mt-6 pt-6 border-t">
          <Button variant="outline" onClick={() => setIsOpen(false)}>
            Cancel
          </Button>
          <Button
            onClick={handleGenerate}
            disabled={isGenerating}
            className="gap-2 min-w-[140px]"
          >
            {isGenerating ? (
              <>
                <div className="h-4 w-4 border-2 border-current border-t-transparent rounded-full animate-spin" />
                Generating...
              </>
            ) : (
              <>
                <Sparkles className="h-4 w-4" />
                Apply to Program
              </>
            )}
          </Button>
        </SheetFooter>
      </SheetContent>
    </Sheet>
  );
}
