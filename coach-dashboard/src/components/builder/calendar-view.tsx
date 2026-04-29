'use client';

import React, { useState, useMemo, useCallback } from 'react';
import {
  ChevronLeft,
  ChevronRight,
  Dumbbell,
  Calendar as CalendarIcon,
  Coffee,
  Trophy,
  Info,
  GripVertical,
} from 'lucide-react';
import { Button } from '@/components/ui/button';
import { Badge } from '@/components/ui/badge';
import {
  Popover,
  PopoverContent,
  PopoverTrigger,
} from '@/components/ui/popover';
import {
  DndContext,
  DragEndEvent,
  DragOverlay,
  DragStartEvent,
  PointerSensor,
  useSensor,
  useSensors,
  useDraggable,
  useDroppable,
} from '@dnd-kit/core';
import { BuilderWeek } from '@/types/builder';
import { cn } from '@/utils/cn';
import {
  format,
  addDays,
  addMonths,
  subMonths,
  isSameMonth,
  isSameDay,
  isToday,
  differenceInDays,
} from 'date-fns';
import {
  CalendarEvent,
  getCalendarDays,
  mapProgramToCalendar,
} from './calendar-utils';

interface CalendarViewProps {
  weeks: BuilderWeek[];
  startDate: Date;
  onDateChange?: (date: Date) => void;
  onDayClick?: (event: CalendarEvent) => void;
  onDayMove?: (
    sourceWeekId: string,
    sourceDayId: string, 
    targetWeekNumber: number,
    targetDayOffset: number
  ) => void;
  competitions?: Array<{ date: string; name: string }>;
}

// Shared event color/icon helpers
function getEventColor(type: CalendarEvent['type']) {
  switch (type) {
    case 'training':
      return 'bg-primary/10 border-primary/30 hover:bg-primary/20';
    case 'deload':
      return 'bg-amber-100 dark:bg-amber-950 border-amber-300 dark:border-amber-700 hover:bg-amber-200 dark:hover:bg-amber-900';
    case 'competition':
      return 'bg-red-100 dark:bg-red-950 border-red-300 dark:border-red-700 hover:bg-red-200 dark:hover:bg-red-900';
    case 'rest':
      return 'bg-muted/30 border-muted';
    default:
      return 'bg-background';
  }
}

function getEventIcon(type: CalendarEvent['type']) {
  switch (type) {
    case 'training':
      return <Dumbbell className="h-3 w-3" />;
    case 'deload':
      return <Coffee className="h-3 w-3" />;
    case 'competition':
      return <Trophy className="h-3 w-3" />;
    default:
      return null;
  }
}

// Event content (shared between cell and drag overlay)
function EventContent({ event, compact = false }: { event: CalendarEvent; compact?: boolean }) {
  return (
    <div className={cn('space-y-1', compact && 'p-2')}>
      <div className="flex items-center gap-1">
        {getEventIcon(event.type)}
        <span className="text-xs font-medium truncate">
          {event.type === 'competition' ? event.dayName : `W${event.weekNumber}D${event.dayNumber}`}
        </span>
      </div>
      {(event.dayName || event.focus) && event.type !== 'competition' && (
        <div className="text-xs text-muted-foreground truncate">
          {event.focus || event.dayName}
        </div>
      )}
      {event.exercises.length > 0 && (
        <Badge variant="secondary" className="text-[10px] h-4 px-1">
          {event.exercises.length} exercises
        </Badge>
      )}
    </div>
  );
}

// Day cell component with drag & drop support
function DayCell({
  date,
  event,
  isCurrentMonth,
  isInProgramRange,
  isDragEnabled,
  isDraggedOver,
  onDayClick,
}: {
  date: Date;
  event?: CalendarEvent;
  isCurrentMonth: boolean;
  isInProgramRange: boolean;
  isDragEnabled: boolean;
  isDraggedOver: boolean;
  onDayClick?: (event: CalendarEvent) => void;
}) {
  const dateKey = format(date, 'yyyy-MM-dd');
  const today = isToday(date);
  
  // Droppable: any cell within program range
  const { setNodeRef: setDropRef, isOver } = useDroppable({
    id: `drop-${dateKey}`,
    data: { date, dateKey },
    disabled: !isInProgramRange,
  });
  
  // Draggable: only training/deload events (not competitions)
  const isDraggable = isDragEnabled && event && event.type !== 'competition' && event.dayId !== '';
  const {
    attributes,
    listeners,
    setNodeRef: setDragRef,
    isDragging,
  } = useDraggable({
    id: `drag-${dateKey}`,
    data: { event, dateKey },
    disabled: !isDraggable,
  });
  
  const showDropHighlight = (isOver || isDraggedOver) && isInProgramRange;
  
  return (
    <Popover>
      <PopoverTrigger asChild>
        <div
          ref={setDropRef}
          className={cn(
            'relative h-24 md:h-28 border rounded-lg transition-all',
            !isCurrentMonth && 'opacity-40',
            today && 'ring-2 ring-primary',
            showDropHighlight && 'ring-2 ring-blue-400 bg-blue-50 dark:bg-blue-950/40',
            isDragging && 'opacity-30',
            !event && !showDropHighlight && 'bg-muted/10 border-border/50',
            event && !isDragging ? getEventColor(event.type) : '',
          )}
        >
          <button
            ref={setDragRef}
            className={cn(
              'w-full h-full p-1 text-left',
              'focus:outline-none focus:ring-2 focus:ring-primary/30 rounded-lg',
              isDraggable && 'cursor-grab active:cursor-grabbing',
            )}
            onClick={() => event && onDayClick?.(event)}
            disabled={!event}
            {...(isDraggable ? { ...attributes, ...listeners } : {})}
          >
            {/* Day number + drag hint */}
            <div className="flex items-center justify-between">
              <div className={cn(
                'text-sm font-medium',
                today && 'text-primary',
                !isCurrentMonth && 'text-muted-foreground'
              )}>
                {format(date, 'd')}
              </div>
              {isDraggable && (
                <GripVertical className="h-3 w-3 text-muted-foreground/50" />
              )}
            </div>
            
            {/* Event content */}
            {event && !isDragging && (
              <div className="mt-1">
                <EventContent event={event} />
              </div>
            )}
            
            {/* Drop hint */}
            {showDropHighlight && !event && (
              <div className="mt-2 text-[10px] text-blue-500 text-center">
                Drop here
              </div>
            )}
          </button>
        </div>
      </PopoverTrigger>
      
      {/* Popover with details */}
      {event && !isDragging && (
        <PopoverContent className="w-72" align="start">
          <DayDetails event={event} />
        </PopoverContent>
      )}
    </Popover>
  );
}

// Day details popover content
function DayDetails({ event }: { event: CalendarEvent }) {
  return (
    <div className="space-y-3">
      {/* Header */}
      <div>
        <div className="flex items-center gap-2">
          <span className="font-semibold">
            {event.type === 'competition' ? event.dayName : event.dayName}
          </span>
          {event.type === 'deload' && (
            <Badge variant="outline" className="text-amber-600">Deload</Badge>
          )}
          {event.type === 'competition' && (
            <Badge variant="outline" className="text-red-600">Competition</Badge>
          )}
        </div>
        <div className="text-sm text-muted-foreground">
          {format(event.date, 'EEEE, MMMM d, yyyy')}
        </div>
        {event.type !== 'competition' && (
          <div className="text-xs text-muted-foreground">
            Week {event.weekNumber}, Day {event.dayNumber}
          </div>
        )}
      </div>
      
      {/* Focus */}
      {event.focus && (
        <div className="flex items-center gap-2 text-sm">
          <span className="text-muted-foreground">Focus:</span>
          <Badge>{event.focus}</Badge>
        </div>
      )}
      
      {/* Exercises */}
      {event.exercises.length > 0 && (
        <div className="space-y-1">
          <div className="text-sm font-medium">Exercises</div>
          <div className="space-y-1 max-h-40 overflow-y-auto">
            {event.exercises.map((ex, idx) => (
              <div key={idx} className="flex items-center justify-between text-sm py-1 border-b last:border-0">
                <span className="truncate flex-1">{ex.exerciseName}</span>
                <span className="text-muted-foreground text-xs ml-2">
                  {ex.sets}×{ex.repsMin}{ex.repsMax && ex.repsMax !== ex.repsMin ? `-${ex.repsMax}` : ''}
                </span>
              </div>
            ))}
          </div>
        </div>
      )}
      
      {/* Notes */}
      {event.notes && (
        <div className="text-sm text-muted-foreground border-t pt-2">
          {event.notes}
        </div>
      )}
    </div>
  );
}

// Main Calendar View Component
export function CalendarView({
  weeks,
  startDate,
  // eslint-disable-next-line @typescript-eslint/no-unused-vars
  onDateChange: _onDateChange,
  onDayClick,
  onDayMove,
  competitions = [],
}: CalendarViewProps) {
  const [currentMonth, setCurrentMonth] = useState(() => startDate || new Date());
  const [activeDragEvent, setActiveDragEvent] = useState<CalendarEvent | null>(null);
  
  // DnD sensors - require 8px movement to activate (avoid conflicts with click)
  const sensors = useSensors(
    useSensor(PointerSensor, {
      activationConstraint: { distance: 8 },
    })
  );
  
  // Map program to calendar events
  const eventMap = useMemo(
    () => mapProgramToCalendar(weeks, startDate, competitions),
    [weeks, startDate, competitions]
  );
  
  // Get calendar grid days
  const calendarDays = useMemo(
    () => getCalendarDays(currentMonth),
    [currentMonth]
  );
  
  // Calculate program date range
  const programEndDate = useMemo(() => {
    if (weeks.length === 0) return startDate;
    const lastWeek = Math.max(...weeks.map(w => w.weekNumber));
    return addDays(startDate, lastWeek * 7 - 1);
  }, [weeks, startDate]);
  
  // Check if date falls within program range
  const isDateInRange = useCallback((date: Date) => {
    const diff = differenceInDays(date, startDate);
    return diff >= 0 && diff <= differenceInDays(programEndDate, startDate);
  }, [startDate, programEndDate]);
  
  // Drag handlers
  const handleDragStart = useCallback((event: DragStartEvent) => {
    const dragData = event.active.data.current;
    if (dragData?.event) {
      setActiveDragEvent(dragData.event as CalendarEvent);
    }
  }, []);
  
  const handleDragEnd = useCallback((event: DragEndEvent) => {
    const { active, over } = event;
    setActiveDragEvent(null);
    
    if (!over || !onDayMove) return;
    
    const sourceData = active.data.current;
    const targetData = over.data.current;
    if (!sourceData?.event || !targetData?.date) return;
    
    const sourceEvent = sourceData.event as CalendarEvent;
    const targetDate = targetData.date as Date;
    
    // Don't move to the same date
    if (isSameDay(sourceEvent.date, targetDate)) return;
    
    // Calculate target week number and day offset within that week
    const daysSinceStart = differenceInDays(targetDate, startDate);
    const targetWeekNumber = Math.floor(daysSinceStart / 7) + 1;
    const targetDayOffset = daysSinceStart % 7; // 0=Mon, 1=Tue, ... 6=Sun
    
    onDayMove(sourceEvent.weekId, sourceEvent.dayId, targetWeekNumber, targetDayOffset);
  }, [onDayMove, startDate]);
  
  // Navigation handlers
  const goToPreviousMonth = () => setCurrentMonth(subMonths(currentMonth, 1));
  const goToNextMonth = () => setCurrentMonth(addMonths(currentMonth, 1));
  const goToToday = () => setCurrentMonth(new Date());
  const goToStart = () => setCurrentMonth(startDate);
  
  // Week day headers
  const weekDays = ['Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat', 'Sun'];
  
  const isDragEnabled = !!onDayMove;
  
  return (
    <DndContext
      sensors={sensors}
      onDragStart={handleDragStart}
      onDragEnd={handleDragEnd}
    >
      <div className="space-y-4">
        {/* Calendar Header */}
        <div className="flex items-center justify-between">
          <div className="flex items-center gap-4">
            <h2 className="text-xl font-semibold">
              {format(currentMonth, 'MMMM yyyy')}
            </h2>
            
            {/* Navigation */}
            <div className="flex items-center gap-1">
              <Button variant="outline" size="icon" onClick={goToPreviousMonth}>
                <ChevronLeft className="h-4 w-4" />
              </Button>
              <Button variant="outline" size="sm" onClick={goToToday}>
                Today
              </Button>
              <Button variant="outline" size="icon" onClick={goToNextMonth}>
                <ChevronRight className="h-4 w-4" />
              </Button>
            </div>
          </div>
          
          {/* Program info */}
          <div className="flex items-center gap-4 text-sm text-muted-foreground">
            <div className="flex items-center gap-2">
              <CalendarIcon className="h-4 w-4" />
              <span>
                {format(startDate, 'MMM d')} - {format(programEndDate, 'MMM d, yyyy')}
              </span>
            </div>
            <Button variant="ghost" size="sm" onClick={goToStart}>
              Go to start
            </Button>
          </div>
        </div>
        
        {/* Legend */}
        <div className="flex items-center gap-4 text-xs flex-wrap">
          <div className="flex items-center gap-1.5">
            <div className="w-3 h-3 rounded bg-primary/20 border border-primary/30" />
            <span>Training</span>
          </div>
          <div className="flex items-center gap-1.5">
            <div className="w-3 h-3 rounded bg-amber-200 dark:bg-amber-900 border border-amber-300" />
            <span>Deload</span>
          </div>
          <div className="flex items-center gap-1.5">
            <div className="w-3 h-3 rounded bg-red-200 dark:bg-red-900 border border-red-300" />
            <span>Competition</span>
          </div>
          {isDragEnabled && (
            <div className="flex items-center gap-1.5">
              <GripVertical className="h-3 w-3 text-muted-foreground" />
              <span className="text-muted-foreground">Drag training days to reschedule</span>
            </div>
          )}
          <div className="flex items-center gap-1.5">
            <Info className="h-3 w-3 text-muted-foreground" />
            <span className="text-muted-foreground">Click on a day to see details</span>
          </div>
        </div>
        
        {/* Calendar Grid */}
        <div className="border rounded-lg overflow-hidden">
          {/* Week day headers */}
          <div className="grid grid-cols-7 border-b bg-muted/30">
            {weekDays.map((day) => (
              <div
                key={day}
                className="py-2 text-center text-sm font-medium text-muted-foreground"
              >
                {day}
              </div>
            ))}
          </div>
          
          {/* Calendar days */}
          <div className="grid grid-cols-7 gap-px bg-border">
            {calendarDays.map((date) => {
              const dateKey = format(date, 'yyyy-MM-dd');
              const event = eventMap.get(dateKey);
              const inRange = isDateInRange(date);
              
              return (
                <DayCell
                  key={dateKey}
                  date={date}
                  event={event}
                  isCurrentMonth={isSameMonth(date, currentMonth)}
                  isInProgramRange={inRange}
                  isDragEnabled={isDragEnabled}
                  isDraggedOver={false}
                  onDayClick={onDayClick}
                />
              );
            })}
          </div>
        </div>
        
        {/* Drag overlay - floating card that follows cursor */}
        <DragOverlay dropAnimation={null}>
          {activeDragEvent && (
            <div className={cn(
              'rounded-lg border-2 border-primary shadow-lg p-2 min-w-[100px]',
              'bg-background/95 backdrop-blur-sm',
            )}>
              <EventContent event={activeDragEvent} compact />
            </div>
          )}
        </DragOverlay>
        
        {/* Week summary footer */}
        <div className="grid grid-cols-4 md:grid-cols-8 gap-2 text-xs">
          {weeks.slice(0, 8).map((week) => (
            <button
              key={week.id}
              onClick={() => {
                const weekStart = addDays(startDate, (week.weekNumber - 1) * 7);
                setCurrentMonth(weekStart);
              }}
              className={cn(
                'p-2 rounded border text-left hover:bg-muted/50 transition-colors',
                'focus:outline-none focus:ring-2 focus:ring-primary/30'
              )}
            >
              <div className="font-medium">W{week.weekNumber}</div>
              <div className="text-muted-foreground">{week.days.length} days</div>
            </button>
          ))}
          {weeks.length > 8 && (
            <div className="p-2 text-muted-foreground">
              +{weeks.length - 8} more weeks
            </div>
          )}
        </div>
      </div>
    </DndContext>
  );
}

// Export utility functions for testing
// Re-export utils for convenience
export type { CalendarEvent } from './calendar-utils';
export { getCalendarDays, mapProgramToCalendar, getTrainingDayOffset } from './calendar-utils';