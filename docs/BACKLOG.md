# CoachPlatform - Product Backlog

> Sistema backend para gestión de atletas, pagos y postulaciones de coach de powerlifting/fitness.
> Arquitectura: Clean Architecture + DDD | Stack: .NET 9, PostgreSQL, Docker

---

## 📊 Resumen del Proyecto

| Métrica | Valor |
|---------|-------|
| **Features Totales** | 15 |
| **Tasks Totales** | 165 |
| **✅ Completadas** | 142 |
| **🟡 En Progreso** | 0 |
| **⬜ Pendientes** | 23 |

**Última actualización**: 2026-03-24

---

## 🎯 Features

---

### F-001: Configuración Inicial del Proyecto
**Estado**: ✅ Completada | **Prioridad**: 🔴 Crítica | **Sprint**: 1

Crear la estructura base de la solución .NET con Clean Architecture.

| ID | Task | Estado | Notas |
|----|------|--------|-------|
| T-001 | Crear solución `CoachPlatform.sln` | ✅ | |
| T-002 | Crear proyecto `CoachPlatform.Domain` | ✅ | Class Library |
| T-003 | Crear proyecto `CoachPlatform.Application` | ✅ | Class Library |
| T-004 | Crear proyecto `CoachPlatform.Infrastructure` | ✅ | Class Library |
| T-005 | Crear proyecto `CoachPlatform.API` | ✅ | ASP.NET Core Web API |
| T-006 | Crear proyecto `CoachPlatform.UnitTests` | ✅ | xUnit |
| T-007 | Crear proyecto `CoachPlatform.IntegrationTests` | ✅ | xUnit |
| T-008 | Configurar referencias entre proyectos | ✅ | |
| T-009 | Instalar paquetes NuGet en todos los proyectos | ✅ | .NET 9 / EF Core 9.x |
| T-010 | Crear archivo `.gitignore` para .NET | ✅ | |

**Criterios de Aceptación**:
- [x] `dotnet build` compila sin errores
- [x] Estructura de carpetas correcta
- [x] Referencias entre proyectos configuradas

---

### F-002: Capa de Dominio
**Estado**: ✅ Completada | **Prioridad**: 🔴 Crítica | **Sprint**: 1

Implementar entidades, value objects, enums e interfaces del dominio.

| ID | Task | Estado | Notas |
|----|------|--------|-------|
| T-011 | Crear `BaseEntity` y `AuditableEntity` | ✅ | Common/ |
| T-012 | Crear `IAggregateRoot` interface | ✅ | Common/ |
| T-013 | Crear Value Object `Email` | ✅ | Con validación regex |
| T-014 | Crear Value Object `Money` | ✅ | Amount + Currency + operaciones |
| T-015 | Crear Value Object `PersonName` | ✅ | FirstName + LastName |
| T-016 | Crear Enum `ApplicationStatus` | ✅ | + Withdrawn |
| T-017 | Crear Enum `PaymentStatus` | ✅ | + Cancelled |
| T-018 | Crear Enum `SubscriptionStatus` | ✅ | + PendingPayment |
| T-019 | Crear Enum `PlanType` | ✅ | + SemiAnnual, Custom |
| T-020 | Crear entidad `Coach` | ✅ | Aggregate Root |
| T-021 | Crear entidad `Athlete` | ✅ | Con CoachId + AthleteStatus |
| T-022 | Crear entidad `Application` | ✅ | Postulación |
| T-023 | Crear entidad `Plan` | ✅ | Tipo de membresía + Features |
| T-024 | Crear entidad `Subscription` | ✅ | Relación Athlete-Plan |
| T-025 | Crear entidad `Payment` | ✅ | Registro de pagos |
| T-026 | Crear entidad `CheckIn` | ✅ | Seguimiento periódico completo |
| T-027 | Crear `IRepository<T>` interface genérica | ✅ | Interfaces/ |
| T-028 | Crear interfaces de repositorios específicos | ✅ | 7 repositorios |
| T-029 | Crear `IUnitOfWork` interface | ✅ | Con transacciones |

**Criterios de Aceptación**:
- [x] Todas las entidades heredan de `AuditableEntity`
- [x] Value Objects son inmutables con validación
- [x] Relaciones entre entidades definidas correctamente

---

### F-003: Capa de Aplicación
**Estado**: ✅ Completado | **Prioridad**: 🔴 Crítica | **Sprint**: 1-2

Implementar DTOs, Commands, Queries y Validators con CQRS ligero.

| ID | Task | Estado | Notas |
|----|------|--------|-------|
| T-030 | Configurar MediatR | ✅ | Pipeline behaviors |
| T-031 | Crear `ValidationBehavior` para MediatR | ✅ | FluentValidation integration |
| T-032 | Crear DTOs de Athletes | ✅ | AthleteDto, CreateAthleteDto |
| T-033 | Crear DTOs de Applications | ✅ | ApplicationDto, etc. |
| T-034 | Crear DTOs de Plans, Subscriptions, Payments | ✅ | + CheckIn, Coach DTOs |
| T-035 | Implementar `CreateAthleteCommand` + Handler | ✅ | Use Case principal |
| T-036 | Implementar `CreateAthleteValidator` | ✅ | FluentValidation |
| T-037 | Implementar `GetAthleteByIdQuery` + Handler | ✅ | |
| T-038 | Implementar `GetAthletesByCoachQuery` + Handler | ✅ | Con filtros y paginación |
| T-039 | Crear `IApplicationDbContext` interface | ✅ | Para DI |

**Criterios de Aceptación**:
- [x] Commands y Queries separados
- [x] Validación automática vía pipeline
- [x] DTOs sin lógica de negocio

---

### F-004: Capa de Infraestructura
**Estado**: ✅ Completada | **Prioridad**: 🔴 Crítica | **Sprint**: 2

Implementar EF Core, DbContext, Configurations y Repositories.

| ID | Task | Estado | Notas |
|----|------|--------|-------|
| T-040 | Crear `ApplicationDbContext` | ✅ | Con auditoría automática |
| T-041 | Crear `CoachConfiguration` (Fluent API) | ✅ | |
| T-042 | Crear `AthleteConfiguration` | ✅ | |
| T-043 | Crear `ApplicationConfiguration` | ✅ | |
| T-044 | Crear `PlanConfiguration` | ✅ | |
| T-045 | Crear `SubscriptionConfiguration` | ✅ | |
| T-046 | Crear `PaymentConfiguration` | ✅ | |
| T-047 | Crear `CheckInConfiguration` | ✅ | |
| T-048 | Implementar `BaseRepository<T>` | ✅ | Genérico |
| T-049 | Implementar repositorios específicos | ✅ | AthleteRepository, etc. |
| T-050 | Implementar `UnitOfWork` | ✅ | |
| T-051 | Crear `DependencyInjection.cs` de Infrastructure | ✅ | AddInfrastructure() |

**Criterios de Aceptación**:
- [x] Todas las entidades mapeadas con Fluent API
- [x] Value Objects convertidos correctamente
- [x] Índices y constraints configurados

---

### F-005: Capa de API
**Estado**: ✅ Completada | **Prioridad**: 🔴 Crítica | **Sprint**: 2

Implementar Controllers, Middleware y configuración de la API.

| ID | Task | Estado | Notas |
|----|------|--------|-------|
| T-052 | Configurar `Program.cs` con DI completo | ✅ | |
| T-053 | Implementar `AthletesController` | ✅ | CRUD completo |
| T-054 | Implementar `ExceptionHandlingMiddleware` | ✅ | Manejo global de errores |
| T-055 | Configurar Swagger/OpenAPI | ✅ | Con ejemplos |
| T-056 | Configurar CORS | ✅ | Para frontend futuro |
| T-057 | Crear `appsettings.json` y `appsettings.Development.json` | ✅ | |

**Criterios de Aceptación**:
- [x] Endpoints responden correctamente
- [x] Swagger documenta todos los endpoints
- [x] Errores devuelven formato consistente

---

### F-006: Testing
**Estado**: ✅ Completada | **Prioridad**: 🟡 Alta | **Sprint**: 2-3

Implementar tests unitarios e integración.

| ID | Task | Estado | Notas |
|----|------|--------|-------|
| T-058 | Configurar proyecto de Unit Tests | ✅ | Fixtures, Moq setup |
| T-059 | Test: `CreateAthleteCommandHandler` | ✅ | Happy path + errores |
| T-060 | Test: Value Objects (Email, Money) | ✅ | Validación |
| T-061 | Test: Entidades de dominio | ✅ | Comportamiento |
| T-062 | Configurar proyecto de Integration Tests | ✅ | WebApplicationFactory |
| T-063 | Test: `AthletesController` endpoints | ✅ | E2E básico |

**Criterios de Aceptación**:
- [x] Cobertura mínima de use cases principales
- [x] Tests pasan en CI/CD
- [x] Mocks correctamente configurados

---

### F-007: Docker y Deployment
**Estado**: ✅ Completada | **Prioridad**: 🟢 Media | **Sprint**: 3

Configurar Docker y preparar para deployment.

| ID | Task | Estado | Notas |
|----|------|--------|-------|
| T-064 | Crear `Dockerfile` multi-stage | ✅ | Optimizado |
| T-065 | Crear `docker-compose.yml` | ✅ | API + PostgreSQL local |
| T-066 | Crear migration inicial | ✅ | EF Core |
| T-067 | Crear script de seed data | ✅ | Coach + Plans iniciales |
| T-068 | Documentar README.md completo | ✅ | Setup, variables, comandos |

**Criterios de Aceptación**:
- [x] `docker-compose up` levanta el sistema completo
- [x] Migrations se aplican automáticamente
- [x] README permite onboarding rápido

---

### F-008: Exercise Library
**Estado**: ✅ Completada | **Prioridad**: 🟡 Alta | **Sprint**: 4

Implementar catálogo de ejercicios por coach para programación de entrenamientos.

| ID | Task | Estado | Notas |
|----|------|--------|-------|
| T-069 | Crear enum `ExerciseCategory` | ✅ | Squat, Bench, Deadlift, etc. |
| T-070 | Crear enum `MuscleGroup` | ✅ | 13 grupos musculares |
| T-071 | Crear entidad `Exercise` (Aggregate Root) | ✅ | Con VideoUrl, Instructions, CoachingCues |
| T-072 | Crear `ExerciseConfiguration` (EF Core) | ✅ | Índices y constraints |
| T-073 | Refactorizar `WorkoutLog` para usar `ExerciseId` | ✅ | FK + nombre denormalizado |
| T-074 | Implementar CQRS completo para Exercises | ✅ | Create, Update, List, GetById |
| T-075 | Crear tests unitarios para Exercise entity | ✅ | 22 tests |

**Criterios de Aceptación**:
- [x] Cada coach tiene su propio catálogo de ejercicios
- [x] WorkoutLog referencia Exercise por Id
- [x] Tests cubren validaciones de dominio

---

### F-009: Multi-tenancy
**Estado**: ✅ Completada | **Prioridad**: 🔴 Crítica | **Sprint**: 4

Implementar aislamiento de datos por coach (tenant).

| ID | Task | Estado | Notas |
|----|------|--------|-------|
| T-076 | Crear `ICurrentUserService` interface | ✅ | Usuario autenticado |
| T-077 | Crear `ITenantService` interface | ✅ | AsyncLocal-based context |
| T-078 | Crear `ITenantRequest` y `IAthleteOwnedRequest` | ✅ | Marker interfaces |
| T-079 | Implementar `TenantValidationBehavior` | ✅ | MediatR pipeline |
| T-080 | Implementar `TenantMiddleware` | ✅ | JWT claims + X-Coach-Id header |
| T-081 | Crear `ForbiddenAccessException` | ✅ | 403 responses |
| T-082 | Aplicar interfaces tenant a Commands/Queries | ✅ | 15 requests actualizados |

**Criterios de Aceptación**:
- [x] Validación automática de tenant en pipeline
- [x] Coach solo accede a sus propios datos
- [x] Athletes validados contra tenant actual

---

### F-010: Training Programs Module
**Estado**: ✅ Completada | **Prioridad**: 🔴 Crítica | **Sprint**: 5

Implementar módulo completo de programas de entrenamiento para coaches.

| ID | Task | Estado | Notas |
|----|------|--------|-------|
| T-083 | Crear enum `ProgramStatus` | ✅ | Active, Completed, Paused, Cancelled |
| T-084 | Crear enum `DayFocus` | ✅ | Push, Pull, Legs, Upper, Lower, FullBody, Active Recovery, Competition |
| T-085 | Crear entidad `ProgramTemplate` (Aggregate Root) | ✅ | Con Weeks collection |
| T-086 | Crear entidad `ProgramWeekTemplate` | ✅ | WeekNumber, Days collection |
| T-087 | Crear entidad `ProgramDayTemplate` | ✅ | DayOfWeek, Focus, Exercises collection |
| T-088 | Crear entidad `ProgramExerciseTemplate` | ✅ | Sets, Reps, RPE, RestSeconds, Instructions |
| T-089 | Crear entidad `AthleteProgram` (Aggregate Root) | ✅ | Programa asignado a atleta |
| T-090 | Crear entidad `AthleteWorkout` | ✅ | Workout ejecutado por atleta |
| T-091 | Crear entidad `AthleteExerciseLog` | ✅ | Log de ejercicio individual |
| T-092 | Crear EF Core Configurations (7 archivos) | ✅ | Fluent API completo |
| T-093 | Crear `TrainingProgramDtos.cs` | ✅ | 15 DTOs |
| T-094 | Implementar Commands CQRS | ✅ | 6 commands con handlers y validators |
| T-095 | Implementar Queries CQRS | ✅ | 4 queries con handlers |
| T-096 | Crear `TrainingProgramsController` | ✅ | 10 endpoints REST |
| T-097 | Crear tests unitarios TrainingPrograms | ✅ | 19 tests (commands + queries) |

**Criterios de Aceptación**:
- [x] Coach puede crear plantillas de programas reutilizables
- [x] Programas pueden asignarse a atletas
- [x] Atletas pueden registrar su progreso por workout
- [x] Tests unitarios cubren happy paths y edge cases

---

### F-011: Frontend Dashboard (Coach)
**Estado**: ✅ Completada | **Prioridad**: 🟡 Alta | **Sprint**: 5

Crear proyecto base del dashboard para coaches con React/Next.js.

| ID | Task | Estado | Notas |
|----|------|--------|-------|
| T-098 | Inicializar proyecto Next.js 14 con App Router | ✅ | TypeScript, ESLint |
| T-099 | Configurar TailwindCSS | ✅ | v3.4, custom theme |
| T-100 | Configurar ShadCN UI | ✅ | v3 compatible (button, card, input, table, dialog, etc.) |
| T-101 | Crear API client con Axios | ✅ | Interceptors, error handling, token support |
| T-102 | Configurar TanStack Query | ✅ | QueryProvider, default options |
| T-103 | Crear layout principal (Sidebar + Topbar) | ✅ | Responsive, navigation |
| T-104 | Implementar página Dashboard | ✅ | StatsCards placeholder |
| T-105 | Implementar página Applications | ✅ | DataTable, PageHeader |
| T-106 | Implementar página Athletes | ✅ | Lista de atletas |
| T-107 | Implementar página Programs | ✅ | Lista de programas |
| T-108 | Implementar página Exercises | ✅ | Catálogo de ejercicios |
| T-109 | Implementar página Settings | ✅ | Configuración de coach |

**Criterios de Aceptación**:
- [x] Framework Next.js 14 con App Router funcionando
- [x] TailwindCSS + ShadCN UI configurados y build passing
- [x] Estructura de carpetas escalable (components, hooks, lib, providers)
- [x] API client preparado para conectar con backend
- [x] Páginas placeholder para todas las secciones principales

---

### F-012: Program Builder (Editor Avanzado)
**Estado**: ✅ Completada | **Prioridad**: 🔴 Crítica | **Sprint**: 6

Implementar editor visual avanzado tipo Notion/Linear para crear programas de entrenamiento.

| ID | Task | Estado | Notas |
|----|------|--------|-------|
| T-110 | Crear tipos de estado local del builder | ✅ | BuilderWeek, BuilderDay, BuilderExercise con tempId, isNew, isDirty |
| T-111 | Implementar BuilderProvider con useReducer | ✅ | Context + Reducer con 18 acciones |
| T-112 | Crear componente ExerciseRow | ✅ | Inline editing, navegación TAB, drag handle |
| T-113 | Crear componente ExerciseTable | ✅ | Headers, autocomplete dialog, add blank row |
| T-114 | Crear componente DayBlock | ✅ | Focus selector, collapse/expand, exercise count |
| T-115 | Crear componente WeekBlock | ✅ | Contiene DayBlocks, stats, add day button |
| T-116 | Crear página /programs/[id]/builder | ✅ | Header con save status, keyboard shortcuts |
| T-117 | Crear utils (cn.ts, format-date.ts) | ✅ | Utilidades reutilizables |
| T-118 | Instalar uuid y verificar build | ✅ | Build passing |

**Criterios de Aceptación**:
- [x] Inline editing en campos de ejercicios similar a Excel/Notion
- [x] Navegación con TAB entre campos y filas
- [x] Keyboard shortcuts: Enter, Escape, Arrow keys, Ctrl+S
- [x] Collapse/expand de semanas y días
- [x] Duplicar semanas, días y ejercicios
- [x] Agregar ejercicios desde librería (autocomplete) o en blanco
- [x] Detección de cambios sin guardar con warning al salir
- [x] UX minimalista estilo Notion/Linear
- [x] Build de Next.js compila sin errores

---

### F-013: Backend — Program Builder API Alignment
**Estado**: ✅ Completada | **Prioridad**: 🔴 Crítica | **Sprint**: 7

Alinear el backend con todos los campos y endpoints que el frontend Program Builder necesita.

| ID | Task | Estado | Notas |
|----|------|--------|-------|
| T-119 | Extender DayFocus enum (Push, Pull, Legs, Rest, Cardio, Custom) | ✅ | Alinear con frontend |
| T-120 | Crear ExerciseType enum (Standard, Emom, Tempo, Superset, Circuit, Dropset) | ✅ | Nuevo enum |
| T-121 | Extender ProgramExerciseTemplate con 7 campos builder | ✅ | ExerciseType, PercentageRM, RawNotation, Weight, EmomConfigJson, TempoConfigJson, SupersetConfigJson |
| T-122 | Agregar Name a ProgramWeekTemplate y ProgramDayTemplate | ✅ | Campo string? nullable |
| T-123 | Agregar métodos ClearWeeks/ClearDays/ClearExercises | ✅ | Para bulk save |
| T-124 | Actualizar EF configurations para nuevos campos | ✅ | 3 archivos de config |
| T-125 | Actualizar DTOs con campos builder + crear SaveProgramTemplateDto | ✅ | Bulk save nested DTO |
| T-126 | Crear SaveProgramTemplate command (bulk save) | ✅ | PUT /programs/{id}/full |
| T-127 | Crear UpdateProgramTemplate command | ✅ | PUT /programs/{id} |
| T-128 | Crear Delete commands (Week, Day, Exercise) | ✅ | 3 endpoints DELETE |
| T-129 | Corregir rutas del controller y frontend | ✅ | AddDay, AddExercise con programId; assign-program en frontend |

**Criterios de Aceptación**:
- [x] Build compila sin errores (0 warnings, 0 errors)
- [x] 166 tests unitarios pasando
- [x] Todos los endpoints del frontend tienen backend correspondiente
- [x] Bulk save endpoint funcional (PUT /programs/{id}/full)
- [x] CRUD completo para weeks, days, exercises

---

### F-014: Frontend–Backend Integration & Cleanup
**Estado**: ✅ Completada | **Prioridad**: 🔴 Crítica | **Sprint**: 7

Crear EF migration, conectar builder save con API real, corregir rutas workout, alinear tipos frontend con backend y eliminar código duplicado.

| ID | Task | Estado | Notas |
|----|------|--------|-------|
| T-130 | Crear EF Migration `AddProgramBuilderFields` | ✅ | Primera migration: schema completo + campos F-013 |
| T-131 | Conectar builder save con API real (`programsApi.saveFull`) | ✅ | Reemplaza stub setTimeout |
| T-132 | Corregir rutas workout en controller | ✅ | `/athletes/{aid}/workouts/{wid}` |
| T-133 | Alinear tipo `ProgramExercise` frontend | ✅ | `reps: string` + 7 campos F-013 |
| T-134 | Agregar endpoints + 5 hooks frontend | ✅ | update, saveFull, deleteWeek/Day/Exercise |
| T-135 | Actualizar `builder-provider` con `parseReps` + mapping F-013 | ✅ | initializeFromProgram |
| T-136 | Crear helpers `repsToString`/`parseReps` en programs-api | ✅ | Conversión boundary frontend↔backend |
| T-137 | Eliminar `hooks/use-programs.ts` duplicado | ✅ | Orphan, no importado por nadie |

**Criterios de Aceptación**:
- [x] Backend build 0 errores, 166 tests pasando
- [x] Frontend 0 errores TypeScript en archivos modificados
- [x] Builder save envía datos reales al endpoint PUT /programs/{id}/full
- [x] Conversión repsMin/repsMax ↔ Reps string funcional en ambas direcciones
- [x] Sin archivos duplicados ni dead code

---

### F-015: Workout Tracking System
**Estado**: 🟡 En Progreso | **Prioridad**: 🔴 Crítica | **Sprint**: 8

Sistema de tracking de entrenamientos en tiempo real para atletas.

| ID | Task | Estado | Notas |
|----|------|--------|-------|
| T-138 | Crear enum `WorkoutStatus` | ✅ | NotStarted, InProgress, Completed, Skipped, PartiallyCompleted |
| T-139 | Extender `AthleteWorkout` | ✅ | Status, StartedAt, SkippedReason + Start(), Skip() |
| T-140 | Extender `AthleteExerciseLog` | ✅ | IsCompleted, TargetReps, TargetWeight, SkippedReason + Complete(), Skip() |
| T-141 | Actualizar EF Configurations | ✅ | AthleteWorkoutConfiguration, AthleteExerciseLogConfiguration |
| T-142 | Crear migration `AddWorkoutTrackingFields` | ✅ | 20260324182616_AddWorkoutTrackingFields.cs |
| T-143 | `GetTodayWorkoutQuery` | ⬜ | Workout del día actual para atleta |
| T-144 | `GetWorkoutWithSetsQuery` | ⬜ | Workout completo con ejercicios y sets |
| T-145 | `GetWorkoutHistoryQuery` | ⬜ | Historial paginado |
| T-146 | `StartWorkoutCommand` | ⬜ | Marca workout como InProgress |
| T-147 | `UpdateSetCommand` | ⬜ | Actualiza peso/reps/RPE de un set |
| T-148 | `CompleteSetCommand` | ⬜ | Marca set como completado |
| T-149 | `SkipWorkoutCommand` | ⬜ | Marca workout como saltado |
| T-150 | Refactorizar `LogWorkoutCommand` | ⬜ | Usar nuevos campos |
| T-151 | Crear/Extender `WorkoutsController` | ⬜ | Endpoints REST |
| T-152 | Crear Request/Response DTOs | ⬜ | DTOs de entrada para commands |
| T-153 | Tests unitarios Commands | ⬜ | Start, Update, Complete, Skip |
| T-154 | Tests unitarios Queries | ⬜ | GetToday, GetWithSets, GetHistory |
| T-155 | Crear tipos TypeScript | ⬜ | TodayWorkout, WorkoutExercise, WorkoutSet |
| T-156 | Crear funciones API + hooks | ⬜ | workoutsApi.ts, use-today-workout.ts |
| T-157 | Crear página `/workout` | ⬜ | Layout, loading, empty states |
| T-158 | Componente `WorkoutHeader` | ⬜ | Status, timer, skip button |
| T-159 | Componente `ExerciseCard` | ⬜ | Exercise name, sets table |
| T-160 | Componente `SetRow` | ⬜ | Target, actual inputs, complete checkbox |
| T-161 | Inline editing en SetRow | ⬜ | Click to edit, blur to save |
| T-162 | Auto-save con debounce | ⬜ | 500ms delay |
| T-163 | Previous performance display | ⬜ | Last workout stats |
| T-164 | Agregar link en navegación | ⬜ | Sidebar: Today's Workout |
| T-165 | Widget en Dashboard | ⬜ | Card de workout del día |

**Criterios de Aceptación**:
- [ ] Atleta puede ver workout del día con ejercicios y sets
- [ ] Atleta puede iniciar, completar o saltar workout
- [ ] Atleta puede registrar peso/reps/RPE por set
- [ ] Auto-save de cambios con feedback visual
- [ ] Visualización de performance anterior

**Documentación detallada**: [F-015_WORKOUT_TRACKING_PLAN.md](./F-015_WORKOUT_TRACKING_PLAN.md)

---

## 📝 Changelog

| Fecha | Cambios |
|-------|---------|
| 2026-03-24 | 🟡 F-015 en progreso - Workout Tracking System: Phase 1 (Domain) + Phase 2 (Migration) completadas (T-138 a T-142) |
| 2026-03-23 | ✅ F-014 completada - Frontend–Backend Integration: EF migration, builder save real, workout routes fix, tipos alineados, hooks consolidados |
| 2026-03-13 | ✅ F-013 completada - Backend Program Builder API Alignment: 7 campos nuevos en ProgramExerciseTemplate, ExerciseType enum, bulk save, CRUD completo, 166 tests pasando |
| 2026-03-13 | ✅ PB-010 completada - Calendar Integration: Vista calendario mensual, mapping semanas/días a fechas, drag-to-reschedule con @dnd-kit, 32 tests unitarios |
| 2026-03-13 | ✅ PB-009 completada - Exercise Library con Historial: endpoint /history, ExercisePickerWithHistory, 1RM/PR/trend display |
| 2026-03-13 | ✅ F-012 completada - Program Builder: Editor avanzado tipo Notion con inline editing, TAB navigation, keyboard shortcuts |
| 2026-03-13 | 📦 Nuevos componentes: ExerciseRow, ExerciseTable, DayBlock, WeekBlock + BuilderProvider |
| 2026-03-12 | ✅ F-011 completada - Frontend Dashboard: Next.js 14, TailwindCSS, ShadCN UI, TanStack Query |
| 2026-03-12 | ✅ F-010 completada - Training Programs: 7 entidades, 6 commands, 4 queries, 19 tests |
| 2026-03-12 | 📦 Nuevo proyecto: coach-dashboard (React/Next.js frontend) |
| 2026-03-10 | ✅ F-009 completada - Multi-tenancy: ITenantService, TenantValidationBehavior, TenantMiddleware |
| 2026-03-10 | ✅ F-008 completada - Exercise Library: Exercise entity, ExerciseCategory enum, MuscleGroup enum, CQRS completo |
| 2026-03-10 | 🔧 Fix: nuget.config para bypass de feed privado, CheckInListItemDto properties |
| 2026-03-10 | 📈 Tests: 147 tests unitarios pasando (+22 nuevos tests para Exercise entity) || 2026-03-06 | 🔄 Refactor: Application layer - Shared/ (Interfaces, DTOs, Behaviors, Exceptions) + Features/Athletes |
| 2026-03-05 | ✅ F-007 completada - Docker: Dockerfile, docker-compose, README |
| 2026-03-05 | ✅ F-006 completada - Tests: Unit Tests (Email, Money, Athlete) |
| 2026-03-05 | ✅ F-005 completada - API: Controllers, Middleware, Swagger, CORS |
| 2026-03-05 | ✅ F-004 completada - Infrastructure: DbContext, Configurations, Repositories |
| 2026-03-05 | ✅ F-003 completada - Application: DTOs, Commands, Queries, Validators |
| 2026-03-05 | ✅ F-002 completada - Dominio: 7 entidades, 3 VOs, 5 enums, 8 interfaces |
| 2026-03-05 | ✅ F-001 completada - Estructura de solución .NET 9 creada |
| 2026-03-05 | Creación inicial del backlog - 7 Features, 68 Tasks definidas |

---

## 🔮 Backlog Futuro (Post-MVP)

Funcionalidades identificadas para fases posteriores:

- [ ] **Autenticación y Autorización** - JWT, Identity, Roles
- [ ] **Notificaciones** - Email transaccional, push notifications
- [x] ~~**Dashboard del Coach**~~ - Métricas, reportes visuales (Implementado en F-011)
- [x] ~~**Program Builder**~~ - Editor avanzado tipo Notion (Implementado en F-012)
- [ ] **Drag & Drop en Builder** - Reordenamiento visual de semanas, días y ejercicios
- [ ] **Integración con Pasarelas de Pago** - Stripe, PayPal, MercadoPago
- [ ] **API de Reportes y Analytics** - Estadísticas de atletas, ingresos
- [ ] **App Mobile** - React Native o Flutter
- [ ] **Calendario y Scheduling** - Sesiones, recordatorios
- [ ] **Integración con Wearables** - Garmin, Apple Watch, Fitbit
- [~] **Athlete Mobile App** - Visualización y logging de workouts (En progreso: F-015 Workout Tracking)

---

## 📌 Notas de Sesión

> Espacio para notas temporales durante el desarrollo

_Sin notas actuales_
