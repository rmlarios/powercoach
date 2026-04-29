'use client';

import { PageHeader, StatsCard } from '@/components/common';
import { Users, ClipboardList, FileText, TrendingUp, ClipboardCheck, BarChart3 } from 'lucide-react';
import { useAuth, useCoach } from '@/providers';
import { useCoachDashboard } from '@/hooks';
import { Skeleton } from '@/components/ui/skeleton';
import { AlertsPanel } from './components/alerts-panel';
import { AthleteStatusTable } from './components/athlete-status-table';
import { ActivityFeed } from './components/activity-feed';

export default function DashboardPage() {
  const { user } = useAuth();
  const { coach } = useCoach();

  // Use coachId from user JWT first, then coach provider, then seeded fallback for Admin
  const coachId = user?.coachId ?? coach?.id ?? '';

  const { data: dashboard, isLoading } = useCoachDashboard(coachId);

  // Loading skeleton
  if (isLoading) {
    return (
      <div>
        <PageHeader
          title="Cargando..."
          description=""
        />
        <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-3 xl:grid-cols-6 mb-6">
          {Array.from({ length: 6 }).map((_, i) => (
            <Skeleton key={i} className="h-[100px] rounded-xl" />
          ))}
        </div>
        <div className="grid gap-6 lg:grid-cols-3 mb-6">
          <Skeleton className="h-[300px] rounded-xl" />
          <Skeleton className="h-[300px] rounded-xl lg:col-span-2" />
        </div>
        <Skeleton className="h-[400px] rounded-xl" />
      </div>
    );
  }

  if (!dashboard) {
    return (
      <div>
        <PageHeader
          title={`¡Bienvenido, ${user?.username?.split(' ')[0] || 'Coach'}!`}
          description={!coachId ? 'Esta cuenta Admin no tiene un perfil de coach asignado.' : 'No se pudieron cargar los datos del dashboard.'}
        />
      </div>
    );
  }

  const { stats, alerts, athleteStatuses, recentActivity } = dashboard;

  return (
    <div>
      <PageHeader
        title={`¡Bienvenido, ${user?.username?.split(' ')[0] || coach?.name?.split(' ')[0] || 'Coach'}!`}
        description="Resumen de tu negocio de coaching hoy."
      />

      {/* Stats Grid - 6 KPIs */}
      <div className="grid gap-3 grid-cols-2 md:grid-cols-3 xl:grid-cols-6 mb-6">
        <StatsCard
          title="Atletas Activos"
          value={stats.activeAthletes}
          icon={Users}
          trend={
            stats.athletesTrend !== 0
              ? { value: Math.abs(stats.athletesTrend), isPositive: stats.athletesTrend > 0 }
              : undefined
          }
        />
        <StatsCard
          title="Solicitudes"
          value={stats.pendingApplications}
          description="pendientes"
          icon={ClipboardList}
        />
        <StatsCard
          title="Programas Activos"
          value={stats.activePrograms}
          icon={FileText}
        />
        <StatsCard
          title="Cumplimiento"
          value={`${stats.completionRate}%`}
          description="últimos 7 días"
          icon={TrendingUp}
        />
        <StatsCard
          title="Check-ins"
          value={stats.pendingCheckIns}
          description="sin revisar"
          icon={ClipboardCheck}
        />
        <StatsCard
          title="Tendencia"
          value={stats.athletesTrend >= 0 ? `+${stats.athletesTrend}` : `${stats.athletesTrend}`}
          description="vs mes anterior"
          icon={BarChart3}
          trend={
            stats.athletesTrend !== 0
              ? { value: Math.abs(stats.athletesTrend), isPositive: stats.athletesTrend > 0 }
              : undefined
          }
        />
      </div>

      {/* Alerts + Activity Feed row */}
      <div className="grid gap-6 lg:grid-cols-3 mb-6">
        <AlertsPanel alerts={alerts} className="lg:col-span-1" />
        <ActivityFeed items={recentActivity} className="lg:col-span-2" />
      </div>

      {/* Athlete Status Table - full width */}
      <AthleteStatusTable athletes={athleteStatuses} />


    </div>
  );
}
