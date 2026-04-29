'use client';

import { ActivityFeedItem } from '@/types';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import {
  Activity,
  Dumbbell,
  PlayCircle,
  ClipboardCheck,
  UserPlus,
} from 'lucide-react';
import { cn } from '@/lib/utils';

interface ActivityFeedProps {
  items: ActivityFeedItem[];
  className?: string;
}

const activityConfig: Record<
  string,
  { icon: React.ElementType; color: string; bgColor: string }
> = {
  WorkoutCompleted: {
    icon: Dumbbell,
    color: 'text-emerald-600',
    bgColor: 'bg-emerald-50',
  },
  WorkoutStarted: {
    icon: PlayCircle,
    color: 'text-blue-600',
    bgColor: 'bg-blue-50',
  },
  CheckInSubmitted: {
    icon: ClipboardCheck,
    color: 'text-violet-600',
    bgColor: 'bg-violet-50',
  },
  ApplicationReceived: {
    icon: UserPlus,
    color: 'text-amber-600',
    bgColor: 'bg-amber-50',
  },
};

function formatTimestamp(dateStr: string): string {
  const diff = Date.now() - new Date(dateStr).getTime();
  const minutes = Math.floor(diff / 60000);
  if (minutes < 1) return 'Ahora';
  if (minutes < 60) return `Hace ${minutes}min`;
  const hours = Math.floor(minutes / 60);
  if (hours < 24) return `Hace ${hours}h`;
  const days = Math.floor(hours / 24);
  if (days === 1) return 'Ayer';
  return `Hace ${days} días`;
}

export function ActivityFeed({ items, className }: ActivityFeedProps) {
  return (
    <Card className={className}>
      <CardHeader className="pb-3">
        <CardTitle className="text-base font-semibold flex items-center gap-2">
          <Activity className="h-4 w-4 text-slate-500" />
          Actividad Reciente
        </CardTitle>
      </CardHeader>
      <CardContent className="pt-0">
        {items.length === 0 ? (
          <p className="text-sm text-slate-500 text-center py-6">
            Sin actividad reciente.
          </p>
        ) : (
          <div className="space-y-1">
            {items.map((item, i) => {
              const config = activityConfig[item.type] ?? {
                icon: Activity,
                color: 'text-slate-600',
                bgColor: 'bg-slate-50',
              };
              const Icon = config.icon;

              return (
                <div
                  key={`${item.type}-${item.timestamp}-${i}`}
                  className="flex items-start gap-3 py-2.5 px-2 rounded-lg hover:bg-slate-50/60 transition-colors"
                >
                  <div
                    className={cn(
                      'h-8 w-8 rounded-full flex items-center justify-center shrink-0',
                      config.bgColor
                    )}
                  >
                    <Icon className={cn('h-4 w-4', config.color)} />
                  </div>
                  <div className="flex-1 min-w-0">
                    <p className="text-sm text-slate-700 leading-snug">
                      {item.description}
                    </p>
                    <p className="text-xs text-slate-400 mt-0.5">
                      {formatTimestamp(item.timestamp)}
                    </p>
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
