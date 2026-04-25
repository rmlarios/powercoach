# CoachPlatform - Product Backlog

> Plataforma de gestión para coaches de powerlifting/fitness con dashboard web.
> Arquitectura: Clean Architecture + DDD | Stack: .NET 9, PostgreSQL 16, Next.js 16, Docker

---

## 📊 Resumen del Proyecto

| Métrica | Valor |
|---------|-------|
| **Features Totales** | 16 |
| **Tasks Totales** | 172 |
| **✅ Completadas** | 149 |
| **🟡 En Progreso** | 0 |
| **⬜ Pendientes** | 23 |
| **Tests Unitarios** | 204+ pasando |

**Última actualización**: 2026-04-24

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
| T-028 | Crear interfaces de repositorios específicos | ✅ | 10 repositorios |
| T-029 | Crear `IUnitOfWork` interface | ✅ | Con transacciones |

**Criterios de Aceptación**:
- [x] Todas las entidades heredan de `AuditableEntity`
- [x] Value Objects son inmutables con validación
- [x] Relaciones entre entidades definidas correctamente

---

### F-003: Capa de Aplicación
**Estado**: ✅ Completada | **Prioridad**: 🔴 Crítica | **Sprint**: 1-2

Implementar DTOs, Commands, Queries y Validators con CQRS ligero.

| ID | Task | Estado | Notas |
|----|------|--------|-------|
| T-030 | Configurar MediatR | ✅ | Pipeline behaviors |
| T-031 | Crear `ValidationBehavior` para MediatR | ✅ | FluentValidation integration |
| T-032 | Crear DTOs de Athletes | ✅ | AthleteDto, CreateAthleteDto |
| T-033 | Crear DTOs de Applications | ✅ | ApplicationDto, etc. |
| T-034 | Crear DTOs de Plans, Subscriptions, Payments | ✅ | + CheckIn, Coach, Dashboard DTOs |
| T-035 | Implementar `CreateAthleteCommand` + Handler | ✅ | |
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
| T-040 | Crear `ApplicationDbContext` | ✅ | 18 DbSets, auditoría automática |
| T-041 | Crear `CoachConfiguration` (Fluent API) | ✅ | |
| T-042 | Crear `AthleteConfiguration` | ✅ | |
| T-043 | Crear `ApplicationConfiguration` | ✅ | |
| T-044 | Crear `PlanConfiguration` | ✅ | |
| T-045 | Crear `SubscriptionConfiguration` | ✅ | |
| T-046 | Crear `PaymentConfiguration` | ✅ | |
| T-047 | Crear `CheckInConfiguration` | ✅ | |
| T-048 | Implementar `BaseRepository<T>` | ✅ | Genérico |
| T-049 | Implementar repositorios específicos | ✅ | 10 repositorios |
| T-050 | Implementar `UnitOfWork` | ✅ | |
| T-051 | Crear `DependencyInjection.cs` de Infrastructure | ✅ | AddInfrastructure() |

**Criterios de Aceptación**:
- [x] Todas las entidades mapeadas con Fluent API (19 archivos)
- [x] Value Objects convertidos correctamente (OwnsOne)
- [x] Índices y constraints configurados

---

### F-005: Capa de API
**Estado**: ✅ Completada | **Prioridad**: 🔴 Crítica | **Sprint**: 2

Implementar Controllers, Middleware y configuración de la API.

| ID | Task | Estado | Notas |
|----|------|--------|-------|
| T-052 | Configurar `Program.cs` con DI completo | ✅ | JWT Auth, Policies, Swagger, CORS, Auto-migration |
| T-053 | Implementar Controllers | ✅ | 11 controllers + sub-controllers |
| T-054 | Implementar `ExceptionHandlingMiddleware` | ✅ | 400/403/404/409/500 |
| T-055 | Configurar Swagger/OpenAPI | ✅ | Con JWT Bearer security |
| T-056 | Configurar CORS | ✅ | Múltiples origins para dev |
| T-057 | Crear `appsettings.json` | ✅ | ConnectionString, JWT, CORS |

**Criterios de Aceptación**:
- [x] 70+ endpoints REST funcionando
- [x] Swagger documenta todos los endpoints
- [x] Errores devuelven formato consistente
- [x] JWT Authentication + Authorization Policies

---

### F-006: Testing
**Estado**: ✅ Completada | **Prioridad**: 🟡 Alta | **Sprint**: 2-3

Implementar tests unitarios.

| ID | Task | Estado | Notas |
|----|------|--------|-------|
| T-058 | Configurar proyecto de Unit Tests | ✅ | xUnit + Moq + FluentAssertions |
| T-059 | Tests: Command Handlers (19 archivos) | ✅ | Happy paths + errores |
| T-060 | Tests: Value Objects (Email, Money) | ✅ | Validación |
| T-061 | Tests: Entidades de dominio (5 archivos) | ✅ | Comportamiento |
| T-062 | Tests: Query Handlers (3 archivos) | ✅ | |
| T-063 | Configurar proyecto de Integration Tests | ✅ | Skeleton preparado |

**Criterios de Aceptación**:
- [x] 204+ tests unitarios pasando
- [x] Domain, Application Commands y Queries cubiertos
- [x] Mocks correctamente configurados

---

### F-007: Docker y Deployment
**Estado**: ✅ Completada | **Prioridad**: 🟢 Media | **Sprint**: 3

Configurar Docker y preparar para deployment.

| ID | Task | Estado | Notas |
|----|------|--------|-------|
| T-064 | Crear `Dockerfile` multi-stage (API) | ✅ | SDK 9.0 → ASP.NET 9.0, non-root |
| T-065 | Crear `docker-compose.yml` | ✅ | API + Frontend + PostgreSQL + pgAdmin |
| T-066 | Crear `Dockerfile` frontend | ✅ | node:20-alpine, standalone |
| T-067 | Migrations automáticas en startup | ✅ | `db.Database.MigrateAsync()` |
| T-068 | Admin seeding en startup | ✅ | admin@powercoach.com |
| T-069 | Documentar README.md | ✅ | Setup, Quick Start, comandos |

**Criterios de Aceptación**:
- [x] `docker-compose up` levanta el sistema completo (4 servicios)
- [x] Migrations se aplican automáticamente
- [x] README permite onboarding rápido

---

### F-008: Exercise Library
**Estado**: ✅ Completada | **Prioridad**: 🟡 Alta | **Sprint**: 4

Implementar catálogo de ejercicios. Inicialmente por coach, luego refactorizado a **global** (ver F-016).

| ID | Task | Estado | Notas |
|----|------|--------|-------|
| T-070 | Crear enum `ExerciseCategory` | ✅ | 9 categorías |
| T-071 | Crear enum `MuscleGroup` | ✅ | 13 grupos musculares |
| T-072 | Crear entidad `Exercise` (Aggregate Root) | ✅ | VideoUrl, Instructions, CoachingCues |
| T-073 | Crear `ExerciseConfiguration` (EF Core) | ✅ | Índice único por Name (global) |
| T-074 | Refactorizar `WorkoutLog` para usar `ExerciseId` | ✅ | FK + nombre denormalizado |
| T-075 | Implementar CQRS completo para Exercises | ✅ | Create, Update, Delete, List, GetById |
| T-076 | Crear tests unitarios para Exercise entity | ✅ | 22 tests |

**Criterios de Aceptación**:
- [x] Catálogo global de ejercicios compartido entre todos los coaches
- [x] WorkoutLog referencia Exercise por Id
- [x] Tests cubren validaciones de dominio

---

### F-009: Multi-tenancy
**Estado**: ✅ Completada | **Prioridad**: 🔴 Crítica | **Sprint**: 4

Implementar aislamiento de datos por coach (tenant).

| ID | Task | Estado | Notas |
|----|------|--------|-------|
| T-077 | Crear `ICurrentUserService` interface | ✅ | Usuario autenticado |
| T-078 | Crear `ITenantService` interface | ✅ | AsyncLocal-based context |
| T-079 | Crear `ITenantRequest` y `IAthleteOwnedRequest` | ✅ | Marker interfaces |
| T-080 | Implementar `TenantValidationBehavior` | ✅ | MediatR pipeline |
| T-081 | Implementar `TenantMiddleware` | ✅ | JWT claims + X-Coach-Id header |
| T-082 | Crear `ForbiddenAccessException` | ✅ | 403 responses |
| T-083 | Aplicar interfaces tenant a Commands/Queries | ✅ | Excepto Exercises (global) |

**Criterios de Aceptación**:
- [x] Validación automática de tenant en pipeline
- [x] Coach solo accede a sus propios datos
- [x] Admin bypass implementado
- [x] Exercises excluido de tenant (catálogo global)

---

### F-010: Training Programs Module
**Estado**: ✅ Completada | **Prioridad**: 🔴 Crítica | **Sprint**: 5

Implementar módulo completo de programas de entrenamiento.

| ID | Task | Estado | Notas |
|----|------|--------|-------|
| T-084 | Crear enums `ProgramStatus`, `DayFocus` | ✅ | |
| T-085 | Crear entidad `ProgramTemplate` (Aggregate Root) | ✅ | Con Weeks collection |
| T-086 | Crear entidad `ProgramWeekTemplate` | ✅ | WeekNumber, Days collection |
| T-087 | Crear entidad `ProgramDayTemplate` | ✅ | DayNumber, Focus, Exercises |
| T-088 | Crear entidad `ProgramExerciseTemplate` | ✅ | Sets, Reps, RPE, ExerciseType, %RM |
| T-089 | Crear entidad `AthleteProgram` (Aggregate Root) | ✅ | Start, Pause, Resume, Complete |
| T-090 | Crear entidad `AthleteWorkout` | ✅ | WorkoutStatus tracking |
| T-091 | Crear entidad `AthleteExerciseLog` | ✅ | Set-level tracking |
| T-092 | Crear EF Core Configurations (7 archivos) | ✅ | Fluent API completo |
| T-093 | Crear DTOs (15+) | ✅ | TrainingProgramDtos.cs |
| T-094 | Implementar Commands CQRS | ✅ | 11 commands con handlers |
| T-095 | Implementar Queries CQRS | ✅ | 4 queries con handlers |
| T-096 | Crear `TrainingProgramsController` | ✅ | 14 endpoints REST |
| T-097 | Crear tests unitarios | ✅ | 19 tests |

**Criterios de Aceptación**:
- [x] Coach puede crear plantillas de programas reutilizables
- [x] Programas pueden asignarse a atletas
- [x] Atletas pueden registrar su progreso por workout
- [x] Bulk save para toda la estructura del programa

---

### F-011: Frontend Dashboard
**Estado**: ✅ Completada | **Prioridad**: 🟡 Alta | **Sprint**: 5

Dashboard web completo para coaches y atletas.

| ID | Task | Estado | Notas |
|----|------|--------|-------|
| T-098 | Inicializar proyecto Next.js 16 con App Router | ✅ | TypeScript, ESLint, Turbopack |
| T-099 | Configurar TailwindCSS | ✅ | v3.4, custom theme |
| T-100 | Configurar componentes UI (Radix + CVA) | ✅ | ShadCN pattern |
| T-101 | Crear API client con Axios | ✅ | Interceptors, token refresh, tenant header |
| T-102 | Configurar TanStack Query | ✅ | QueryProvider |
| T-103 | Crear layout principal (Sidebar + Topbar) | ✅ | Role-based navigation |
| T-104 | Implementar AuthProvider + Login | ✅ | JWT tokens en localStorage + cookies |
| T-105 | Implementar página Dashboard | ✅ | StatsCards, AlertsPanel, ActivityFeed |
| T-106 | Implementar página Applications | ✅ | Lista + Detalle + Approve/Reject |
| T-107 | Implementar página Athletes | ✅ | Lista + Detalle + CRUD |
| T-108 | Implementar página Programs | ✅ | Lista + Builder link |
| T-109 | Implementar página Exercises | ✅ | Catálogo global CRUD |
| T-110 | Implementar página Check-ins | ✅ | Coach review + Athlete form |
| T-111 | Implementar páginas Plans, Subscriptions, Payments | ✅ | CRUD completo |
| T-112 | Implementar página Settings | ✅ | |
| T-113 | Implementar página Admin Users | ✅ | User management |
| T-114 | Implementar formulario público Apply | ✅ | Postulación sin auth |
| T-115 | Implementar Athlete Dashboard + Check-in | ✅ | Vista de atleta |
| T-116 | Middleware de auth + role routing | ✅ | proxy.ts |

**Criterios de Aceptación**:
- [x] Next.js 16 con App Router y Turbopack
- [x] 21 páginas/rutas implementadas
- [x] Role-based navigation (Coach, Athlete, Admin)
- [x] API client con auto-refresh de tokens
- [x] Exportación Excel y PDF

---

### F-012: Program Builder (Editor Avanzado)
**Estado**: ✅ Completada | **Prioridad**: 🔴 Crítica | **Sprint**: 6

Editor visual avanzado tipo Notion/Linear para crear programas de entrenamiento.

| ID | Task | Estado | Notas |
|----|------|--------|-------|
| T-117 | Crear tipos de estado local del builder | ✅ | BuilderWeek, BuilderDay, BuilderExercise |
| T-118 | Implementar BuilderProvider con useReducer | ✅ | 18+ acciones |
| T-119 | Crear componente ExerciseRow | ✅ | Inline editing, TAB navigation |
| T-120 | Crear componente ExerciseTable | ✅ | Autocomplete dialog |
| T-121 | Crear componente DayBlock | ✅ | Focus selector, collapse/expand |
| T-122 | Crear componente WeekBlock | ✅ | Stats, add day button |
| T-123 | Crear página /programs/[id]/builder | ✅ | Ctrl+S, save status |
| T-124 | Conectar builder save con API (bulk save) | ✅ | PUT /programs/{id}/full |

**Criterios de Aceptación**:
- [x] Inline editing similar a Excel/Notion
- [x] TAB navigation entre campos y filas
- [x] Keyboard shortcuts: Enter, Escape, Ctrl+S
- [x] Collapse/expand semanas y días
- [x] Duplicar semanas, días y ejercicios
- [x] Autocomplete de ejercicios desde catálogo global
- [x] Detección de cambios sin guardar
- [x] Bulk save al backend

---

### F-013: Backend — Program Builder API Alignment
**Estado**: ✅ Completada | **Prioridad**: 🔴 Crítica | **Sprint**: 7

Alinear backend con los campos y endpoints que el Program Builder necesita.

| ID | Task | Estado | Notas |
|----|------|--------|-------|
| T-125 | Crear `ExerciseType` enum | ✅ | Standard, Emom, Tempo, Superset, Circuit |
| T-126 | Extender `ProgramExerciseTemplate` con 7 campos | ✅ | ExerciseType, %RM, Weight, configs JSON |
| T-127 | Agregar `Name` a Week/Day templates | ✅ | Nullable string |
| T-128 | Agregar métodos ClearWeeks/Days/Exercises | ✅ | Para bulk save |
| T-129 | Actualizar EF configurations | ✅ | 3 archivos |
| T-130 | Crear SaveProgramTemplate command | ✅ | PUT /programs/{id}/full |
| T-131 | Crear Delete commands (Week, Day, Exercise) | ✅ | 3 endpoints DELETE |
| T-132 | Migration `AddProgramBuilderFields` | ✅ | Schema completo |

**Criterios de Aceptación**:
- [x] Build 0 errores
- [x] Bulk save endpoint funcional
- [x] CRUD completo para weeks, days, exercises

---

### F-014: Frontend–Backend Integration & Cleanup
**Estado**: ✅ Completada | **Prioridad**: 🔴 Crítica | **Sprint**: 7

Conectar builder save con API real, corregir rutas, alinear tipos, eliminar duplicados.

| ID | Task | Estado | Notas |
|----|------|--------|-------|
| T-133 | Conectar builder save con API real | ✅ | Reemplaza stub setTimeout |
| T-134 | Corregir rutas workout en controller | ✅ | |
| T-135 | Alinear tipos frontend con backend | ✅ | `reps: string` + campos F-013 |
| T-136 | Agregar endpoints + hooks frontend | ✅ | update, saveFull, delete |
| T-137 | Crear helpers repsToString/parseReps | ✅ | Conversión boundary |
| T-138 | Eliminar archivos duplicados | ✅ | hooks/use-programs.ts orphan |

---

### F-015: Workout Tracking System
**Estado**: ✅ Completada | **Prioridad**: 🔴 Crítica | **Sprint**: 8

Sistema de tracking de entrenamientos en tiempo real para atletas.

| ID | Task | Estado | Notas |
|----|------|--------|-------|
| T-139 | Crear enum `WorkoutStatus` | ✅ | NotStarted → Completed/Skipped |
| T-140 | Extender `AthleteWorkout` con tracking fields | ✅ | Status, StartedAt, SkippedReason |
| T-141 | Extender `AthleteExerciseLog` con tracking | ✅ | IsCompleted, TargetReps/Weight |
| T-142 | Actualizar EF Configurations | ✅ | |
| T-143 | Migration `AddWorkoutTrackingFields` | ✅ | |
| T-144 | Implementar Commands (Start, Save, Complete, Skip) | ✅ | 7 commands |
| T-145 | Implementar Queries (Today, Detail, Week, History) | ✅ | 6 queries |
| T-146 | Crear WorkoutsController con endpoints | ✅ | 12 endpoints REST |
| T-147 | Crear DTOs de Workout Tracking | ✅ | WorkoutTrackingDtos.cs |
| T-148 | Crear tipos TypeScript | ✅ | workout-tracking.ts |
| T-149 | Crear API service + hooks | ✅ | workout-tracking-api.ts, hooks/workouts/ |
| T-150 | Crear página `/workout` | ✅ | Layout completo |
| T-151 | Componentes: WorkoutHeader, ExerciseCard, SetRow | ✅ | Inline editing |
| T-152 | RestTimer, PRBadge, WeekStrip | ✅ | |
| T-153 | Previous performance display | ✅ | |
| T-154 | Tests unitarios (38 nuevos) | ✅ | 14 domain + 13 log + 9 handler + 2 query |

**Criterios de Aceptación**:
- [x] Atleta puede ver workout del día con ejercicios y sets
- [x] Atleta puede iniciar, completar o saltar workout
- [x] Atleta puede registrar peso/reps/RPE por set
- [x] Inline editing con auto-save
- [x] Visualización de performance anterior
- [x] 204+ tests pasando

**Documentación detallada**: [F-015_WORKOUT_TRACKING_PLAN.md](./F-015_WORKOUT_TRACKING_PLAN.md)

---

### F-016: Catálogo de Ejercicios Global
**Estado**: ✅ Completada | **Prioridad**: 🟡 Alta | **Sprint**: 9

Refactorizar el catálogo de ejercicios de per-coach a global compartido.

| ID | Task | Estado | Notas |
|----|------|--------|-------|
| T-155 | Hacer `Exercise.CoachId` nullable (`Guid?`) | ✅ | FK opcional |
| T-156 | Actualizar `ExerciseConfiguration` | ✅ | `OnDelete(SetNull)`, índice único solo por Name |
| T-157 | Remover `ITenantRequest` de Exercise commands/queries | ✅ | Sin validación de tenant |
| T-158 | Remover `CoachId` de handlers y validators | ✅ | Duplicate check global |
| T-159 | Cambiar ruta API a `/api/exercises` (sin coach prefix) | ✅ | ExercisesController |
| T-160 | Actualizar frontend endpoints (sin coachId) | ✅ | endpoints.ts, exercises-api.ts |
| T-161 | Actualizar hooks (sin coachId) | ✅ | use-exercises.ts |
| T-162 | Actualizar página de ejercicios | ✅ | exercises/page.tsx |
| T-163 | Actualizar Program Builder (fetch global) | ✅ | builder/page.tsx |
| T-164 | Migration `MakeExercisesGlobal` | ✅ | CoachId nullable, índice global |

**Criterios de Aceptación**:
- [x] Catálogo de ejercicios compartido entre todos los coaches
- [x] Sin ejercicios duplicados por coach
- [x] API funcional en `/api/exercises`
- [x] Frontend funcional sin errores TypeScript
- [x] Migration aplicada correctamente

---

### F-A01: Autenticación y Autorización (JWT)
**Estado**: ✅ Completada | **Prioridad**: 🔴 Crítica | **Sprint**: 8

Sistema completo de autenticación con JWT.

| ID | Task | Estado | Notas |
|----|------|--------|-------|
| T-165 | Crear entidad `User` con roles | ✅ | Coach, Athlete, Admin |
| T-166 | Implementar `JwtService` | ✅ | HS256, access + refresh tokens |
| T-167 | Crear `AuthController` | ✅ | Login, refresh, logout, me, CRUD users |
| T-168 | Implementar Login/Logout/Refresh commands | ✅ | |
| T-169 | Crear Authorization Policies | ✅ | CoachOnly, AthleteOnly, AdminOnly, CoachOrAdmin |
| T-170 | Admin seeding en startup | ✅ | admin@powercoach.com |
| T-171 | Frontend AuthProvider + Login page | ✅ | Token management |
| T-172 | Migration `AddUserAuthentication` | ✅ | Users table |

---

## 📝 Changelog

| Fecha | Cambios |
|-------|---------|
| 2026-04-24 | ✅ F-016 completada — Catálogo de ejercicios global: Exercise.CoachId nullable, ruta /api/exercises, sin tenant |
| 2026-04-24 | 🔧 Bug fixes: Email VO comparison en LINQ, date null guards en frontend, 409 handling en assign-program |
| 2026-04-16 | ✅ F-A01 completada — Autenticación JWT: User entity, JwtService, AuthController, Login/Logout, Admin seed |
| 2026-03-24 | ✅ F-015 completada — Workout Tracking System: 12 endpoints, 15 componentes, 38 tests nuevos, 204+ total |
| 2026-03-24 | ✅ F-014 completada — Frontend–Backend Integration: builder save real, rutas corregidas, tipos alineados |
| 2026-03-13 | ✅ F-013 completada — Backend Program Builder API: 7 campos nuevos, ExerciseType enum, bulk save |
| 2026-03-13 | ✅ F-012 completada — Program Builder: Editor avanzado tipo Notion con inline editing |
| 2026-03-12 | ✅ F-011 completada — Frontend Dashboard: Next.js, TailwindCSS, ShadCN, TanStack Query |
| 2026-03-12 | ✅ F-010 completada — Training Programs: 7 entidades, 11 commands, 4 queries |
| 2026-03-10 | ✅ F-009 completada — Multi-tenancy: TenantService, TenantMiddleware, TenantValidationBehavior |
| 2026-03-10 | ✅ F-008 completada — Exercise Library: Exercise entity, enums, CQRS, 22 tests |
| 2026-03-05 | ✅ F-001 a F-007 completadas — Setup inicial, Domain, Application, Infrastructure, API, Tests, Docker |

---

## 🔮 Backlog Futuro (Post-MVP)

| Feature | Estado | Descripción |
|---------|--------|-------------|
| Notificaciones | ⬜ | Email transaccional, push notifications |
| Drag & Drop en Builder | ⬜ | Reordenamiento visual con @dnd-kit (parcialmente implementado) |
| Integración Pasarelas de Pago | ⬜ | Stripe, PayPal, MercadoPago |
| API de Reportes y Analytics | ⬜ | Estadísticas de atletas, ingresos |
| App Mobile | ⬜ | React Native o Flutter |
| Integración con Wearables | ⬜ | Garmin, Apple Watch, Fitbit |
| Dashboard Widget: Workout del Día | ⬜ | Card en dashboard principal |
| Roles de edición en Exercises | ⬜ | Ownership/permisos en catálogo global |
| Integration Tests | ⬜ | WebApplicationFactory E2E tests |
| CI/CD Pipeline | ⬜ | GitHub Actions: build, test, deploy |
| Rate Limiting | ⬜ | Por IP y por usuario |
| Structured Logging | ⬜ | Serilog |
| OpenTelemetry | ⬜ | Tracing distribuido |

---

## 📌 Notas de Sesión

> Espacio para notas temporales durante el desarrollo

_Sin notas actuales_
