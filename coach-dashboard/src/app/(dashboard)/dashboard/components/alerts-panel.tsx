'use client';

import { DashboardAlert } from '@/types';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import {
  AlertTriangle,
  XCircle,
  Flame,
  Calendar,
  CreditCard,
  FileText,
} from 'lucide-react';
import { cn } from '@/lib/utils';

interface AlertsPanelProps {
  alerts: DashboardAlert[];
  className?: string;
}

const alertIconMap: Record<string, React.ElementType> = {
  HighFatigue: Flame,
  MissedWorkout: XCircle,
  MissedCheckIn: Calendar,
  ExpiringSubscription: CreditCard,
  PendingApplication: FileText,
};

function timeAgo(dateStr: string): string {
  const diff = Date.now() - new Date(dateStr).getTime();
  const minutes = Math.floor(diff / 60000);
  if (minutes < 60) return `hace ${minutes}min`;
  const hours = Math.floor(minutes / 60);
  if (hours < 24) return `hace ${hours}h`;
  const days = Math.floor(hours / 24);
  return `hace ${days}d`;
}

export function AlertsPanel({ alerts, className }: AlertsPanelProps) {
  if (alerts.length === 0) {
    return (
      <Card className={className}>
        <CardHeader className="pb-3">
          <CardTitle className="text-base font-semibold flex items-center gap-2">
            <AlertTriangle className="h-4 w-4 text-slate-400" />
            Alertas
          </CardTitle>
        </CardHeader>
        <CardContent>
          <p className="text-sm text-slate-500 text-center py-6">
            Sin alertas activas. ¡Todo en orden! ✓
          </p>
        </CardContent>
      </Card>
    );
  }

  return (
    <Card className={className}>
      <CardHeader className="pb-3">
        <CardTitle className="text-base font-semibold flex items-center gap-2">
          <AlertTriangle className="h-4 w-4 text-amber-500" />
          Alertas
          <span className="ml-auto text-xs font-normal text-slate-400">
            {alerts.length} activa{alerts.length !== 1 ? 's' : ''}
          </span>
        </CardTitle>
      </CardHeader>
      <CardContent className="space-y-1 pt-0">
        {alerts.map((alert, i) => {
          const Icon = alertIconMap[alert.alertType] || AlertTriangle;
          const isDanger = alert.severity === 'danger';

          return (
            <div
              key={`${alert.alertType}-${alert.athleteId ?? i}`}
              className={cn(
                'flex items-start gap-3 rounded-lg px-3 py-2.5 text-sm transition-colors',
                isDanger
                  ? 'bg-red-50 border border-red-100'
                  : 'bg-amber-50/70 border border-amber-100'
              )}
            >
              <Icon
                className={cn(
                  'h-4 w-4 mt-0.5 shrink-0',
                  isDanger ? 'text-red-500' : 'text-amber-500'
                )}
              />
              <div className="flex-1 min-w-0">
                <p
                  className={cn(
                    'font-medium leading-snug',
                    isDanger ? 'text-red-800' : 'text-amber-800'
                  )}
                >
                  {alert.message}
                </p>
                <p className="text-xs text-slate-400 mt-0.5">
                  {timeAgo(alert.createdAt)}
                </p>
              </div>
            </div>
          );
        })}
      </CardContent>
    </Card>
  );
}
