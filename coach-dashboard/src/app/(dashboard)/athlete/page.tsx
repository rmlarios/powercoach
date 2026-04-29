'use client';

import { useAuth } from '@/providers';
import { useTodayWorkout, useWeekWorkouts } from '@/hooks';
import { useRouter } from 'next/navigation';
import { routes } from '@/config/routes';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Badge } from '@/components/ui/badge';
import { Skeleton } from '@/components/ui/skeleton';
import {
  Dumbbell,
  Calendar,
  ClipboardCheck,
  ChevronRight,
  Play,
  CheckCircle2,
  Clock,
  SkipForward,
  AlertCircle,
} from 'lucide-react';
import { format } from 'date-fns';
import type { WeekDay, WorkoutStatus } from '@/types/workout-tracking';

const statusConfig: Record<WorkoutStatus, { label: string; color: string; icon: React.ReactNode }> = {
  NotStarted: { label: 'Not Started', color: 'bg-slate-100 text-slate-700', icon: <Clock className="h-3.5 w-3.5" /> },
  InProgress: { label: 'In Progress', color: 'bg-blue-100 text-blue-700', icon: <Play className="h-3.5 w-3.5" /> },
  Completed: { label: 'Completed', color: 'bg-green-100 text-green-700', icon: <CheckCircle2 className="h-3.5 w-3.5" /> },
  Skipped: { label: 'Skipped', color: 'bg-amber-100 text-amber-700', icon: <SkipForward className="h-3.5 w-3.5" /> },
  PartiallyCompleted: { label: 'Partial', color: 'bg-orange-100 text-orange-700', icon: <AlertCircle className="h-3.5 w-3.5" /> },
};

function WorkoutStatusBadge({ status }: { status: WorkoutStatus }) {
  const cfg = statusConfig[status] || statusConfig.NotStarted;
  return (
    <Badge variant="secondary" className={`${cfg.color} gap-1 text-xs font-medium`}>
      {cfg.icon}
      {cfg.label}
    </Badge>
  );
}

function DayCard({ day }: { day: WeekDay }) {
  const router = useRouter();

  return (
    <div
      className={`flex items-center justify-between p-4 rounded-xl border transition-all cursor-pointer hover:shadow-sm ${
        day.isToday
          ? 'border-blue-200 bg-blue-50/50 ring-2 ring-blue-500/20'
          : day.status === 'Completed'
          ? 'border-green-100 bg-green-50/30'
          : 'border-slate-100 bg-white hover:border-slate-200'
      }`}
      onClick={() => router.push(routes.workout)}
    >
      <div className="flex items-center gap-4">
        <div
          className={`w-10 h-10 rounded-lg flex items-center justify-center text-sm font-bold ${
            day.isToday
              ? 'bg-blue-600 text-white'
              : day.status === 'Completed'
              ? 'bg-green-600 text-white'
              : 'bg-slate-100 text-slate-600'
          }`}
        >
          D{day.dayNumber}
        </div>
        <div>
          <p className="font-semibold text-slate-900">
            {day.dayName || `Day ${day.dayNumber}`}
            {day.isToday && (
              <span className="ml-2 text-xs text-blue-600 font-medium uppercase tracking-wider">Today</span>
            )}
          </p>
          <p className="text-sm text-slate-500">
            {day.focus || `${day.exerciseCount} exercises`}
            {day.durationMinutes ? ` · ${day.durationMinutes} min` : ''}
          </p>
        </div>
      </div>

      <div className="flex items-center gap-3">
        <WorkoutStatusBadge status={day.status} />
        {day.isToday && (day.status === 'NotStarted' || day.status === 'InProgress') && (
          <ChevronRight className="h-5 w-5 text-blue-400" />
        )}
      </div>
    </div>
  );
}

export default function AthleteDashboardPage() {
  const { user } = useAuth();
  const router = useRouter();
  const athleteId = user?.athleteId || '';

  const { data: todayWorkout, isLoading: loadingToday } = useTodayWorkout(athleteId);
  const { data: weekData, isLoading: loadingWeek } = useWeekWorkouts(athleteId);

  const greeting = () => {
    const hour = new Date().getHours();
    if (hour < 12) return 'Good morning';
    if (hour < 18) return 'Good afternoon';
    return 'Good evening';
  };

  return (
    <div className="space-y-8 max-w-3xl mx-auto">
      {/* Header */}
      <div>
        <h1 className="text-2xl font-bold text-slate-900">
          {greeting()}, {user?.username || 'Athlete'} 💪
        </h1>
        <p className="text-slate-500 mt-1">
          {format(new Date(), 'EEEE, MMMM d, yyyy')}
        </p>
      </div>

      {/* Quick Actions */}
      <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
        <Card
          className="cursor-pointer hover:shadow-md transition-shadow border-blue-100 bg-gradient-to-br from-blue-50 to-white"
          onClick={() => router.push(routes.workout)}
        >
          <CardContent className="p-6 flex items-center gap-4">
            <div className="w-12 h-12 rounded-xl bg-blue-600 flex items-center justify-center shrink-0">
              <Dumbbell className="h-6 w-6 text-white" />
            </div>
            <div>
              <h3 className="font-semibold text-slate-900">Start Workout</h3>
              <p className="text-sm text-slate-500">
                {todayWorkout
                  ? `${todayWorkout.dayName || `Day ${todayWorkout.dayNumber}`} – ${
                      todayWorkout.exercises?.length || 0
                    } exercises`
                  : 'No workout scheduled'}
              </p>
            </div>
          </CardContent>
        </Card>

        <Card
          className="cursor-pointer hover:shadow-md transition-shadow border-emerald-100 bg-gradient-to-br from-emerald-50 to-white"
          onClick={() => router.push(routes.athleteCheckIn)}
        >
          <CardContent className="p-6 flex items-center gap-4">
            <div className="w-12 h-12 rounded-xl bg-emerald-600 flex items-center justify-center shrink-0">
              <ClipboardCheck className="h-6 w-6 text-white" />
            </div>
            <div>
              <h3 className="font-semibold text-slate-900">Submit Check-in</h3>
              <p className="text-sm text-slate-500">Log wellness &amp; progress</p>
            </div>
          </CardContent>
        </Card>
      </div>

      {/* Today's Workout */}
      <Card className="shadow-sm">
        <CardHeader className="pb-3">
          <CardTitle className="text-lg flex items-center gap-2">
            <Dumbbell className="h-5 w-5 text-blue-600" />
            Today&apos;s Workout
          </CardTitle>
        </CardHeader>
        <CardContent>
          {loadingToday ? (
            <div className="space-y-3">
              <Skeleton className="h-16 w-full rounded-xl" />
              <Skeleton className="h-10 w-40 rounded-lg" />
            </div>
          ) : todayWorkout ? (
            <div className="space-y-4">
              <div className="flex items-center justify-between">
                <div>
                  <h3 className="text-lg font-semibold text-slate-900">
                    {todayWorkout.dayName || `Day ${todayWorkout.dayNumber}`}
                  </h3>
                  <p className="text-sm text-slate-500">
                    {todayWorkout.programName} · Week {todayWorkout.weekNumber}
                    {todayWorkout.focus && ` · ${todayWorkout.focus}`}
                  </p>
                </div>
                <WorkoutStatusBadge status={todayWorkout.status} />
              </div>

              {todayWorkout.exercises && todayWorkout.exercises.length > 0 && (
                <div className="grid grid-cols-2 sm:grid-cols-3 gap-2">
                  {todayWorkout.exercises.slice(0, 6).map((ex) => (
                    <div
                      key={ex.exerciseId}
                      className="px-3 py-2 bg-slate-50 rounded-lg text-sm"
                    >
                      <p className="font-medium text-slate-700 truncate">{ex.exerciseName}</p>
                      <p className="text-xs text-slate-400">
                        {ex.prescribedSets}×{ex.prescribedReps || '?'}
                      </p>
                    </div>
                  ))}
                </div>
              )}

              <Button
                onClick={() => router.push(routes.workout)}
                className="w-full sm:w-auto"
                size="lg"
              >
                {todayWorkout.status === 'InProgress' ? (
                  <>
                    <Play className="h-4 w-4 mr-2" /> Continue Workout
                  </>
                ) : todayWorkout.status === 'Completed' ? (
                  <>
                    <CheckCircle2 className="h-4 w-4 mr-2" /> View Completed
                  </>
                ) : (
                  <>
                    <Play className="h-4 w-4 mr-2" /> Start Workout
                  </>
                )}
              </Button>
            </div>
          ) : (
            <div className="text-center py-8">
              <Dumbbell className="h-10 w-10 mx-auto text-slate-300 mb-3" />
              <p className="text-slate-500 font-medium">No workout scheduled for today</p>
              <p className="text-sm text-slate-400 mt-1">Enjoy your rest day!</p>
            </div>
          )}
        </CardContent>
      </Card>

      {/* This Week */}
      <Card className="shadow-sm">
        <CardHeader className="pb-3">
          <CardTitle className="text-lg flex items-center gap-2">
            <Calendar className="h-5 w-5 text-indigo-600" />
            This Week
            {weekData && (
              <span className="text-sm font-normal text-slate-400 ml-2">
                {weekData.programName} · Week {weekData.weekNumber}/{weekData.totalWeeks}
              </span>
            )}
          </CardTitle>
        </CardHeader>
        <CardContent>
          {loadingWeek ? (
            <div className="space-y-3">
              {[1, 2, 3].map((i) => (
                <Skeleton key={i} className="h-16 w-full rounded-xl" />
              ))}
            </div>
          ) : weekData && weekData.days.length > 0 ? (
            <div className="space-y-3">
              {weekData.days.map((day) => (
                <DayCard key={day.workoutId} day={day} />
              ))}
            </div>
          ) : (
            <div className="text-center py-8">
              <Calendar className="h-10 w-10 mx-auto text-slate-300 mb-3" />
              <p className="text-slate-500 font-medium">No program assigned yet</p>
              <p className="text-sm text-slate-400 mt-1">Contact your coach to get started</p>
            </div>
          )}
        </CardContent>
      </Card>
    </div>
  );
}
