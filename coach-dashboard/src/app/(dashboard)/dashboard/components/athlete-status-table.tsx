'use client';

import { AthleteStatusRow } from '@/types';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Users } from 'lucide-react';
import { cn } from '@/lib/utils';

interface AthleteStatusTableProps {
  athletes: AthleteStatusRow[];
  className?: string;
}

const statusColorClasses: Record<string, { dot: string; bg: string; text: string }> = {
  red: { dot: 'bg-red-500', bg: 'bg-red-50', text: 'text-red-700' },
  yellow: { dot: 'bg-amber-400', bg: 'bg-amber-50', text: 'text-amber-700' },
  green: { dot: 'bg-emerald-500', bg: 'bg-emerald-50', text: 'text-emerald-700' },
};

function formatRelativeDate(dateStr?: string): string {
  if (!dateStr) return '—';
  const diff = Date.now() - new Date(dateStr).getTime();
  const hours = Math.floor(diff / 3600000);
  if (hours < 1) return 'Hace minutos';
  if (hours < 24) return `Hace ${hours}h`;
  const days = Math.floor(hours / 24);
  if (days === 1) return 'Ayer';
  return `Hace ${days} días`;
}

export function AthleteStatusTable({ athletes, className }: AthleteStatusTableProps) {
  return (
    <Card className={className}>
      <CardHeader className="pb-3">
        <CardTitle className="text-base font-semibold flex items-center gap-2">
          <Users className="h-4 w-4 text-slate-500" />
          Estado de Atletas
          <span className="ml-auto text-xs font-normal text-slate-400">
            {athletes.length} activo{athletes.length !== 1 ? 's' : ''}
          </span>
        </CardTitle>
      </CardHeader>
      <CardContent className="pt-0">
        {athletes.length === 0 ? (
          <p className="text-sm text-slate-500 text-center py-6">
            No hay atletas activos.
          </p>
        ) : (
          <div className="space-y-1">
            {/* Header */}
            <div className="grid grid-cols-12 gap-2 px-3 py-1.5 text-xs font-medium text-slate-400 uppercase tracking-wider">
              <div className="col-span-4">Atleta</div>
              <div className="col-span-3">Último entreno</div>
              <div className="col-span-3">Programa</div>
              <div className="col-span-2 text-center">Estado</div>
            </div>

            {/* Rows */}
            {athletes.map((athlete) => {
              const colors = statusColorClasses[athlete.statusColor] ?? statusColorClasses.green;

              return (
                <div
                  key={athlete.athleteId}
                  className="grid grid-cols-12 gap-2 items-center rounded-lg px-3 py-2.5 hover:bg-slate-50 transition-colors border-b border-slate-50 last:border-0"
                >
                  {/* Name */}
                  <div className="col-span-4 flex items-center gap-2 min-w-0">
                    <div
                      className={cn(
                        'h-8 w-8 rounded-full flex items-center justify-center text-xs font-semibold shrink-0',
                        colors.bg,
                        colors.text
                      )}
                    >
                      {athlete.name
                        .split(' ')
                        .map((n) => n[0])
                        .join('')
                        .slice(0, 2)
                        .toUpperCase()}
                    </div>
                    <div className="min-w-0">
                      <p className="text-sm font-medium text-slate-900 truncate">
                        {athlete.name}
                      </p>
                      {athlete.lastCheckInDate && (
                        <p className="text-[11px] text-slate-400 leading-tight">
                          Check-in: {formatRelativeDate(athlete.lastCheckInDate)}
                          {athlete.isCheckInReviewed === false && (
                            <span className="text-amber-500 ml-1">· sin revisar</span>
                          )}
                        </p>
                      )}
                    </div>
                  </div>

                  {/* Last Workout */}
                  <div className="col-span-3 min-w-0">
                    {athlete.lastWorkoutName ? (
                      <>
                        <p className="text-sm text-slate-700 truncate">
                          {athlete.lastWorkoutName}
                        </p>
                        <p className="text-[11px] text-slate-400">
                          {formatRelativeDate(athlete.lastWorkoutDate)}
                        </p>
                      </>
                    ) : (
                      <p className="text-sm text-slate-400">—</p>
                    )}
                  </div>

                  {/* Program */}
                  <div className="col-span-3 min-w-0">
                    {athlete.activeProgramName ? (
                      <>
                        <p className="text-sm text-slate-700 truncate">
                          {athlete.activeProgramName}
                        </p>
                        {athlete.currentWeek && (
                          <p className="text-[11px] text-slate-400">
                            Semana {athlete.currentWeek}
                          </p>
                        )}
                      </>
                    ) : (
                      <p className="text-sm text-slate-400">Sin programa</p>
                    )}
                  </div>

                  {/* Status */}
                  <div className="col-span-2 flex justify-center">
                    <span
                      className={cn(
                        'inline-flex items-center gap-1.5 text-xs font-medium px-2.5 py-1 rounded-full',
                        colors.bg,
                        colors.text
                      )}
                    >
                      <span className={cn('h-1.5 w-1.5 rounded-full', colors.dot)} />
                      {athlete.status}
                    </span>
                  </div>
                </div>
              );
            })}
          </div>
        )}
      </CardContent>
    </Card>
  );
}
