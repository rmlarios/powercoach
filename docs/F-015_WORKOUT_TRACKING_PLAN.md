# F-015: Workout Tracking System - Implementation Plan

> Plan de implementación para el módulo de tracking de entrenamientos en tiempo real.
> Generado: 2026-03-24 | Estado: 🟡 En Progreso

---

## 📋 Resumen Ejecutivo

### Objetivo
Permitir que atletas registren sus entrenamientos en tiempo real desde móvil/desktop, con:
- Vista del workout del día
- Registro de sets completados (peso, reps, RPE)
- Tracking de progreso vs performance anterior
- Gestión de estado del workout (en progreso, completado, saltado)

### Decisión Arquitectónica: **Extender Modelo Existente (Opción B)**

| Opción | Descripción | Decisión |
|--------|-------------|----------|
| A | Crear nuevas entidades `WorkoutSession`, `WorkoutExercise`, `WorkoutSet` | ❌ Rechazada |
| B | Extender `AthleteWorkout` + `AthleteExerciseLog` existentes | ✅ **Elegida** |

**Justificación**:
- `AthleteWorkout` ya existe y tiene relación con `AthleteProgram`
- `AthleteExerciseLog` ya captura sets individuales
- Crear entidades nuevas duplicaría funcionalidad y rompería integraciones existentes
- Solo necesitamos agregar campos de estado y métricas

---

## 🏗️ Sprints Propuestos

| Sprint | Fases | Duración Est. | Descripción |
|--------|-------|---------------|-------------|
| **A** | 1, 2, 3, 4, 6 | 2-3 días | Backend Core: Domain + Commands/Queries + API |
| **B** | 5, 7 | 1 día | Backend Polish: Refactor + Tests |
| **C** | 8, 9 | 2 días | Frontend Core: Types + Página workout |
| **D** | 10, 11 | 1-2 días | Frontend Polish: UX + Integración |

---

## 📦 Fases de Implementación

---

### Phase 1: Domain Extension
**Prioridad**: 🔴 Crítica | **Complejidad**: Media | **Sprint**: A | **Estado**: ✅ Completada

Extender entidades existentes con campos necesarios para tracking.

| ID | Task | Estado | Descripción |
|----|------|--------|-------------|
| T-138 | Crear enum `WorkoutStatus` | ✅ | `NotStarted`, `InProgress`, `Completed`, `Skipped`, `PartiallyCompleted` |
| T-139 | Extender `AthleteWorkout` | ✅ | Agregado: `Status`, `StartedAt`, `SkippedReason` + métodos `Start()`, `Skip()` |
| T-140 | Extender `AthleteExerciseLog` | ✅ | Agregado: `IsCompleted`, `TargetReps`, `TargetWeight`, `SkippedReason` + métodos `Complete()`, `Skip()`, `SetTargets()` |
| T-141 | Actualizar EF Configurations | ✅ | `AthleteWorkoutConfiguration`, `AthleteExerciseLogConfiguration` con nuevos campos |

**Archivos modificados**:
- ✅ `Domain/Enums/WorkoutStatus.cs` (creado)
- ✅ `Domain/Entities/AthleteWorkout.cs`
- ✅ `Domain/Entities/AthleteExerciseLog.cs`
- ✅ `Infrastructure/Persistence/Configurations/AthleteWorkoutConfiguration.cs`
- ✅ `Infrastructure/Persistence/Configurations/AthleteExerciseLogConfiguration.cs`

---

### Phase 2: EF Migration
**Prioridad**: 🔴 Crítica | **Complejidad**: Baja | **Sprint**: A | **Estado**: ✅ Completada

| ID | Task | Estado | Descripción |
|----|------|--------|-------------|
| T-142 | Crear migration `AddWorkoutTrackingFields` | ✅ | Nuevos campos en AthleteWorkout y AthleteExerciseLog |

**Archivo generado**: `Infrastructure/Migrations/20260324182616_AddWorkoutTrackingFields.cs`

**Cambios en DB**:
- `AthleteWorkouts`: +Status (varchar 20), +StartedAt (timestamp), +SkippedReason (varchar 500)
- `AthleteExerciseLogs`: +IsCompleted (bool), +TargetReps (int), +TargetWeight (decimal), +SkippedReason (varchar 500)

---

### Phase 3: Queries
**Prioridad**: 🔴 Crítica | **Complejidad**: Media | **Sprint**: A | **Estado**: ✅ Completada

| ID | Task | Estado | Descripción |
|----|------|--------|-------------|
| T-143 | `GetTodayWorkoutQuery` | ✅ | Workout del día actual para un atleta |
| T-144 | `GetWorkoutWithSetsQuery` | ✅ | Workout completo con ejercicios y sets |
| T-145 | `GetWorkoutHistoryQuery` | ✅ | Historial de workouts del atleta (paginado) |

**DTOs necesarios**:
```csharp
public record TodayWorkoutDto(
    Guid WorkoutId,
    DateTime ScheduledDate,
    string Status,
    string? DayName,
    string? Focus,
    List<WorkoutExerciseDto> Exercises
);

public record WorkoutExerciseDto(
    Guid ExerciseLogId,
    Guid ExerciseId,
    string ExerciseName,
    int Order,
    List<WorkoutSetDto> Sets,
    PreviousPerformanceDto? PreviousPerformance
);

public record WorkoutSetDto(
    int SetNumber,
    int? TargetReps,
    decimal? TargetWeight,
    int? ActualReps,
    decimal? ActualWeight,
    decimal? ActualRPE,
    bool IsCompleted
);

public record PreviousPerformanceDto(
    DateTime Date,
    decimal? MaxWeight,
    int? MaxReps,
    decimal? EstimatedOneRM
);
```

---

### Phase 4: Commands
**Prioridad**: 🔴 Crítica | **Complejidad**: Media | **Sprint**: A | **Estado**: ✅ Completada

| ID | Task | Estado | Descripción |
|----|------|--------|-------------|
| T-146 | `StartWorkoutCommand` | ✅ | Marca workout como InProgress, setea StartedAt |
| T-147 | `UpdateSetCommand` | ✅ | Actualiza peso/reps/RPE de un set específico |
| T-148 | `CompleteSetCommand` | ✅ | Marca set como completado |
| T-149 | `SkipWorkoutCommand` | ✅ | Marca workout completo como saltado con razón |

**Validaciones**:
- `StartWorkoutCommand`: Workout debe existir, no estar ya completado
- `UpdateSetCommand`: Set debe pertenecer a workout del atleta actual
- `CompleteSetCommand`: Debe tener al menos reps registradas
- `SkipWorkoutCommand`: Workout no debe estar completado

---

### Phase 5: Refactor LogWorkout
**Prioridad**: 🟡 Alta | **Complejidad**: Baja | **Sprint**: B | **Estado**: ✅ Completada

| ID | Task | Estado | Descripción |
|----|------|--------|-------------|
| T-150 | `SaveSetCommand` (upsert) | ✅ | Crear/actualizar set con auto-save, extender `LogExercise()` con targets |

---

### Phase 6: Controller & DTOs
**Prioridad**: 🔴 Crítica | **Complejidad**: Media | **Sprint**: A | **Estado**: ✅ Completada

| ID | Task | Estado | Descripción |
|----|------|--------|-------------|
| T-151 | Crear/Extender `WorkoutsController` | ✅ | 8 nuevos endpoints REST en `AthleteWorkoutsController` |
| T-152 | Crear Request/Response DTOs | ✅ | `SaveSetRequest`, `UpdateSetRequest`, `SkipWorkoutRequest`, `CompleteWorkoutRequest` |

**Endpoints**:
```
GET  /athletes/{athleteId}/workouts/today        → TodayWorkoutDto
GET  /athletes/{athleteId}/workouts/{workoutId}  → WorkoutDetailDto
GET  /athletes/{athleteId}/workouts              → PagedList<WorkoutSummaryDto>
POST /athletes/{athleteId}/workouts/{workoutId}/start
PUT  /athletes/{athleteId}/workouts/{workoutId}/sets/{setIndex}
POST /athletes/{athleteId}/workouts/{workoutId}/sets/{setIndex}/complete
POST /athletes/{athleteId}/workouts/{workoutId}/skip
POST /athletes/{athleteId}/workouts/{workoutId}/complete
```

---

### Phase 7: Backend Tests
**Prioridad**: 🟡 Alta | **Complejidad**: Media | **Sprint**: B | **Estado**: ✅ Completada

| ID | Task | Estado | Descripción |
|----|------|--------|-------------|
| T-153 | Tests unitarios Commands | ✅ | Start, Skip, Complete workout handlers (9 tests) |
| T-154 | Tests unitarios Domain | ✅ | AthleteWorkout tracking (14 tests) + AthleteExerciseLog tracking (13 tests) |

**Resultado**: 204 tests totales, 204 pasando (166 existentes + 38 nuevos)

**Archivos creados**:
- `tests/CoachPlatform.UnitTests/Domain/Entities/AthleteWorkoutTrackingTests.cs` (14 tests)
- `tests/CoachPlatform.UnitTests/Domain/Entities/AthleteExerciseLogTrackingTests.cs` (13 tests)
- `tests/CoachPlatform.UnitTests/Application/Commands/WorkoutTrackingCommandHandlerTests.cs` (9 tests)

**Bugs corregidos durante testing**:
- `Start()`/`Skip()` usaban `Status == Completed` en vez de `IsCompleted` — no cubría `PartiallyCompleted`
- Tests de handlers: navigation property `AthleteProgram` null en mocks — fijado con reflection

---

### Phase 8: Frontend Types & API
**Prioridad**: 🔴 Crítica | **Complejidad**: Baja | **Sprint**: C | **Estado**: ✅ Completada

| ID | Task | Estado | Descripción |
|----|------|--------|-------------|
| T-155 | Crear tipos TypeScript | ✅ | `WorkoutStatus`, `TodayWorkout`, `WorkoutExerciseGroup`, `WorkoutSet`, `PreviousPerformance`, `WorkoutHistoryItem` + request DTOs |
| T-156 | Crear funciones API + hooks | ✅ | `workout-tracking-api.ts` (9 funciones) + `use-workout-tracking.ts` (3 queries + 6 mutations) + endpoints + barrel exports |

**Archivos creados/modificados**:
- `types/workout-tracking.ts` — 12 interfaces/types alineados 1:1 con backend DTOs
- `lib/api/workout-tracking-api.ts` — `workoutTrackingApi` con 9 métodos
- `hooks/workouts/use-workout-tracking.ts` — 3 query hooks + 6 mutation hooks con invalidación
- `lib/api/endpoints.ts` — Nuevo bloque `workoutTracking` con 9 endpoints
- `types/index.ts`, `lib/api/index.ts`, `hooks/index.ts` — Barrel exports actualizados

**Archivo types**:
```typescript
// types/workout-tracking.ts
export type WorkoutStatus = 'NotStarted' | 'InProgress' | 'Completed' | 'Skipped' | 'PartiallyCompleted';

export interface TodayWorkout {
  workoutId: string;
  scheduledDate: string;
  status: WorkoutStatus;
  dayName?: string;
  focus?: string;
  exercises: WorkoutExercise[];
}

export interface WorkoutExercise {
  exerciseLogId: string;
  exerciseId: string;
  exerciseName: string;
  order: number;
  sets: WorkoutSet[];
  previousPerformance?: PreviousPerformance;
}

export interface WorkoutSet {
  setNumber: number;
  targetReps?: number;
  targetWeight?: number;
  actualReps?: number;
  actualWeight?: number;
  actualRPE?: number;
  isCompleted: boolean;
}
```

---

### Phase 9: Frontend Page - Workout Tracking
**Prioridad**: 🔴 Crítica | **Complejidad**: Alta | **Sprint**: C | **Estado**: ✅ Completada

| ID | Task | Estado | Descripción |
|----|------|--------|---------|
| T-157 | Crear estructura página `/workout` | ✅ | Layout notebook, skeleton, empty state, save indicator |
| T-158 | Componente `WorkoutHeader` | ✅ | Status badge, timer en vivo, Start/Skip/Complete actions |
| T-159 | Componente `ExerciseCard` | ✅ | Nombre, prescripción, previous performance, set rows con focus management |
| T-160 | Componente `SetRow` | ✅ | Inline edit, smart parser ("100x8"), auto-tab, 44px touch zones, haptic |

**Archivos creados** (siguiendo GuiaUI.txt):
```
app/(dashboard)/workout/
  page.tsx                          # Orchestrator page
  lib/
    workout-utils.ts                # Smart parser, auto-save hook, haptic, timer
  components/
    workout-header.tsx              # Status + timer + actions
    exercise-card.tsx               # Exercise block con set focus management
    set-row.tsx                     # Inline editable set (core data entry)
    previous-performance.tsx        # Sutil badge de rendimiento anterior
    empty-workout.tsx               # Sin workout programado
    workout-skeleton.tsx            # Thin loading skeleton
    workout-dialogs.tsx             # Skip + Complete slide-up sheets
    save-indicator.tsx              # "Guardando..." / "✓ Guardado"
```

**Principios UX aplicados (GuiaUI.txt)**:
- Paper feel: `bg-slate-50`, sin bordes pesados, separadores sutiles
- Font mono para datos numéricos (`font-mono`)
- Inline editing (sin modales para datos)
- Smart parser: doble-click para modo "100x8"
- Auto-save con debounce 600ms
- Touch zones 44x44px, haptic feedback en check
- Auto-focus next set tras completar
- Micro-interacciones: opacity transition 200ms, line-through en completados

---

### Phase 10: Frontend UX Enhancements
**Prioridad**: 🟡 Alta | **Complejidad**: Media | **Sprint**: D | **Estado**: ✅ Completada (integrada en Phase 9)

| ID | Task | Estado | Descripción |
|----|------|--------|---------|
| T-161 | Inline editing en SetRow | ✅ | Tap = input activo, blur = auto-save, Enter = next field |
| T-162 | Auto-save con debounce | ✅ | `useAutoSave` hook con 600ms debounce + flush on complete |
| T-163 | Previous performance display | ✅ | `PreviousPerformanceBadge` con peso×reps, e1RM, fecha relativa |

**UX Features**:
- Tap en input → modo edición inmediato
- Cambio de valor → auto-save tras debounce
- Indicador visual de "guardando..." / "guardado"
- Swipe en set para marcar completado (mobile)
- Previous performance como texto sutil debajo del target

---

### Phase 11: Frontend Integration
**Prioridad**: 🟢 Media | **Complejidad**: Baja | **Sprint**: D | **Estado**: 🟡 Parcial

| ID | Task | Estado | Descripción |
|----|------|--------|---------|
| T-164 | Agregar link en navegación | ✅ | Sidebar: "Workout" con icono Timer en `config/navigation.ts` + route `/workout` |
| T-165 | Widget en Dashboard | ⬜ | Card de workout del día en dashboard del atleta |

---

## 📊 Métricas de Éxito

| Métrica | Target |
|---------|--------|
| Backend tests nuevos | ≥ 15 |
| Frontend tests nuevos | ≥ 10 |
| Build sin errores | ✅ |
| Tiempo de carga workout del día | < 500ms |
| Auto-save latencia | < 1s |

---

## 🔗 Dependencias

### Backend
- `AthleteWorkout` entity existente
- `AthleteExerciseLog` entity existente  
- `Exercise` entity para nombres
- Multi-tenancy (`ITenantService`) para validación

### Frontend
- TanStack Query configurado
- Axios API client
- shadcn/ui components (Card, Input, Button, Badge)

---

## 📝 Notas de Implementación

### Consideraciones Mobile-First
- Inputs numéricos con teclado numérico (`inputMode="numeric"`)
- Botones grandes para touch
- Gestos swipe para completar sets
- Pull-to-refresh para recargar workout

### Performance
- Solo cargar workout del día en página principal
- Lazy load historial de ejercicios on-demand
- Optimistic updates para feedback inmediato

### Edge Cases
- Workout sin ejercicios (template vacío)
- Atleta sin programa asignado
- Día sin workout programado (rest day)
- Workout ya completado (solo lectura)
- Sets adicionales no planificados

---

## 🚀 Próximos Pasos

1. ✅ ~~**Phase 1**: Domain Extension~~ — Completada 2026-03-24
2. ✅ ~~**Phase 2**: Crear EF Migration~~ — Completada 2026-03-24
3. **Phase 3-4**: Implementar Queries y Commands
4. **Phase 6**: Controller y DTOs

---

## 📎 Referencias

- [LogWorkoutPrompt.txt](../LogWorkoutPrompt.txt) - Prompt original del usuario
- [ARCHITECTURE_AUDIT.md](./ARCHITECTURE_AUDIT.md) - Auditoría arquitectónica
- [BACKLOG.md](./BACKLOG.md) - Backlog general del proyecto
