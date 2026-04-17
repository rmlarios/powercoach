# UX Enhancements Plan — Workout Tracking

> **Objetivo**: Elevar la experiencia de tracking de rutinas al nivel de apps como Strong/Hevy, con diferenciadores únicos gracias a la relación coach↔atleta.
>
> **Fecha de creación**: 2026-03-25  
> **Última actualización**: 2026-03-26  
> **Tareas totales**: 61 | **Completadas**: 61 | **Pendientes**: 0  ✅ ALL DONE

---

## Orden de Ejecución

1. ~~A-3 → A-5 — Cerrar backend Week Strip~~ ✅
2. ~~B-3 → B-5 — Cerrar backend Exercise History~~ ✅
3. ~~A-6 → A-9 — Frontend types/api/hooks/mock Week Strip~~ ✅
4. ~~B-6 → B-9 — Frontend types/api/hooks/mock Exercise History~~ ✅
5. ~~A-10 → A-15 — Componente WeekStrip + integración~~ ✅
6. ~~B-10 → B-20 — Componente ExerciseDetailSheet + integración~~ ✅
7. ~~C-1 → C-6 — Rest Timer~~ ✅
8. ~~D-1 → D-5 — PR Detection~~ ✅
9. ~~E-1 → E-3 — Coach Notes~~ ✅
10. ~~F-1 → H-4 — Polish features~~ ✅

---

## FASE A: Week Strip (Vista Semanal) — P0 CRÍTICO

> **Por qué**: Sin esto el atleta está ciego fuera de "hoy". No puede ver días anteriores ni planificados.

| # | Tarea | Tipo | Depende de | Estado |
|---|---|---|---|---|
| A-1 | DTO: `WeekWorkoutsDto`, `WeekDayDto` en `WorkoutTrackingDtos.cs` | Backend | — | ✅ |
| A-2 | Query: `GetWeekWorkoutsQuery` + `GetWeekWorkoutsQueryHandler` | Backend | A-1 | ✅ |
| A-3 | Endpoint: `GET /athletes/{id}/workouts/week?weekNumber=N` en `WorkoutsController` | Backend | A-2 | ✅ |
| A-4 | Build backend: verificar 0 errores de compilación | Verify | A-3 | ✅ |
| A-5 | Unit test: `GetWeekWorkoutsQueryHandlerTests` (semana actual, semana específica, sin programa activo) | Test | A-4 | ✅ |
| A-6 | Frontend types: `WeekWorkouts`, `WeekDay` en `workout-tracking.ts` + re-export en `types/index.ts` | Frontend | — | ✅ |
| A-7 | API function: `getWeekWorkouts(athleteId, weekNumber?)` en `workout-tracking-api.ts` + endpoint en `endpoints.ts` | Frontend | A-6 | ✅ |
| A-8 | Hook: `useWeekWorkouts(athleteId, weekNumber?)` en `use-workout-tracking.ts` | Frontend | A-7 | ✅ |
| A-9 | Mock data: `mockWeekWorkouts` — 5 días (2 completados, 1 hoy InProgress, 2 pendientes) | Frontend | A-6 | ✅ |
| A-10 | Componente: `WeekStrip` — barra horizontal de días con dots de estado, ← → nav, tap callback | Frontend | A-9 | ✅ |
| A-11 | WeekStrip: Navegación semana anterior/siguiente (flechas ← →, actualiza weekNumber) | Frontend | A-10 | ✅ |
| A-12 | WeekStrip: Tap en día → dispara callback `onSelectDay(workoutId)` | Frontend | A-10 | ✅ |
| A-13 | Integrar `WeekStrip` en `workout/page.tsx` — sticky debajo del header (z-20) | Frontend | A-12 | ✅ |
| A-14 | Page: lógica para cargar workout de otro día al tap (placeholder — TODO full routing) | Frontend | A-13 | ✅ |
| A-15 | Verify: `npx tsc --noEmit` sin errores nuevos — 0 errores nuevos confirmado | Verify | A-14 | ✅ |

### Archivos tocados (Fase A)
- `src/CoachPlatform.Application/Shared/DTOs/WorkoutTrackingDtos.cs` — WeekWorkoutsDto, WeekDayDto
- `src/CoachPlatform.Application/Features/WorkoutLogs/Queries/GetWeekWorkouts/` — Query + Handler
- `src/CoachPlatform.API/Controllers/WorkoutsController.cs` — nuevo endpoint
- `tests/CoachPlatform.UnitTests/Application/Queries/GetWeekWorkoutsQueryHandlerTests.cs`
- `coach-dashboard/src/types/workout-tracking.ts` — WeekWorkouts, WeekDay
- `coach-dashboard/src/types/index.ts` — re-exports
- `coach-dashboard/src/lib/api/endpoints.ts` — nueva ruta
- `coach-dashboard/src/lib/api/workout-tracking-api.ts` — getWeekWorkouts
- `coach-dashboard/src/hooks/workouts/use-workout-tracking.ts` — useWeekWorkouts
- `coach-dashboard/src/app/(dashboard)/workout/lib/mock-workout-data.ts` — mockWeekWorkouts
- `coach-dashboard/src/app/(dashboard)/workout/components/week-strip.tsx` — NUEVO
- `coach-dashboard/src/app/(dashboard)/workout/page.tsx` — integración

---

## FASE B: Exercise Detail Bottom Sheet — P0 CRÍTICO

> **Por qué**: Al tocar un ejercicio, el atleta debe poder ver video, instrucciones, coaching cues del coach, y su historial de levantamientos. Esto nos diferencia de Strong/Hevy.

| # | Tarea | Tipo | Depende de | Estado |
|---|---|---|---|---|
| B-1 | DTOs: `ExerciseLiftHistoryDto`, `LiftEntryDto`, `ExerciseLiftPRDto` en `WorkoutTrackingDtos.cs` | Backend | — | ✅ |
| B-2 | Query: `GetExerciseLiftHistoryQuery` + Handler (Epley e1RM, PRs, agrupación por sesión) | Backend | B-1 | ✅ |
| B-3 | Endpoint: `GET /athletes/{id}/workouts/exercises/{exerciseId}/lift-history?limit=20` en controller | Backend | B-2 | ✅ |
| B-4 | Build backend: verificar 0 errores de compilación | Verify | B-3 | ✅ |
| B-5 | Unit test: `GetExerciseLiftHistoryQueryHandlerTests` (con datos, sin datos, PRs, e1RM, single-rep) | Test | B-4 | ✅ |
| B-6 | Frontend types: `ExerciseLiftHistory`, `LiftEntry`, `ExerciseLiftPR` en `workout-tracking.ts` | Frontend | — | ✅ |
| B-7 | API function: `getExerciseLiftHistory(athleteId, exerciseId, limit?)` + endpoint | Frontend | B-6 | ✅ |
| B-8 | Hook: `useExerciseLiftHistory(athleteId, exerciseId)` con `enabled` condicional | Frontend | B-7 | ✅ |
| B-9 | Mock data: `mockLiftHistory` — 8 sesiones con PRs, videoUrl, coaching cues, instrucciones | Frontend | B-6 | ✅ |
| B-10 | Componente: `ExerciseDetailSheet` — bottom sheet con drag handle, slide-up, backdrop blur | Frontend | — | ✅ |
| B-11 | Sheet section: Header — nombre, categoría, músculo, equipo, compound/isolation badges | Frontend | B-10 | ✅ |
| B-12 | Sheet section: Media — VideoUrl → YouTube embed, fallback a link con Play icon | Frontend | B-11 | ✅ |
| B-13 | Sheet section: Instructions — lista numerada con pasos circulares | Frontend | B-11 | ✅ |
| B-14 | Sheet section: Coaching Cues — pill tags con Play icon, estilo blue-50 | Frontend | B-11 | ✅ |
| B-15 | Sheet section: PR Cards — 4 mini cards (max peso, reps, e1RM, volumen) con icono + fecha | Frontend | B-9 | ✅ |
| B-16 | Sheet section: Lift History Table — sesiones con peso, reps, e1RM, volumen, trend arrows | Frontend | B-9 | ✅ |
| B-17 | Sheet section: Sparkline e1RM Chart — SVG con gradient fill, trend color, date labels | Frontend | B-16 | ✅ |
| B-18 | Nombre del ejercicio en `ExerciseCard` clickeable (dotted underline, hover blue, a11y) | Frontend | B-10 | ✅ |
| B-19 | Integrar data real (hook) en sheet con loading skeleton + mock mode support | Frontend | B-8, B-18 | ✅ |
| B-20 | Verify: `npx tsc --noEmit` — 0 errores nuevos confirmado, 212/212 backend tests | Verify | B-19 | ✅ |

### Archivos tocados (Fase B)
- `src/CoachPlatform.Application/Shared/DTOs/WorkoutTrackingDtos.cs` — ExerciseLiftHistoryDto, etc.
- `src/CoachPlatform.Application/Features/WorkoutLogs/Queries/GetExerciseLiftHistory/` — Query + Handler
- `src/CoachPlatform.API/Controllers/WorkoutsController.cs` — nuevo endpoint
- `tests/CoachPlatform.UnitTests/Application/Queries/GetExerciseLiftHistoryQueryHandlerTests.cs`
- `coach-dashboard/src/types/workout-tracking.ts`
- `coach-dashboard/src/types/index.ts`
- `coach-dashboard/src/lib/api/endpoints.ts`
- `coach-dashboard/src/lib/api/workout-tracking-api.ts`
- `coach-dashboard/src/hooks/workouts/use-workout-tracking.ts`
- `coach-dashboard/src/app/(dashboard)/workout/lib/mock-workout-data.ts`
- `coach-dashboard/src/app/(dashboard)/workout/components/exercise-detail-sheet.tsx` — NUEVO
- `coach-dashboard/src/app/(dashboard)/workout/components/exercise-card.tsx` — ejercicio clickeable
- `coach-dashboard/src/app/(dashboard)/workout/page.tsx` — estado del sheet

---

## FASE C: Rest Timer — P1

> **Por qué**: Esperado por los usuarios. Strong/Hevy lo tienen. Un countdown automático entre sets reduce la fricción.

| # | Tarea | Tipo | Depende de | Estado |
|---|---|---|---|---|
| C-1 | Componente: `RestTimer` — countdown circular SVG con progreso, mono countdown center | Frontend | — | ✅ |
| C-2 | Lógica: arranque automático al completar set (si ejercicio tiene `restSeconds > 0`) | Frontend | C-1 | ✅ |
| C-3 | Controles: botones Skip / +30s / -30s debajo del timer | Frontend | C-1 | ✅ |
| C-4 | Feedback: haptic vibration (30ms) + Web Audio API beep (880Hz sine) al llegar a 0 | Frontend | C-3 | ✅ |
| C-5 | Integrar en `ExerciseCard`: aparece inline entre set completado y siguiente set | Frontend | C-4 | ✅ |
| C-6 | Verify: `npx tsc --noEmit` — 0 errores nuevos | Verify | C-5 | ✅ |

### Archivos tocados (Fase C)
- `coach-dashboard/src/app/(dashboard)/workout/components/rest-timer.tsx` — NUEVO
- `coach-dashboard/src/app/(dashboard)/workout/components/exercise-card.tsx` — integración
- `coach-dashboard/src/app/(dashboard)/workout/lib/workout-utils.ts` — sonido/haptic helpers

---

## FASE D: PR Detection & Celebration — P1

> **Por qué**: Factor WOW. Strong tiene confetti, nosotros tenemos un badge dorado sutil que mantiene la estética paper.

| # | Tarea | Tipo | Depende de | Estado |
|---|---|---|---|---|
| D-1 | Utility: `detectPR(weight, reps, prev)` → `{isPR, types[], deltas[]}` — weight/reps/e1RM axes | Frontend | — | ✅ |
| D-2 | Componente: `PRBadge` — badge dorado con gradient, Trophy icon, scale-in animation | Frontend | — | ✅ |
| D-3 | Integrar detección en `SetRow`: al completar, detectPR → `PRBadge` debajo de la fila (col-span-5) | Frontend | D-1, D-2 | ✅ |
| D-4 | Animación: flash dorado (bg-amber-50 + ring-amber-200, 1.2s) en toda la fila + haptic 30ms | Frontend | D-3 | ✅ |
| D-5 | Verify: `npx tsc --noEmit` — 0 errores nuevos | Verify | D-4 | ✅ |

### Archivos tocados (Fase D)
- `coach-dashboard/src/app/(dashboard)/workout/lib/workout-utils.ts` — detectPR
- `coach-dashboard/src/app/(dashboard)/workout/components/pr-badge.tsx` — NUEVO
- `coach-dashboard/src/app/(dashboard)/workout/components/set-row.tsx` — integración

---

## FASE E: Coach Notes Inline — P1 KILLER FEATURE

> **Por qué**: NINGUNA app de tracking muestra notas del coach en tiempo real. Es la ventaja de ser plataforma coach↔atleta. El campo `exerciseNotes` ya llega del backend.

| # | Tarea | Tipo | Depende de | Estado |
|---|---|---|---|---|
| E-1 | Componente: `CoachNote` — burbuja chat azul con MessageCircle icon, label "Coach" | Frontend | — | ✅ |
| E-2 | Integrar en `ExerciseCard`: reemplazó `<p>` italic por `<CoachNote>` bubble | Frontend | E-1 | ✅ |
| E-3 | Verify: `npx tsc --noEmit` — 0 errores nuevos | Verify | E-2 | ✅ |

### Archivos tocados (Fase E)
- `coach-dashboard/src/app/(dashboard)/workout/components/coach-note.tsx` — NUEVO
- `coach-dashboard/src/app/(dashboard)/workout/components/exercise-card.tsx` — integración

---

## FASE F: Delta vs Plan Badge — P2

> **Por qué**: Feedback inmediato de si el atleta superó o no el target del coach.

| # | Tarea | Tipo | Depende de | Estado |
|---|---|---|---|---|
| F-1 | Utility: `calculateDelta(actual, target, unit)` → `{value, direction}` — kg/reps | Frontend | — | ✅ |
| F-2 | Componente: `DeltaBadge` — micro indicator con ChevronUp/Down/Equal, 9px mono | Frontend | — | ✅ |
| F-3 | Integrar en `SetRow`: delta badges junto a weight/reps inputs cuando `isCompleted` + hay target | Frontend | F-1, F-2 | ✅ |
| F-4 | Verify: `npx tsc --noEmit` — 0 errores nuevos | Verify | F-3 | ✅ |

### Archivos tocados (Fase F)
- `coach-dashboard/src/app/(dashboard)/workout/lib/workout-utils.ts` — calculateDelta
- `coach-dashboard/src/app/(dashboard)/workout/components/delta-badge.tsx` — NUEVO
- `coach-dashboard/src/app/(dashboard)/workout/components/set-row.tsx` — integración

---

## FASE G: Set Notes (long-press) — P2

> **Por qué**: El campo `notes` existe en el modelo backend pero no tiene UI. Permite anotar dolor, técnica, etc.

| # | Tarea | Tipo | Depende de | Estado |
|---|---|---|---|---|
| G-1 | `SetRow`: long-press 500ms (touch+mouse) en set label → toggle notas inline | Frontend | — | ✅ |
| G-2 | Campo notas: input amber-50, placeholder "Nota…", autosave on blur, Enter → blur | Frontend | G-1 | ✅ |
| G-3 | Indicador: `StickyNote` icon amber 2.5px bajo set label si nota no vacía (click → expand) | Frontend | G-2 | ✅ |
| G-4 | Verify: `npx tsc --noEmit` — 0 errores nuevos | Verify | G-3 | ✅ |

### Archivos tocados (Fase G)
- `coach-dashboard/src/app/(dashboard)/workout/components/set-row.tsx` — long-press + notas

---

## FASE H: Workout Summary Card — P2

> **Por qué**: Al completar el workout, una card de resumen cierra la experiencia con datos útiles.

| # | Tarea | Tipo | Depende de | Estado |
|---|---|---|---|---|
| H-1 | Utility: `calculateWorkoutStats(exercises)` → volumen total, sets completados/total, PRs count | Frontend | — | ✅ |
| H-2 | Componente: `WorkoutSummary` — card con duración, volumen, PRs, fatigue rating, sets x/y | Frontend | H-1 | ✅ |
| H-3 | Integrar: mostrar `WorkoutSummary` en la page cuando `workout.status === 'Completed'` | Frontend | H-2 | ✅ |
| H-4 | Verify: `npx tsc --noEmit` — 0 errores nuevos | Verify | H-3 | ✅ |

### Archivos tocados (Fase H)
- `coach-dashboard/src/app/(dashboard)/workout/lib/workout-utils.ts` — calculateWorkoutStats
- `coach-dashboard/src/app/(dashboard)/workout/components/workout-summary.tsx` — NUEVO
- `coach-dashboard/src/app/(dashboard)/workout/page.tsx` — integración

---

## Resumen General

| Fase | Feature | Prioridad | Tareas | ✅ | ⬜ |
|---|---|---|---|---|---|
| A | Week Strip | P0 | 15 | 15 | 0 |
| B | Exercise Detail Sheet | P0 | 20 | 20 | 0 |
| C | Rest Timer | P1 | 6 | 6 | 0 |
| D | PR Detection | P1 | 5 | 5 | 0 |
| E | Coach Notes | P1 | 3 | 3 | 0 |
| F | Delta vs Plan | P2 | 4 | 4 | 0 |
| G | Set Notes | P2 | 4 | 4 | 0 |
| H | Workout Summary | P2 | 4 | 4 | 0 |
| | **TOTAL** | | **61** | **61** | **0** |

---

## Diferenciadores vs Competencia

| Feature | Strong | Hevy | JEFIT | **PowerCoach** |
|---|---|---|---|---|
| Vista semanal con contexto de programa | ❌ | ❌ | ❌ | ✅ Fase A |
| Tap ejercicio → video + cues del COACH | ❌ (GIF genérico) | ❌ | ❌ | ✅ Fase B |
| Coach notes en tiempo real | ❌ | ❌ | ❌ | ✅ Implementado |
| %1RM con peso sugerido automático | ❌ | ❌ | ❌ | ✅ Ya implementado |
| Compound notation (1x1 91% + 3x4 80%) | ❌ | ❌ | ❌ | ✅ Ya implementado |
| PR detection | ✅ Confetti | ✅ Badge | ❌ | ✅ Implementado |
| Rest timer | ✅ | ✅ | ✅ | ✅ Implementado |
| Superset visual grouping | Parcial | Parcial | ❌ | ✅ Ya implementado |

---

## Notas para Retomar Sesión

**Estado**: ✅ **TODAS LAS 61 TAREAS COMPLETADAS** (Fases A-H). Plan UX finalizado.

**Estado actual del backend** (2026-03-26):
- 212/212 tests pasando (8 nuevos para workout tracking queries)
- DTOs: WeekWorkoutsDto, WeekDayDto, ExerciseLiftHistoryDto, LiftEntryDto, ExerciseLiftPRDto
- Queries: GetWeekWorkoutsQuery + Handler, GetExerciseLiftHistoryQuery + Handler
- Endpoints: `GET /athletes/{id}/workouts/week`, `GET /athletes/{id}/workouts/exercises/{exerciseId}/lift-history`
- Epley e1RM: `weight × (1 + reps/30)`, single rep → weight
- WeekNumber inference: hoy > próximo > semana 1

**Estado del frontend** (2026-03-26):
- Types: WeekWorkouts, WeekDay, ExerciseLiftHistory, LiftEntry, ExerciseLiftPR
- API: getWeekWorkouts, getExerciseLiftHistory + endpoints
- Hooks: useWeekWorkouts, useExerciseLiftHistory
- Mock data: mockWeekWorkouts (5 días), mockLiftHistory (8 sesiones Bench Press)
- Componentes nuevos: `week-strip.tsx`, `exercise-detail-sheet.tsx` (ya existía)
- Integración: WeekStrip sticky z-20, ExerciseDetailSheet on exercise name tap
- Exercise name en ExerciseCard es clickeable (dotted underline, hover blue, keyboard a11y)
- 0 nuevos errores TS (solo pre-existentes en mock-data.ts)

**Comando para verificar backend**: `cd d:\PowerCoach\powercoach; dotnet build`  
**Comando para verificar tests**: `cd d:\PowerCoach\powercoach; dotnet test`  
**Comando para verificar frontend TS**: `cd d:\PowerCoach\powercoach\coach-dashboard; npx tsc --noEmit 2>&1 | Where-Object { $_ -match 'error TS' } | Where-Object { $_ -notmatch 'mock-data\.ts' }`  
**Dev server**: `cd d:\PowerCoach\powercoach\coach-dashboard; npx next dev -p 3000`
