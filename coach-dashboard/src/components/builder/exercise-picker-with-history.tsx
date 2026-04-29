'use client';

import React, { useState, useCallback, useMemo } from 'react';
import { Search, TrendingUp, TrendingDown, Minus, Trophy, Dumbbell, Calendar, Info, Clock, Lightbulb } from 'lucide-react';
import { Input } from '@/components/ui/input';
import { Badge } from '@/components/ui/badge';
import { ScrollArea } from '@/components/ui/scroll-area';
import { Skeleton } from '@/components/ui/skeleton';
import { ExerciseOption } from '@/types/builder';
import { useExerciseHistory } from '@/hooks/use-athletes';
import { cn } from '@/utils/cn';

interface ExercisePickerWithHistoryProps {
  exerciseOptions: ExerciseOption[];
  athleteId?: string | null;
  onSelect: (exercise: ExerciseOption) => void;
}

export function ExercisePickerWithHistory({
  exerciseOptions,
  athleteId,
  onSelect,
}: ExercisePickerWithHistoryProps) {
  const [searchQuery, setSearchQuery] = useState('');
  const [hoveredExerciseId, setHoveredExerciseId] = useState<string | null>(null);
  const [selectedPreviewId, setSelectedPreviewId] = useState<string | null>(null);

  // Use the hovered or clicked exercise for history preview
  const previewExerciseId = selectedPreviewId || hoveredExerciseId;

  // Fetch history for preview exercise
  const { data: exerciseHistory, isLoading: historyLoading } = useExerciseHistory(
    athleteId || '',
    previewExerciseId || ''
  );

  // Filter exercises
  const filteredExercises = useMemo(() => {
    return exerciseOptions.filter(
      (ex) =>
        ex.name.toLowerCase().includes(searchQuery.toLowerCase()) ||
        ex.category?.toLowerCase().includes(searchQuery.toLowerCase()) ||
        ex.muscleGroup?.toLowerCase().includes(searchQuery.toLowerCase())
    );
  }, [exerciseOptions, searchQuery]);

  // Group exercises by category
  const groupedExercises = useMemo(() => {
    const groups: Record<string, ExerciseOption[]> = {};
    filteredExercises.forEach((ex) => {
      const category = ex.category || 'Other';
      if (!groups[category]) groups[category] = [];
      groups[category].push(ex);
    });
    return groups;
  }, [filteredExercises]);

  const handleExerciseClick = useCallback((exercise: ExerciseOption) => {
    setSelectedPreviewId(exercise.id);
  }, []);

  const handleExerciseDoubleClick = useCallback((exercise: ExerciseOption) => {
    onSelect(exercise);
  }, [onSelect]);

  const handleSelectClick = useCallback(() => {
    const exercise = exerciseOptions.find(e => e.id === previewExerciseId);
    if (exercise) {
      onSelect(exercise);
    }
  }, [exerciseOptions, previewExerciseId, onSelect]);

  // Get trend icon
  const getTrendIcon = (trend: string) => {
    switch (trend) {
      case 'improving':
        return <TrendingUp className="h-4 w-4 text-green-500" />;
      case 'declining':
        return <TrendingDown className="h-4 w-4 text-red-500" />;
      default:
        return <Minus className="h-4 w-4 text-muted-foreground" />;
    }
  };

  // Get preview exercise details
  const previewExercise = previewExerciseId 
    ? exerciseOptions.find(e => e.id === previewExerciseId) 
    : null;

  // Render history panel content
  const renderHistoryPanel = () => {
    if (!athleteId) {
      return (
        <div className="flex flex-col items-center justify-center h-full text-center px-4">
          <Info className="h-8 w-8 text-muted-foreground mb-2" />
          <p className="text-sm text-muted-foreground">
            Select an athlete to see exercise history
          </p>
        </div>
      );
    }

    if (!previewExerciseId) {
      return (
        <div className="flex flex-col items-center justify-center h-full text-center px-4">
          <Dumbbell className="h-8 w-8 text-muted-foreground mb-2" />
          <p className="text-sm text-muted-foreground">
            Hover or click an exercise to see history
          </p>
        </div>
      );
    }

    if (historyLoading) {
      return (
        <div className="space-y-4 p-4">
          <Skeleton className="h-6 w-3/4" />
          <Skeleton className="h-20 w-full" />
          <Skeleton className="h-20 w-full" />
          <Skeleton className="h-16 w-full" />
        </div>
      );
    }

    if (!exerciseHistory) {
      return (
        <div className="flex flex-col items-center justify-center h-full text-center px-4">
          <Calendar className="h-8 w-8 text-muted-foreground mb-2" />
          <p className="text-sm font-medium">{previewExercise?.name}</p>
          <Badge variant="outline" className="mt-2">
            No history available
          </Badge>
          <p className="text-xs text-muted-foreground mt-2">
            This exercise hasn&apos;t been logged yet for this athlete
          </p>
        </div>
      );
    }

    // Determine trend direction from trendKg
    const trendDirection = exerciseHistory.trendKg 
      ? (exerciseHistory.trendKg > 0 ? 'improving' : exerciseHistory.trendKg < 0 ? 'declining' : 'stable')
      : null;

    return (
      <div className="p-4 space-y-4">
        {/* Exercise Name */}
        <div>
          <h3 className="font-semibold text-lg">{exerciseHistory.exerciseName}</h3>
          {exerciseHistory.hasHistory && (
            <p className="text-xs text-muted-foreground">
              {exerciseHistory.recentLogs.length} recent logs
            </p>
          )}
        </div>

        {/* 1RM Card */}
        {exerciseHistory.current1RM && (
          <div className="bg-primary/5 rounded-lg p-3 border border-primary/20">
            <div className="flex items-center justify-between mb-1">
              <span className="text-sm font-medium flex items-center gap-1">
                <Trophy className="h-4 w-4 text-yellow-500" />
                Current 1RM
              </span>
              <span className="text-lg font-bold text-primary">
                {exerciseHistory.current1RM.weight} kg
              </span>
            </div>
            <p className="text-xs text-muted-foreground">
              {exerciseHistory.current1RM.isTested ? 'Tested' : 'Estimated'} • {new Date(exerciseHistory.current1RM.recordedAt).toLocaleDateString()}
            </p>
          </div>
        )}

        {/* PR & Trend */}
        {(exerciseHistory.personalRecord || trendDirection) && (
          <div className="grid grid-cols-2 gap-2">
            {/* PR */}
            {exerciseHistory.personalRecord && (
              <div className="bg-muted/50 rounded-lg p-3">
                <div className="text-xs text-muted-foreground mb-1">Personal Record</div>
                <div className="font-semibold">
                  {exerciseHistory.personalRecord.weight} kg
                </div>
                <div className="text-xs text-muted-foreground">
                  {new Date(exerciseHistory.personalRecord.recordedAt).toLocaleDateString()}
                </div>
              </div>
            )}

            {/* Trend */}
            {trendDirection && (
              <div className="bg-muted/50 rounded-lg p-3">
                <div className="text-xs text-muted-foreground mb-1">Trend</div>
                <div className="flex items-center gap-1 font-semibold capitalize">
                  {getTrendIcon(trendDirection)}
                  {trendDirection}
                </div>
                <div className="text-xs text-muted-foreground">
                  {exerciseHistory.trendKg !== undefined && exerciseHistory.trendKg > 0 ? '+' : ''}{exerciseHistory.trendKg?.toFixed(1)} kg
                </div>
              </div>
            )}
          </div>
        )}

        {/* Last Programmed */}
        {exerciseHistory.lastProgrammed && (
          <div className="bg-muted/30 rounded-lg p-3">
            <div className="flex items-center gap-1 text-sm font-medium mb-1">
              <Clock className="h-4 w-4" />
              Last Programmed
            </div>
            <div className="text-sm">
              {exerciseHistory.lastProgrammed.programName}
            </div>
            <div className="text-xs text-muted-foreground">
              Week {exerciseHistory.lastProgrammed.weekNumber}, Day {exerciseHistory.lastProgrammed.dayNumber}
            </div>
            <div className="text-sm font-medium mt-1">
              {exerciseHistory.lastProgrammed.prescription}
            </div>
            <p className="text-xs text-muted-foreground mt-1">
              {new Date(exerciseHistory.lastProgrammed.programDate).toLocaleDateString()}
            </p>
          </div>
        )}

        {/* Recent Logs */}
        {exerciseHistory.recentLogs && exerciseHistory.recentLogs.length > 0 && (
          <div>
            <h4 className="text-sm font-medium mb-2">Recent Performance</h4>
            <div className="space-y-1">
              {exerciseHistory.recentLogs.slice(0, 3).map((log, idx) => (
                <div key={idx} className="flex justify-between text-sm py-1 border-b border-muted/30 last:border-0">
                  <span className="text-muted-foreground">
                    {new Date(log.performedAt).toLocaleDateString()}
                  </span>
                  <span>
                    {log.weight} kg × {log.reps}
                    {log.rpe && ` @ RPE ${log.rpe}`}
                  </span>
                </div>
              ))}
            </div>
          </div>
        )}

        {/* Suggestion */}
        {exerciseHistory.suggestedStartWeight && (
          <div className="bg-blue-50 dark:bg-blue-950/30 rounded-lg p-3 border border-blue-200 dark:border-blue-800">
            <div className="flex items-start gap-2">
              <Lightbulb className="h-4 w-4 text-blue-600 dark:text-blue-400 mt-0.5 flex-shrink-0" />
              <div>
                <div className="text-sm font-medium text-blue-800 dark:text-blue-300">
                  Suggested Starting Weight
                </div>
                <div className="text-lg font-bold text-blue-600 dark:text-blue-400">
                  {exerciseHistory.suggestedStartWeight} kg
                </div>
                {exerciseHistory.suggestedStartPercentage && (
                  <div className="text-xs text-blue-600/80 dark:text-blue-400/80 mt-1">
                    {exerciseHistory.suggestedStartPercentage}% of 1RM
                  </div>
                )}
              </div>
            </div>
          </div>
        )}

        {/* Add Button */}
        <button
          onClick={handleSelectClick}
          className={cn(
            "w-full py-2 px-4 rounded-md",
            "bg-primary text-primary-foreground",
            "hover:bg-primary/90 transition-colors",
            "font-medium text-sm"
          )}
        >
          Add to Program
        </button>
      </div>
    );
  };

  return (
    <div className="flex h-[450px]">
      {/* Left Panel - Exercise List */}
      <div className="w-1/2 border-r flex flex-col">
        {/* Search */}
        <div className="p-3 border-b">
          <div className="relative">
            <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 h-4 w-4 text-muted-foreground" />
            <Input
              placeholder="Search exercises..."
              value={searchQuery}
              onChange={(e) => setSearchQuery(e.target.value)}
              className="pl-9"
              autoFocus
            />
          </div>
        </div>

        {/* Exercise List */}
        <ScrollArea className="flex-1">
          <div className="p-2 space-y-3">
            {Object.entries(groupedExercises).map(([category, exercises]) => (
              <div key={category}>
                <div className="px-2 py-1 text-xs font-semibold text-muted-foreground uppercase tracking-wide">
                  {category}
                </div>
                <div className="space-y-0.5">
                  {exercises.map((exercise) => {
                    const isSelected = selectedPreviewId === exercise.id;
                    const isHovered = hoveredExerciseId === exercise.id;
                    
                    return (
                      <button
                        key={exercise.id}
                        onClick={() => handleExerciseClick(exercise)}
                        onDoubleClick={() => handleExerciseDoubleClick(exercise)}
                        onMouseEnter={() => setHoveredExerciseId(exercise.id)}
                        onMouseLeave={() => setHoveredExerciseId(null)}
                        className={cn(
                          'w-full text-left px-3 py-2 rounded-md transition-colors',
                          'focus:outline-none',
                          isSelected
                            ? 'bg-primary/10 border border-primary/30'
                            : isHovered
                              ? 'bg-muted'
                              : 'hover:bg-muted/50'
                        )}
                      >
                        <div className="flex items-center justify-between">
                          <span className="font-medium text-sm">{exercise.name}</span>
                          {/* Show indicator if athlete has history */}
                          {athleteId && (
                            <ExerciseHistoryIndicator
                              athleteId={athleteId}
                              exerciseId={exercise.id}
                            />
                          )}
                        </div>
                        {exercise.muscleGroup && (
                          <div className="text-xs text-muted-foreground">
                            {exercise.muscleGroup}
                          </div>
                        )}
                      </button>
                    );
                  })}
                </div>
              </div>
            ))}

            {filteredExercises.length === 0 && (
              <div className="py-8 text-center text-sm text-muted-foreground">
                No exercises found
              </div>
            )}
          </div>
        </ScrollArea>
      </div>

      {/* Right Panel - History Preview */}
      <div className="w-1/2 bg-muted/20">
        <ScrollArea className="h-full">
          {renderHistoryPanel()}
        </ScrollArea>
      </div>
    </div>
  );
}

// Small indicator component for exercise history
function ExerciseHistoryIndicator({ 
  athleteId, 
  exerciseId 
}: { 
  athleteId: string; 
  exerciseId: string;
}) {
  const { data: history, isLoading } = useExerciseHistory(athleteId, exerciseId);

  if (isLoading) return null;

  if (!history || !history.hasHistory) {
    return (
      <span className="text-muted-foreground/50" title="No history for this athlete">
        <Dumbbell className="h-3 w-3" />
      </span>
    );
  }

  return (
    <Badge 
      variant="secondary" 
      className="h-5 px-1.5 text-xs"
      title={history.current1RM 
        ? `1RM: ${history.current1RM.weight} kg` 
        : `${history.recentLogs.length} logged sessions`
      }
    >
      {history.current1RM ? `1RM: ${history.current1RM.weight}` : `${history.recentLogs.length} logs`}
    </Badge>
  );
}
