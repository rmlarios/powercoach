## Plan: Coach Dashboard Inteligente

**TL;DR**: Crear un módulo backend (Query CQRS + DTOs + Controller) y reemplazar la page frontend existente con datos reales. El backend agrega estado de atletas, alertas (fatiga, inactividad, check-ins pendientes), y actividad reciente desde `AthleteWorkout`, `CheckIn` y `Application`. El frontend usa la ruta existente `/dashboard` (ya tiene layout + sidebar), reemplazando los datos hardcodeados con un hook TanStack Query.

**Decisiones clave**:
- Reusar la ruta existente `/dashboard` (no crear `/coach/dashboard` — ya existe con sidebar + layout)
- Una sola Query CQRS que devuelve todo el DTO — 1 request, dashboard carga en <200ms
- Alertas como reglas evaluadas en el handler (no domain service separado — simple y extensible)
- Sin migración EF necesaria — solo lectura de entidades existentes

---

**Steps**

### Backend

**1.** Crear DTOs en `src/CoachPlatform.Application/Shared/DTOs/DashboardDtos.cs`:
- `CoachDashboardDto` — contenedor principal: `Stats`, `Alerts[]`, `AthleteStatuses[]`, `RecentActivity[]`
- `DashboardStatsDto` — `ActiveAthletes`, `PendingApplications`, `ActivePrograms`, `CompletionRate`, `PendingCheckIns`, `AthletesTrend` (vs mes anterior)
- `DashboardAlertDto` — `AlertType` (enum string: `HighFatigue`, `MissedWorkout`, `MissedCheckIn`, `ExpiringSubscription`, `PendingApplication`), `Severity` (`warning`/`danger`), `AthleteName`, `AthleteId`, `Message`, `CreatedAt`
- `AthleteStatusRowDto` — `AthleteId`, `Name`, `Status`, `StatusColor` (green/yellow/red), `LastWorkoutDate`, `LastWorkoutName`, `LastCheckInDate`, `IsCheckInReviewed`, `ActiveProgramName`, `CurrentWeek`
- `ActivityFeedItemDto` — `Type` (`WorkoutCompleted`/`CheckInSubmitted`/`ApplicationReceived`/`WorkoutStarted`), `AthleteName`, `AthleteId`, `Description`, `Timestamp`

**2.** Crear la carpeta `Features/Dashboard/Queries/GetCoachDashboard/` con:
- `GetCoachDashboardQuery.cs` — `record` implementando `IRequest<CoachDashboardDto>` + `ITenantRequest` con propiedad `CoachId`
- `GetCoachDashboardQueryHandler.cs` — inyecta `IApplicationDbContext`, ejecuta queries con `AsNoTracking()`:

  **Stats**: 
  - `ActiveAthletes` = `Athletes.Count(a => a.CoachId == id && a.Status == Active)`
  - `PendingApplications` = `Applications.Count(a => a.CoachId == id && a.Status == Pending)`
  - `ActivePrograms` = `AthletePrograms` join Athletes por CoachId donde `Status == Active`
  - `CompletionRate` = `AthleteWorkouts` de últimos 7 días, `Completed / (Completed + Skipped + NotStarted con ScheduledDate < hoy)`
  - `PendingCheckIns` = `CheckIns` join Athletes por CoachId, `!IsReviewed`

  **Alerts** (reglas simples, evaluadas en-handler):
  - **HighFatigue**: `AthleteWorkouts` de últimos 2 días con `FatigueRating >= 8` → severity `warning`
  - **MissedWorkout**: Atletas activos sin `AthleteWorkout` con `CompletedDate` en últimos 2 días (y tienen programa activo con workouts programados) → severity `danger`
  - **MissedCheckIn**: Atletas activos sin `CheckIn` en últimos 7 días → severity `warning`
  - **ExpiringSubscription**: `Subscriptions` con `EndDate` en próximos 7 días y `Status == Active` → severity `warning`
  - **PendingApplication**: `Applications` con `Status == Pending` más de 48h → severity `warning`

  **AthleteStatuses**: Join `Athletes` (Active) con:
  - Último `AthleteWorkout` (via `AthleteProgram`) → `LastWorkoutDate`, `LastWorkoutName` (DayName del `ProgramDayTemplate`)
  - Último `CheckIn` → `LastCheckInDate`, `IsReviewed`
  - `AthleteProgram` activo → `ProgramName`, `CurrentWeek`
  - `StatusColor`: green si workout <2d y checkIn <7d, yellow si check-in pendiente, red si workout >2d o fatiga alta

  **RecentActivity**: Union de:
  - `AthleteWorkouts` completados/iniciados últimos 3 días → `WorkoutCompleted`/`WorkoutStarted`
  - `CheckIns` últimos 3 días → `CheckInSubmitted`
  - `Applications` últimos 3 días → `ApplicationReceived`
  - Ordenados por `Timestamp DESC`, `Take(20)`

**3.** Crear `src/CoachPlatform.API/Controllers/DashboardController.cs`:
- `[Route("api/dashboard")]`, `[ApiController]`
- Inyecta `IMediator` + `ITenantService`
- `GET /api/dashboard/coach` → `GetCoachDashboardQuery { CoachId = _tenantService.CoachId }` → `Ok(result)`

**4.** Crear tests unitarios en `tests/CoachPlatform.UnitTests/Application/Queries/GetCoachDashboardQueryHandlerTests.cs`:
- `Handle_WithActiveAthletes_ShouldReturnCorrectStats`
- `Handle_WithHighFatigue_ShouldGenerateAlert`
- `Handle_WithMissedWorkouts_ShouldGenerateAlert`
- `Handle_WithUnreviewedCheckIns_ShouldGenerateAlert`
- `Handle_WithRecentActivity_ShouldReturnOrderedFeed`
- `Handle_WithNoAthletes_ShouldReturnEmptyDashboard`

### Frontend

**5.** Agregar tipos en `coach-dashboard/src/types/dashboard.ts`:
- Interfaces TypeScript que espejean los DTOs del backend: `CoachDashboard`, `DashboardStats`, `DashboardAlert`, `AthleteStatusRow`, `ActivityFeedItem`
- Re-exportar desde `types/index.ts`

**6.** Agregar endpoint + API function:
- Agregar `dashboard: { coach: '/dashboard/coach' }` en `endpoints.ts`
- Crear `coach-dashboard/src/lib/api/dashboard-api.ts` con `getCoachDashboard(coachId): Promise<CoachDashboard>`

**7.** Crear hook `coach-dashboard/src/hooks/use-dashboard.ts`:
- `dashboardKeys` factory (`['dashboard', 'coach', coachId]`)
- `useCoachDashboard(coachId)` — TanStack Query con `staleTime: 30_000` (refresco cada 30s), `refetchInterval: 60_000` (auto-refetch cada 1min)
- Re-exportar desde `hooks/index.ts`

**8.** Crear componentes en `coach-dashboard/src/app/(dashboard)/dashboard/components/`:
- `alerts-panel.tsx` — Lista vertical de alertas con iconos (AlertTriangle/AlertCircle), colores severity (red/amber), botón de acción por alerta, link a atleta. Usa `Badge` de shadcn.
- `athlete-status-table.tsx` — Tabla con columnas: Avatar+Nombre, Status (color dot), Programa, Último Workout (relative date), Último Check-in (relative date), acciones. Usa `StatusBadge` existente. Link a `/athletes/{id}`. Color coding: fila tintada sutilmente si hay problema.
- `activity-feed.tsx` — Timeline vertical tipo SaaS (línea lateral con dots). Items: icono por tipo (Dumbbell/ClipboardCheck/UserPlus), descripción, relative timestamp. Max 20 items con scroll.

**9.** Reescribir `coach-dashboard/src/app/(dashboard)/dashboard/page.tsx`:
- Importar `useCoachDashboard` + `useCoach`
- `StatsCard` grid (4 cards) con datos reales de `stats`
- `AlertsPanel` si hay alertas (prominente, arriba)
- Grid 2 columnas: `AthleteStatusTable` (más ancha, 2/3) + `ActivityFeed` (1/3)
- Loading skeleton con `Skeleton` de shadcn mientras carga
- Mock data fallback en dev mode (`USE_MOCK_DATA`) con datos realistas
- Empty state si no hay atletas

### Verification

- Backend: `cd d:\PowerCoach\powercoach; dotnet build; dotnet test` — 0 errores, todos tests pasan (existentes 212 + 6 nuevos ≈ 218)
- Frontend: `cd d:\PowerCoach\powercoach\coach-dashboard; npx tsc --noEmit 2>&1 | Where-Object { $_ -match 'error TS' } | Where-Object { $_ -notmatch 'mock-data\.ts' }` — 0 errores nuevos
- Visual: abrir `http://localhost:3000/dashboard`, verificar que las 4 secciones renderizan (stats, alertas, tabla atletas, activity feed)

### Decisions

- **Ruta `/dashboard`** sobre `/coach/dashboard`: ya existe la page con sidebar navigation configurada, renombrar rompería la nav
- **Una Query CQRS** sobre múltiples endpoints: el dashboard debe cargar en un solo request para sentirse rápido
- **Alertas en el handler** sobre domain service separado: las reglas son simples (3 condiciones), un service agregaría indirección innecesaria; cuando crezcan, extraer a `IAlertEvaluator`
- **No EF migration**: todo es lectura sobre entidades existentes
- **Mock data en dev**: misma estrategia que workout tracking (`USE_MOCK_DATA`), permite testing visual sin backend
