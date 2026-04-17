# CoachPlatform — Auditoría Arquitectónica Completa

**Fecha**: 2026-03-24  
**Alcance**: Análisis completo del código existente (backend + frontend)  
**Última actualización**: F-015 Phase 1 (Domain Extension)

---

## 1. ARQUITECTURA GENERAL

### Capas

| Capa | Proyecto | Responsabilidad |
|------|----------|-----------------|
| **Domain** | `CoachPlatform.Domain` | Entidades, Value Objects, Enums, Interfaces de repositorio |
| **Application** | `CoachPlatform.Application` | CQRS (Commands/Queries via MediatR), DTOs, Validators, Pipeline Behaviors |
| **Infrastructure** | `CoachPlatform.Infrastructure` | EF Core 9 + PostgreSQL, Repositories, Services (Tenant, CurrentUser) |
| **API** | `CoachPlatform.API` | Controllers REST, Middleware (Exception, Tenant), Swagger |
| **Frontend** | `coach-dashboard` | Next.js 14, React 18, TanStack Query, Axios, shadcn/ui |

### Dependencias
```
API → Application → Domain
API → Infrastructure → Application → Domain
Frontend → API (HTTP/REST)
```

### Patrones

| Patrón | Implementación |
|--------|---------------|
| **Clean Architecture** | 4 capas con dependencias unidireccionales |
| **CQRS** | Commands y Queries separados via MediatR |
| **Repository Pattern** | 7 repositorios tipados + `IUnitOfWork` |
| **Value Objects** | `Email`, `PersonName`, `Money` (inmutables, con igualdad por valor) |
| **Aggregate Roots** | 7 agregados (`Coach`, `Athlete`, `Application`, `Plan`, `Exercise`, `ProgramTemplate`, `AthleteProgram`) |
| **Multi-tenancy** | Por Coach, `AsyncLocal`-based `TenantService` + pipeline behavior |
| **Pipeline Behaviors** | Validación (FluentValidation), Logging, Tenant Validation |

### Observaciones Arquitectónicas

- **Los handlers bypasean repositorios**: La mayoría de Commands/Queries usan `IApplicationDbContext` directamente en vez de las interfaces de repositorio. Los repositorios se registran en DI pero solo se usan en algunos handlers de Features antiguas.
- **No hay global query filters**: El filtrado por tenant se hace manualmente en cada handler, no hay filtro global de EF Core.
- **Faltan repositorios para Training Programs**: No existe `IProgramTemplateRepository`, `IAthleteProgramRepository`, ni `IExerciseRepository`. Estas features usan `DbContext` directo.

---

## 2. DOMINIO

### Entidades — Estado Completo

| Entidad | Aggregate Root | Responsabilidad | Estado |
|---------|---------------|-----------------|--------|
| `Coach` | ✅ | Tenant principal, gestiona atletas/planes/ejercicios | ✅ Completa |
| `Athlete` | ✅ | Atleta del coach, lifecycle completo (Active→Graduated) | ✅ Completa |
| `Application` | ✅ | Postulación de aspirante, state machine (Pending→Accepted/Rejected) | ✅ Completa |
| `Plan` | ✅ | Tipo de membresía con precio, duración, features | ✅ Completa |
| `Exercise` | ✅ | Catálogo de ejercicios por coach | ✅ Completa |
| `ProgramTemplate` | ✅ | Plantilla de programa reutilizable | ✅ Completa |
| `AthleteProgram` | ✅ | Programa asignado a un atleta | ✅ Completa |
| `ProgramWeekTemplate` | ❌ | Semana dentro de un programa | ✅ Completa |
| `ProgramDayTemplate` | ❌ | Día dentro de una semana | ✅ Completa |
| `ProgramExerciseTemplate` | ❌ | Ejercicio prescrito dentro de un día | ✅ Completa (con 7 campos F-013) |
| `AthleteWorkout` | ❌ | Workout generado para un atleta | ✅ Extendida F-015 (Status, StartedAt, SkippedReason + Start/Skip methods) |
| `AthleteExerciseLog` | ❌ | Log de un set ejecutado | ✅ Extendida F-015 (IsCompleted, TargetReps, TargetWeight, SkippedReason + Complete/Skip/SetTargets methods) |
| `AthleteMaxLift` | ❌ | Registro de 1RM (tested o estimado) | ✅ Completa |
| `Subscription` | ❌ | Relación Athlete↔Plan con estado y fechas | ✅ Completa |
| `Payment` | ❌ | Registro de pago con state machine | ✅ Completa |
| `CheckIn` | ❌ | Check-in periódico con métricas + feedback del coach | ✅ Completa |
| `TrainingCycle` | ❌ | Ciclo de entrenamiento (nombre, duración, fechas) | ⚠️ Básica — solo metadata, sin relación con `ProgramTemplate` |
| `WorkoutLog` | ❌ | Log simple de workout (legacy, pre-programas) | ⚠️ Legacy — duplica funcionalidad de `AthleteExerciseLog` |

### Relaciones Clave
```
Coach ──1:N──→ Athlete ──1:N──→ Subscription ──1:N──→ Payment
                       ──1:N──→ CheckIn
                       ──1:N──→ TrainingCycle
                       ──1:N──→ WorkoutLog (legacy)
Coach ──1:N──→ Application
Coach ──1:N──→ Plan ──1:N──→ Subscription
Coach ──1:N──→ Exercise
Coach ──1:N──→ ProgramTemplate ──1:N──→ ProgramWeekTemplate ──1:N──→ ProgramDayTemplate ──1:N──→ ProgramExerciseTemplate
                                                                                                     └──→ Exercise (FK)
AthleteProgram ──→ Athlete + ProgramTemplate
               ──1:N──→ AthleteWorkout ──1:N──→ AthleteExerciseLog ──→ Exercise
Athlete ──1:N──→ AthleteMaxLift ──→ Exercise
```

---

## 3. PROGRAM BUILDER — Análisis Detallado

### Modelo de Datos

| Nivel | Entidad | Properties Clave |
|-------|---------|-----------------|
| **Programa** | `ProgramTemplate` | Name, Description, DurationWeeks, IsActive |
| **Semana** | `ProgramWeekTemplate` | WeekNumber, Name?, Notes? |
| **Día** | `ProgramDayTemplate` | DayNumber (1-7), Name?, Focus (DayFocus), Notes? |
| **Ejercicio** | `ProgramExerciseTemplate` | ExerciseId, Sets, **Reps (string)**, TargetRpe, RestSeconds, Order, ExerciseType, PercentageRM, RawNotation, Weight, EmomConfigJson, TempoConfigJson, SupersetConfigJson |

### ¿Soporta Progresión por Semana?

**Parcialmente.** El modelo permite definir diferentes prescripciones para el mismo ejercicio en distintas semanas (e.g., Semana 1: 4x8@70%, Semana 2: 4x6@75%). Esto funciona porque cada `ProgramWeekTemplate` tiene sus propios `ProgramDayTemplate` → `ProgramExerciseTemplate`.

**Limitación:** No hay un concepto explícito de "progresión". La progresión se implementa duplicando ejercicios en cada semana con valores diferentes. El frontend builder sí tiene un `ProgressionGenerator` que auto-genera prescripciones across weeks, pero estos datos se aplanan al guardar — no hay metadata de progresión almacenada en backend.

### ¿Soporta Progresión por Ejercicio?

**Sí**, a nivel de UI. El frontend tiene:
- `ExerciseProgressionView`: vista cross-week agrupada por ejercicio
- `ProgressionTable`: tabla con prescripciones por semana
- `ProgressionGenerator`: generador automático con templates (hypertrophy, strength, peaking, deload) y patrones (linear, wave, step, peak)

**Limitación:** El backend no almacena el tipo de progresión ni los parámetros del generador. Si se regenera, se pierden los ajustes manuales hechos después.

### Limitaciones del Modelo

| Limitación | Impacto | Severidad |
|-----------|---------|-----------|
| **No hay `ProgramSet` (Set individual)** | El campo `Sets: int` indica cantidad. No hay variación set-a-set (e.g., "Set 1: 80kg, Set 2: 85kg, Set 3: 90kg") | 🟡 Media |
| **Sin versionamiento** | Editar un programa activo puede afectar atletas asignados | 🔴 Alta |
| **Sin template inheritance** | No se pueden crear variantes de un programa base | 🟢 Baja |
| **JSON configs no tipadas** | EmomConfig/TempoConfig/SupersetConfig son `string?` (JSON), sin validación backend | 🟡 Media |
| **DayNumber fijo 1-7** | No permite programas con más de 7 días por semana ni días opcionales | 🟢 Baja |
| **TrainingCycle no vinculado** | `TrainingCycle` es independiente de `ProgramTemplate` — no hay relación macro-meso-micro | 🟡 Media |

---

## 4. API — Endpoints por Módulo

### Applications (5 endpoints)
| Verbo | Ruta | Acción |
|-------|------|--------|
| GET | `/api/applications` | Listar (paginado, filtros) |
| GET | `/api/applications/{id}` | Detalle |
| POST | `/api/applications` | Crear |
| POST | `/api/applications/{id}/approve` | Aprobar → crea Athlete |
| POST | `/api/applications/{id}/reject` | Rechazar |

### Athletes (8 endpoints)
| Verbo | Ruta | Acción |
|-------|------|--------|
| GET | `/api/athletes` | Listar (paginado, filtros) |
| GET | `/api/athletes/{id}` | Detalle |
| POST | `/api/athletes` | Crear |
| PUT | `/api/athletes/{id}` | Actualizar |
| POST | `/api/athletes/{id}/deactivate` | Desactivar |
| GET | `/api/athletes/{id}/max-lifts` | Max lifts |
| POST | `/api/athletes/{id}/max-lifts` | Registrar max lift |
| GET | `/api/athletes/{id}/exercise-history/{exerciseId}` | Historial de ejercicio |

### Programs (16 endpoints)
| Verbo | Ruta | Acción |
|-------|------|--------|
| GET | `/api/programs` | Listar |
| GET | `/api/programs/{id}` | Detalle completo (nested) |
| POST | `/api/programs` | Crear |
| PUT | `/api/programs/{id}` | Update metadata |
| PUT | `/api/programs/{id}/full` | **Bulk save** (replace-all) |
| POST | `/api/programs/{pid}/weeks` | Add week |
| DELETE | `/api/programs/{pid}/weeks/{wid}` | Delete week |
| POST | `/api/programs/{pid}/weeks/{wid}/days` | Add day |
| DELETE | `/api/programs/{pid}/days/{did}` | Delete day |
| POST | `/api/programs/{pid}/weeks/{wid}/days/{did}/exercises` | Add exercise |
| DELETE | `/api/programs/{pid}/exercises/{eid}` | Delete exercise |
| POST | `/api/athletes/{aid}/assign-program` | Asignar programa |
| GET | `/api/athletes/{aid}/program` | Programa actual del atleta |
| GET | `/api/athletes/{aid}/workouts/{wid}` | Workout específico |
| POST | `/api/athletes/{aid}/workouts/{wid}/log` | Log workout |
| GET | `/api/athletes/{aid}/workouts` | Listar workouts (legacy) |

### Payments (3 endpoints)
| Verbo | Ruta | Acción |
|-------|------|--------|
| GET | `/api/payments` | Listar (filtros) |
| POST | `/api/payments` | Registrar pago |
| GET | `/api/athletes/{aid}/payments` | Pagos por atleta |

### CheckIns (2 endpoints)
| Verbo | Ruta | Acción |
|-------|------|--------|
| POST | `/api/checkins` | Crear check-in |
| GET | `/api/athletes/{aid}/checkins` | Listar por atleta |

### Plans (4 endpoints)
| Verbo | Ruta | Acción |
|-------|------|--------|
| GET | `/api/plans` | Listar |
| POST | `/api/plans` | Crear |
| PUT | `/api/plans/{id}` | Actualizar |
| POST | `/api/plans/{id}/deactivate` | Desactivar |

### Subscriptions (3 endpoints)
| Verbo | Ruta | Acción |
|-------|------|--------|
| POST | `/api/subscriptions` | Crear |
| POST | `/api/subscriptions/{id}/cancel` | Cancelar |
| GET | `/api/athletes/{aid}/subscriptions` | Listar por atleta |

### Otros (3 endpoints)
| Verbo | Ruta | Acción |
|-------|------|--------|
| POST | `/api/trainingcycles` | Crear |
| GET | `/api/athletes/{aid}/training-cycles` | Listar por atleta |
| POST | `/api/workouts` | Log workout (legacy) |

### Endpoints Faltantes

| Qué falta | Módulo | Prioridad |
|-----------|--------|-----------|
| `GET /api/coaches/{id}` | Coaches | 🔴 No hay CRUD de coach en API |
| `PUT /api/checkins/{id}` | CheckIns | 🟡 No se puede editar un check-in |
| `POST /api/checkins/{id}/feedback` | CheckIns | 🟡 No hay endpoint para feedback del coach |
| `GET /api/plans/{id}` | Plans | 🟡 No hay detalle individual |
| `GET /api/subscriptions/{id}` | Subscriptions | 🟢 No hay detalle individual |
| `PUT /api/exercises/{id}` | Exercises | 🟡 No hay update de ejercicio |
| Reordenar exercises/days/weeks | Programs | 🟡 Solo delete+re-add para reordenar |

### Endpoints Redundantes

| Redundancia | Detalle |
|-------------|---------|
| `POST /api/workouts` vs `POST /athletes/{aid}/workouts/{wid}/log` | Dos sistemas de logging: legacy (`WorkoutLog`) y program-based (`AthleteExerciseLog`). El legacy debería deprecarse. |

---

## 5. FRONTEND

### Páginas

| Ruta | Estado | Conectada a API |
|------|--------|-----------------|
| `/dashboard` | ✅ UI completa | ❌ **Datos hardcoded** — no llama API |
| `/athletes` | ✅ Tabla paginada | ✅ `useAthletes()` |
| `/athletes/{id}` | ❌ **No existe** (`page.tsx` falta) | — |
| `/applications` | ✅ Tabla paginada | ✅ `useApplications()` |
| `/applications/{id}` | ❌ **No existe** (`page.tsx` falta) | — |
| `/programs` | ✅ Tabla paginada | ✅ `usePrograms()` |
| `/programs/{id}` | ❌ **No existe** (`page.tsx` falta) | — |
| `/programs/{id}/builder` | ✅ Editor completo (914 líneas) | ✅ Save real via `programsApi.saveFull()` |
| `/exercises` | ✅ Tabla (client-side filter) | ✅ `useExercises()` |
| `/settings` | ✅ Perfil + logo | ⚠️ Solo localStorage |

### Módulos por Completitud

| Módulo | UI | Hooks | API Client | Estado |
|--------|-----|-------|-----------|--------|
| **Athletes** | Lista ✅, Detalle ❌ | ✅ 8 hooks | ✅ | 🟡 Falta detail page |
| **Applications** | Lista ✅, Detalle ❌ | ✅ 5 hooks | ✅ | 🟡 Falta detail page |
| **Programs** | Lista ✅, Builder ✅ | ✅ 15 hooks | ✅ | 🟡 Falta detail view |
| **Exercises** | Lista ✅ | ✅ 3 hooks | ✅ | 🟡 No edit/update |
| **Subscriptions** | ❌ No hay UI | ❌ | ⚠️ Solo endpoints definidos | 🔴 No implementado |
| **Payments** | ❌ No hay UI | ❌ | ⚠️ Solo endpoints definidos | 🔴 No implementado |
| **Check-Ins** | ❌ No hay UI | ❌ | ⚠️ Solo endpoints definidos | 🔴 No implementado |
| **Training Cycles** | ❌ No hay UI | ❌ | ❌ | 🔴 No implementado |
| **Dashboard** | ✅ UI | ❌ | ❌ | 🟡 Hardcoded |

---

## 6. INTEGRACIÓN FRONTEND ↔ BACKEND

### Mismatches Críticos

| Tipo | Frontend | Backend | Severidad |
|------|----------|---------|-----------|
| **ExerciseCategory enum** | `'Strength' \| 'Cardio' \| 'Flexibility' \| 'Balance' \| 'Other'` | `Squat \| Bench \| Deadlift \| OverheadPress \| Row \| Pull \| Accessory \| Cardio \| Mobility \| Core` | 🔴 **Completamente diferentes** |
| **MuscleGroup enum** | 7 valores | 13 valores (faltan Quadriceps, Hamstrings, Glutes, Biceps, Triceps, Calves) | 🟡 |
| **DayFocus enum** | `UpperBody`/`LowerBody` | `Upper`/`Lower` (naming); faltan Squat, Bench, Deadlift, Hypertrophy, Accessories, Recovery, Competition | 🟡 |
| **Athlete type** | `city` (no existe en BE), `trainingGoals` (BE: `Goals`), `bodyWeight` (BE: `Weight`) | 8 propiedades BE no en FE | 🟡 |
| **Application type** | `dateOfBirth`, `city`, `medicalConditions` (no en BE); `trainingGoals`→`Goals`, `notes`→`CoachNotes`, `submittedAt`→`CreatedAt` | 11 propiedades BE no en FE | 🟡 |
| **Exercise type** | `muscleGroup` | BE: `PrimaryMuscleGroup` | 🟡 |

### Program Builder — Bien alineado ✅

Las types del builder (`ProgramTemplate`, `ProgramWeek`, `ProgramDay`, `ProgramExercise`) están **correctamente alineadas** tras F-013/F-014. Los campos F-013 (ExerciseType, PercentageRM, etc.) coinciden, y la conversión `reps string ↔ repsMin/repsMax` funciona via helpers.

### Mock Data en Dev

El builder usa `USE_MOCK_DATA = process.env.NODE_ENV === 'development'` — **en desarrollo nunca golpea el API real**. Esto oculta posibles errores de integración.

---

## 7. TESTING

### Backend: 166 tests pasando

| Categoría | Tests | Cobertura |
|-----------|-------|-----------|
| Value Objects (Email, Money) | 13 | ✅ Buena |
| Entities (Athlete, Application, Exercise) | 42 | ✅ Buena |
| Commands (15 handlers) | 82 | ✅ Cobertura de happy path + edge cases |
| Queries (1 handler) | 7 | 🔴 **Solo 1 query testeada** de ~12 |
| Integration Tests | 0 | 🔴 **Proyecto vacío** |

### Frontend: 244 tests pasando

| Categoría | Tests | Cobertura |
|-----------|-------|-----------|
| Set Notation Parser | ~45 | ✅ Excelente |
| Weight Calculator | ~30 | ✅ Excelente |
| Calendar View Utils | ~30 | ✅ Buena |
| Day Centric Utils | ~18 | ✅ Buena |
| Excel Export Utils | ~30 | ✅ Buena |
| Progression Utils | ~30 | ✅ Buena |
| PDF Utils | ~22 | ✅ Buena |
| Type Tests | 7 | ✅ Básica |
| **Component tests** | 0 | 🔴 **Ningún componente React testeado** |
| **Hook tests** | 0 | 🔴 **Ningún hook testeado** |
| **Page tests** | 0 | 🔴 **Ninguna página testeada** |

### Gaps de Testing

| Qué falta | Prioridad |
|-----------|-----------|
| Integration tests backend (WebApplicationFactory) | 🔴 |
| Query handler tests (solo 1/12 testeada) | 🔴 |
| Tests para `SaveProgramTemplateCommandHandler` (el handler más complejo) | 🔴 |
| Component tests React (builder components) | 🟡 |
| Hook tests (mutations, cache invalidation) | 🟡 |
| E2E tests (Playwright/Cypress) | 🟢 |

---

## 8. PROBLEMAS DETECTADOS

### Inconsistencias

| # | Problema | Severidad |
|---|---------|-----------|
| 1 | **`ExerciseCategory` enum completamente diferente entre FE y BE** — el frontend no puede mostrar correctamente las categorías reales | 🔴 Critical |
| 2 | **`WorkoutLog` es legacy y duplica `AthleteExerciseLog`** — dos sistemas de logging en paralelo | 🟡 High |
| 3 | **Dashboard con datos hardcoded** — no refleja datos reales | 🟡 High |
| 4 | **Handlers usan `DbContext` directo** en vez de repositorios — rompe el patrón Repository y dificulta testing | 🟡 Medium |
| 5 | **No hay global query filter por tenant** — cada handler filtra manualmente, propenso a fugas de datos | 🔴 Critical |
| 6 | **Builder usa mock data en dev** — oculta errores de integración | 🟡 Medium |

### Malas Prácticas

| # | Problema | Impacto |
|---|---------|---------|
| 1 | **Sin autenticación** — `CoachProvider` usa coach hardcoded, `ICurrentUserService` nunca se usa realmente | Alto — cualquiera accede a todo |
| 2 | **`next.config.mjs` vacío** — sin headers de seguridad, sin API proxy, sin image optimization | Medio |
| 3 | **Sin error boundaries** — no hay `error.tsx` en ningún route segment | Medio |
| 4 | **Mixed languages** (español/inglés en UI) | Bajo |
| 5 | **Exercises page no paginada server-side** — carga todo al cliente | Bajo (por ahora) |
| 6 | **JSON config strings sin validación backend** (EmomConfig, TempoConfig, SupersetConfig) | Medio |

### Riesgos Futuros

| Riesgo | Descripción |
|--------|-------------|
| **Data leak entre tenants** | Sin global query filter, un bug en cualquier handler nuevo puede exponer datos de otro coach |
| **Programa activo corrupto** | No hay versionamiento — editar un ProgramTemplate afecta AthletePrograms activos |
| **Scaling** | Sin caché (solo staleTime de React Query), sin rate limiting, sin pagination en exercises |

---

## 9. GAPS FUNCIONALES

### No Implementados

| Feature | Backend | Frontend | Prioridad |
|---------|---------|----------|-----------|
| **Autenticación/Autorización** | `ICurrentUserService` existe pero no conectado a Identity/JWT real | Sin login page, sin auth flow | 🔴 |
| **Athlete Detail Page** | API existe (`GET /athletes/{id}`) | ❌ No hay `page.tsx` | 🔴 |
| **Application Detail Page** | API existe (`GET /applications/{id}`) | ❌ No hay `page.tsx` | 🔴 |
| **Program Detail/Overview Page** | API existe (`GET /programs/{id}`) | ❌ No hay `page.tsx` | 🟡 |
| **Subscriptions UI** | ✅ CRUD completo en backend | ❌ Sin API client, hooks, ni páginas | 🟡 |
| **Payments UI** | ✅ CRUD en backend | ❌ Sin pages | 🟡 |
| **Check-Ins UI** | ✅ Backend (crear + listar) | ❌ Sin pages | 🟡 |
| **Coach Feedback System** | Entity soporta (`CheckIn.AddCoachFeedback`) | ❌ Sin endpoint ni UI | 🟡 |
| **Notifications** | ❌ Nada | ❌ Nada | 🟢 |
| **Media/Content Upload** | `ProfilePictureUrl`/`PhotoUrls` como strings | Solo logo local (base64) | 🟢 |
| **Athlete Workout Tracking UI** | ✅ Backend completo | ❌ Sin UI para que atleta registre workouts | 🟡 |
| **Performance History/Analytics** | `ExerciseHistory` query existe | ❌ Sin dashboard de analytics | 🟡 |
| **Exercise Update** | Entity soporta `Update()` | ❌ Sin API endpoint ni UI | 🟡 |
| **Training Cycles UI** | ✅ Backend (crear + listar) | ❌ Sin pages | 🟢 |

---

## 10. RECOMENDACIONES — Próximos Pasos

### Backend — Prioridad Alta

| # | Acción | Justificación |
|---|--------|---------------|
| 1 | **Implementar Global Query Filters por CoachId** | Previene data leaks. `modelBuilder.Entity<X>().HasQueryFilter(x => x.CoachId == _tenantService.CoachId)` |
| 2 | **Agregar versionamiento a `ProgramTemplate`** | `Version: int` + snapshot al asignar. Editar un template crea nueva versión sin afectar atletas activos |
| 3 | **Deprecar `WorkoutLog`** | Migrar a `AthleteExerciseLog` y eliminar el sistema legacy |
| 4 | **Agregar endpoint `PUT /api/coaches/{coachId}/exercises/{id}`** | Completar CRUD de ejercicios |
| 5 | **Agregar endpoint `POST /api/checkins/{id}/feedback`** | Permitir feedback del coach |
| 6 | **Agregar query handler tests** | Solo 1/12 queries testeadas |

### Frontend — Prioridad Alta

| # | Acción | Justificación |
|---|--------|---------------|
| 1 | **Crear `/athletes/{id}` detail page** | Enlace roto desde tabla de atletas |
| 2 | **Crear `/applications/{id}` detail page** | Enlace roto desde tabla de applications |
| 3 | **Alinear enums `ExerciseCategory` y `MuscleGroup`** con backend | Mismatch crítico — las categorías mostradas no coinciden con las del backend |
| 4 | **Conectar Dashboard a API real** | Reemplazar datos hardcoded con queries reales |
| 5 | **Eliminar mock data fallback del builder** | O al menos hacerlo configurable, no automático en dev |
| 6 | **Alinear tipos `Athlete` y `Application`** | Resolver naming mismatches (`trainingGoals`→`goals`, `bodyWeight`→`weight`, etc.) |

### Arquitectura — Prioridad Media

| # | Acción | Justificación |
|---|--------|---------------|
| 1 | **Implementar autenticación** | JWT + ASP.NET Identity. Sin esto nada es seguro |
| 2 | **Consolidar handler pattern** | Decidir: o repositorios o `DbContext` directo, no ambos |
| 3 | **Agregar `error.tsx` boundaries** en App Router | Errores no manejados crashean la UI |
| 4 | **Configurar `next.config.mjs`** | API proxy (evitar CORS issues en prod), security headers, image domains |
| 5 | **Implementar integration tests** | Proyecto existe vacío — usar `WebApplicationFactory` |
| 6 | **Vincular `TrainingCycle` con `ProgramTemplate`** | Actualmente son entidades independientes sin relación |
