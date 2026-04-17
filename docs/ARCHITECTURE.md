# CoachPlatform - Arquitectura

> Documentación técnica de arquitectura y decisiones de diseño.

---

## 📐 Stack Tecnológico

### Backend

| Categoría | Tecnología | Versión |
|-----------|------------|--------|
| **Runtime** | .NET | 9.0 |
| **Framework** | ASP.NET Core Web API | 9.0 |
| **ORM** | Entity Framework Core | 9.x |
| **Base de Datos** | PostgreSQL | 15+ |
| **Hosting DB** | Supabase | - |
| **Contenedores** | Docker | - |
| **Testing** | xUnit + FluentAssertions + Moq | - |

### Frontend (coach-dashboard)

| Categoría | Tecnología | Versión |
|-----------|------------|--------|
| **Framework** | Next.js (App Router) | 14.x |
| **UI Library** | React | 18.x |
| **Lenguaje** | TypeScript | 5.x |
| **Estilos** | TailwindCSS | 3.4 |
| **Componentes** | ShadCN UI | 0.8 (v3) |
| **Data Fetching** | TanStack Query | 5.x |
| **HTTP Client** | Axios | 1.x |

---

## 🏗️ Diagrama de Arquitectura

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                              FRONTEND                                       │
│  ┌───────────────────────────────────────────────────────────────────────┐  │
│  │                      coach-dashboard (Next.js 14)                     │  │
│  │  Pages │ Components │ Hooks │ Providers │ API Client │ TanStack Query│  │
│  │                    (React + TypeScript + TailwindCSS)                 │  │
│  └───────────────────────────────────────────────────────────────────────┘  │
├─────────────────────────────────────────────────────────────────────────────┤
│                                  ▼  REST API                                │
├─────────────────────────────────────────────────────────────────────────────┤
│                              PRESENTATION                                   │
│  ┌───────────────────────────────────────────────────────────────────────┐  │
│  │                     CoachPlatform.API                                 │  │
│  │  Controllers │ Middleware │ Filters │ Configuration │ Swagger        │  │
│  └───────────────────────────────────────────────────────────────────────┘  │
├─────────────────────────────────────────────────────────────────────────────┤
│                              APPLICATION                                    │
│  ┌───────────────────────────────────────────────────────────────────────┐  │
│  │                   CoachPlatform.Application                           │  │
│  │  Commands │ Queries │ Handlers │ DTOs │ Validators │ Behaviors       │  │
│  │                         (MediatR + FluentValidation)                  │  │
│  └───────────────────────────────────────────────────────────────────────┘  │
├─────────────────────────────────────────────────────────────────────────────┤
│                                DOMAIN                                       │
│  ┌───────────────────────────────────────────────────────────────────────┐  │
│  │                     CoachPlatform.Domain                              │  │
│  │  Entities │ Value Objects │ Enums │ Domain Events │ Interfaces       │  │
│  │                    (Sin dependencias externas)                        │  │
│  └───────────────────────────────────────────────────────────────────────┘  │
├─────────────────────────────────────────────────────────────────────────────┤
│                             INFRASTRUCTURE                                  │
│  ┌───────────────────────────────────────────────────────────────────────┐  │
│  │                   CoachPlatform.Infrastructure                        │  │
│  │  DbContext │ Repositories │ Configurations │ External Services       │  │
│  │                    (EF Core + PostgreSQL)                             │  │
│  └───────────────────────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────────────────────┘
                                      │
                                      ▼
                        ┌───────────────────────┐
                        │      PostgreSQL       │
                        │      (Supabase)       │
                        └───────────────────────┘
```

---

## 📦 Estructura de Proyectos

```
CoachPlatform/
├── src/
│   ├── CoachPlatform.Domain/
│   │   ├── Common/
│   │   │   ├── BaseEntity.cs
│   │   │   ├── AuditableEntity.cs
│   │   │   └── IAggregateRoot.cs
│   │   ├── Entities/
│   │   │   ├── Coach.cs
│   │   │   ├── Athlete.cs
│   │   │   ├── Application.cs
│   │   │   ├── Plan.cs
│   │   │   ├── Subscription.cs
│   │   │   ├── Payment.cs
│   │   │   ├── CheckIn.cs
│   │   │   ├── Exercise.cs
│   │   │   ├── TrainingCycle.cs
│   │   │   ├── WorkoutLog.cs
│   │   │   ├── ProgramTemplate.cs          # Training Programs
│   │   │   ├── ProgramWeekTemplate.cs
│   │   │   ├── ProgramDayTemplate.cs
│   │   │   ├── ProgramExerciseTemplate.cs
│   │   │   ├── AthleteProgram.cs
│   │   │   ├── AthleteWorkout.cs
│   │   │   └── AthleteExerciseLog.cs
│   │   ├── ValueObjects/
│   │   │   ├── Email.cs
│   │   │   ├── Money.cs
│   │   │   └── PersonName.cs
│   │   ├── Enums/
│   │   │   ├── ApplicationStatus.cs
│   │   │   ├── AthleteStatus.cs
│   │   │   ├── PaymentStatus.cs
│   │   │   ├── SubscriptionStatus.cs
│   │   │   ├── PlanType.cs
│   │   │   ├── ExerciseCategory.cs
│   │   │   ├── MuscleGroup.cs
│   │   │   ├── ProgramStatus.cs            # Training Programs
│   │   │   └── DayFocus.cs
│   │   ├── DomainEvents/
│   │   └── Interfaces/
│   │       ├── IRepository.cs
│   │       ├── IAthleteRepository.cs
│   │       ├── ICoachRepository.cs
│   │       └── IUnitOfWork.cs
│   │
│   ├── CoachPlatform.Application/
│   │   ├── Shared/
│   │   │   ├── Behaviors/
│   │   │   │   ├── ValidationBehavior.cs
│   │   │   │   ├── LoggingBehavior.cs
│   │   │   │   └── TenantValidationBehavior.cs
│   │   │   ├── DTOs/
│   │   │   │   ├── PagedResult.cs
│   │   │   │   ├── AthleteDtos.cs
│   │   │   │   ├── ApplicationDtos.cs
│   │   │   │   ├── PlanDtos.cs
│   │   │   │   ├── SubscriptionDtos.cs
│   │   │   │   ├── PaymentDtos.cs
│   │   │   │   ├── CheckInDtos.cs
│   │   │   │   ├── CoachDtos.cs
│   │   │   │   └── TrainingProgramDtos.cs      # 15 DTOs para programas
│   │   │   ├── Exceptions/
│   │   │   │   ├── ValidationException.cs
│   │   │   │   ├── NotFoundException.cs
│   │   │   │   ├── ConflictException.cs
│   │   │   │   └── ForbiddenAccessException.cs
│   │   │   └── Interfaces/
│   │   │       ├── IApplicationDbContext.cs
│   │   │       ├── ICurrentUserService.cs
│   │   │       ├── ITenantService.cs
│   │   │       └── ITenantRequest.cs
│   │   ├── Features/
│   │   │   ├── Athletes/
│   │   │   │   ├── Commands/
│   │   │   │   │   └── CreateAthlete/
│   │   │   │   │       ├── CreateAthleteCommand.cs
│   │   │   │   │       ├── CreateAthleteCommandHandler.cs
│   │   │   │   │       └── CreateAthleteCommandValidator.cs
│   │   │   │   └── Queries/
│   │   │   │       ├── GetAthleteById/
│   │   │   │       │   ├── GetAthleteByIdQuery.cs
│   │   │   │       │   └── GetAthleteByIdQueryHandler.cs
│   │   │   │       └── GetAthletesByCoach/
│   │   │   │           ├── GetAthletesByCoachQuery.cs
│   │   │   │           └── GetAthletesByCoachQueryHandler.cs
│   │   │   └── TrainingPrograms/               # Nuevo módulo
│   │   │       ├── Commands/
│   │   │       │   ├── CreateProgramTemplate/
│   │   │       │   ├── AddProgramWeek/
│   │   │       │   ├── AddProgramDay/
│   │   │       │   ├── AddProgramExercise/
│   │   │       │   ├── AssignProgramToAthlete/
│   │   │       │   └── LogWorkout/
│   │   │       └── Queries/
│   │   │           ├── GetProgramTemplates/
│   │   │           ├── GetProgramTemplateById/
│   │   │           ├── GetAthleteProgram/
│   │   │           └── GetAthleteWorkout/
│   │   └── DependencyInjection.cs
│   │
│   ├── CoachPlatform.Infrastructure/
│   │   ├── Persistence/
│   │   │   ├── ApplicationDbContext.cs
│   │   │   ├── Configurations/
│   │   │   │   ├── CoachConfiguration.cs
│   │   │   │   ├── AthleteConfiguration.cs
│   │   │   │   ├── ApplicationConfiguration.cs
│   │   │   │   ├── PlanConfiguration.cs
│   │   │   │   ├── SubscriptionConfiguration.cs
│   │   │   │   ├── PaymentConfiguration.cs
│   │   │   │   ├── CheckInConfiguration.cs
│   │   │   │   ├── ExerciseConfiguration.cs
│   │   │   │   ├── TrainingCycleConfiguration.cs
│   │   │   │   ├── WorkoutLogConfiguration.cs
│   │   │   │   ├── ProgramTemplateConfiguration.cs      # Training Programs
│   │   │   │   ├── ProgramWeekTemplateConfiguration.cs
│   │   │   │   ├── ProgramDayTemplateConfiguration.cs
│   │   │   │   ├── ProgramExerciseTemplateConfiguration.cs
│   │   │   │   ├── AthleteProgramConfiguration.cs
│   │   │   │   ├── AthleteWorkoutConfiguration.cs
│   │   │   │   └── AthleteExerciseLogConfiguration.cs
│   │   │   ├── Repositories/
│   │   │   │   ├── BaseRepository.cs
│   │   │   │   ├── CoachRepository.cs
│   │   │   │   ├── AthleteRepository.cs
│   │   │   │   ├── ApplicationRepository.cs
│   │   │   │   ├── PlanRepository.cs
│   │   │   │   ├── SubscriptionRepository.cs
│   │   │   │   ├── PaymentRepository.cs
│   │   │   │   └── CheckInRepository.cs
│   │   │   └── Migrations/
│   │   ├── Services/
│   │   │   ├── CurrentUserService.cs
│   │   │   └── TenantService.cs
│   │   ├── UnitOfWork.cs
│   │   └── DependencyInjection.cs
│   │
│   └── CoachPlatform.API/
│       ├── Controllers/
│       │   ├── AthletesController.cs
│       │   └── TrainingProgramsController.cs   # 10 endpoints
│       ├── Middleware/
│       │   ├── ExceptionHandlingMiddleware.cs
│       │   └── TenantMiddleware.cs
│       ├── Program.cs
│       ├── appsettings.json
│       ├── appsettings.Development.json
│       └── Dockerfile
│
├── tests/
│   ├── CoachPlatform.UnitTests/
│   │   ├── Domain/
│   │   │   ├── Entities/
│   │   │   │   ├── AthleteTests.cs
│   │   │   │   ├── ApplicationTests.cs
│   │   │   │   └── ExerciseTests.cs
│   │   │   └── ValueObjects/
│   │   │       ├── EmailTests.cs
│   │   │       └── MoneyTests.cs
│   │   └── Application/
│   │       └── Commands/
│   │           └── (Handler tests)
│   └── CoachPlatform.IntegrationTests/
│
├── docs/
│   ├── BACKLOG.md
│   └── ARCHITECTURE.md
│
├── CoachPlatform.sln
├── docker-compose.yml
├── Dockerfile
├── .dockerignore
├── .gitignore
└── README.md
```

### Estructura Frontend (coach-dashboard)

```
coach-dashboard/
├── src/
│   ├── app/
│   │   ├── (dashboard)/
│   │   │   ├── layout.tsx              # Layout con Sidebar + Topbar
│   │   │   ├── dashboard/
│   │   │   │   └── page.tsx
│   │   │   ├── applications/
│   │   │   │   └── page.tsx
│   │   │   ├── athletes/
│   │   │   │   └── page.tsx
│   │   │   ├── programs/
│   │   │   │   ├── page.tsx
│   │   │   │   └── [id]/
│   │   │   │       └── builder/
│   │   │   │           └── page.tsx    # Program Builder (Notion-like editor)
│   │   │   ├── exercises/
│   │   │   │   └── page.tsx
│   │   │   └── settings/
│   │   │       └── page.tsx
│   │   ├── globals.css
│   │   └── layout.tsx                  # Root layout
│   │
│   ├── components/
│   │   ├── builder/                    # Program Builder components
│   │   │   ├── index.ts
│   │   │   ├── exercise-row.tsx        # Inline editing row
│   │   │   ├── exercise-table.tsx      # Table with autocomplete
│   │   │   ├── day-block.tsx           # Day container
│   │   │   └── week-block.tsx          # Week container
│   │   ├── common/
│   │   │   ├── data-table.tsx
│   │   │   ├── page-header.tsx
│   │   │   ├── confirm-dialog.tsx
│   │   │   ├── status-badge.tsx
│   │   │   ├── stats-card.tsx
│   │   │   ├── loading.tsx
│   │   │   └── pagination.tsx
│   │   ├── layout/
│   │   │   ├── sidebar.tsx
│   │   │   └── topbar.tsx
│   │   └── ui/                         # ShadCN UI components
│   │       ├── button.tsx
│   │       ├── card.tsx
│   │       ├── input.tsx
│   │       ├── table.tsx
│   │       ├── dialog.tsx
│   │       ├── dropdown-menu.tsx
│   │       └── ... (avatar, badge, etc.)
│   │
│   ├── hooks/
│   │   ├── applications/               # Application hooks
│   │   ├── athletes/                   # Athlete hooks
│   │   ├── programs/                   # Program hooks
│   │   └── exercises/                  # Exercise hooks
│   │
│   ├── lib/
│   │   └── api/
│   │       ├── client.ts               # Axios instance
│   │       ├── endpoints.ts
│   │       ├── applications-api.ts
│   │       ├── athletes-api.ts
│   │       ├── programs-api.ts
│   │       ├── exercises-api.ts
│   │       └── index.ts
│   │
│   ├── providers/
│   │   ├── index.ts
│   │   ├── query-provider.tsx          # TanStack Query
│   │   ├── coach-provider.tsx          # Coach context
│   │   └── builder-provider.tsx        # Program Builder state (useReducer)
│   │
│   ├── types/
│   │   ├── index.ts
│   │   ├── common.ts
│   │   ├── application.ts
│   │   ├── athlete.ts
│   │   ├── exercise.ts
│   │   ├── program.ts
│   │   └── builder.ts                  # BuilderWeek, BuilderDay, BuilderExercise, BuilderAction
│   │
│   └── utils/
│       ├── index.ts
│       ├── cn.ts                       # clsx + tailwind-merge
│       └── format-date.ts
│
├── public/
├── package.json
├── tailwind.config.ts
├── tsconfig.json
└── next.config.ts
```

---

## 🔗 Dependencias entre Proyectos

```
                    ┌─────────────────┐
                    │   API           │
                    └────────┬────────┘
                             │ references
              ┌──────────────┼──────────────┐
              ▼              ▼              │
    ┌─────────────────┐ ┌─────────────────┐ │
    │  Application    │ │ Infrastructure  │ │
    └────────┬────────┘ └────────┬────────┘ │
             │                   │          │
             └─────────┬─────────┘          │
                       ▼                    │
              ┌─────────────────┐           │
              │     Domain      │◄──────────┘
              └─────────────────┘

Tests:
- UnitTests → Domain, Application
- IntegrationTests → All projects
```

| Proyecto | Referencias |
|----------|-------------|
| Domain | (ninguna - capa pura) |
| Application | Domain |
| Infrastructure | Application, Domain |
| API | Application, Infrastructure |
| UnitTests | Domain, Application |
| IntegrationTests | Todos |

---

## 📋 Decisiones de Arquitectura (ADRs)

### ADR-001: Clean Architecture

**Estado**: ✅ Aceptada  
**Fecha**: 2026-03-05

**Contexto**:  
Necesitamos una arquitectura que permita escalar el sistema, facilite el testing y mantenga el código organizado.

**Decisión**:  
Usar Clean Architecture con 4 capas: Domain, Application, Infrastructure, API.

**Consecuencias**:
- ✅ Separación clara de responsabilidades
- ✅ Domain sin dependencias externas (testeable, portable)
- ✅ Fácil de cambiar infraestructura (DB, servicios externos)
- ⚠️ Más archivos y carpetas que un monolito simple
- ⚠️ Curva de aprendizaje inicial

---

### ADR-002: CQRS Ligero con MediatR

**Estado**: ✅ Aceptada  
**Fecha**: 2026-03-05

**Contexto**:  
Queremos separar operaciones de lectura y escritura sin la complejidad de Event Sourcing.

**Decisión**:  
Usar MediatR para implementar CQRS ligero:
- Commands para operaciones de escritura
- Queries para operaciones de lectura
- Pipeline behaviors para cross-cutting concerns

**Consecuencias**:
- ✅ Handlers pequeños y enfocados (SRP)
- ✅ Fácil de testear cada handler individualmente
- ✅ Pipeline extensible (logging, validation, caching)
- ⚠️ No es CQRS completo (misma DB para lectura/escritura)
- ⚠️ Overhead de indirección via MediatR

---

### ADR-003: Multi-tenant Ready (CoachId en Entidades)

**Estado**: ✅ Aceptada  
**Fecha**: 2026-03-05

**Contexto**:  
El sistema inicialmente será usado por un solo coach, pero debe poder evolucionar a multi-coach/multi-tenant.

**Decisión**:  
Incluir `CoachId` como foreign key en todas las entidades principales desde el inicio:
- Athlete → CoachId
- Application → CoachId  
- Plan → CoachId

**Consecuencias**:
- ✅ Evolución a multi-tenant sin refactoring de schema
- ✅ Queries ya filtradas por CoachId
- ✅ Datos aislados por diseño
- ⚠️ Slightly more complex queries inicialmente
- ⚠️ FK adicional en cada tabla

---

### ADR-004: Repository Pattern

**Estado**: ✅ Aceptada  
**Fecha**: 2026-03-05

**Contexto**:  
Necesitamos abstraer el acceso a datos para facilitar testing y posibles cambios de ORM.

**Decisión**:  
Implementar Repository Pattern con:
- `IRepository<T>` interfaz genérica
- Interfaces específicas por entidad cuando sea necesario
- `IUnitOfWork` para transacciones

**Consecuencias**:
- ✅ Handlers no dependen directamente de EF Core
- ✅ Fácil mockear repositorios en tests
- ✅ Centralización de queries complejas
- ⚠️ Capa adicional de abstracción
- ⚠️ Debate sobre si es necesario sobre EF Core

---

### ADR-005: Value Objects para Validación

**Estado**: ✅ Aceptada  
**Fecha**: 2026-03-05

**Contexto**:  
Campos como Email, Money y PersonName tienen reglas de validación que deberían estar encapsuladas.

**Decisión**:  
Crear Value Objects inmutables para:
- `Email`: Validación de formato, unicidad implícita
- `Money`: Amount + Currency, operaciones matemáticas
- `PersonName`: FirstName + LastName, formato consistente

**Consecuencias**:
- ✅ Validación encapsulada en el tipo
- ✅ Inmutabilidad garantizada
- ✅ Domain más expresivo (DDD)
- ⚠️ Requiere conversión en EF Core (OwnsOne/ValueConverter)
- ⚠️ Más clases en el dominio

---

### ADR-006: PostgreSQL como Base de Datos

**Estado**: ✅ Aceptada  
**Fecha**: 2026-03-05

**Contexto**:  
Necesitamos una base de datos relacional robusta, compatible con Supabase.

**Decisión**:  
Usar PostgreSQL como base de datos principal.

**Consecuencias**:
- ✅ Compatible con Supabase (hosting managed)
- ✅ Soporte excelente para JSON, arrays, full-text search
- ✅ Open source, sin costos de licencia
- ✅ Amplia comunidad y documentación
- ⚠️ Diferencias de sintaxis con SQL Server si se migra

---

### ADR-007: Fluent API sobre Data Annotations

**Estado**: ✅ Aceptada  
**Fecha**: 2026-03-05

**Contexto**:  
Necesitamos configurar el mapeo de entidades a tablas en EF Core.

**Decisión**:  
Usar Fluent API en archivos de Configuration separados, no Data Annotations.

**Consecuencias**:
- ✅ Entidades del dominio limpias (sin atributos de EF)
- ✅ Configuración centralizada y organizada
- ✅ Mayor flexibilidad (algunas configs solo en Fluent API)
- ⚠️ Más archivos de configuración
- ⚠️ Menos visible la configuración al leer la entidad

---

### ADR-008: Training Programs con Templates

**Estado**: ✅ Aceptada  
**Fecha**: 2026-03-12

**Contexto**:  
Los coaches necesitan crear programas de entrenamiento reutilizables y asignarlos a atletas.

**Decisión**:  
Implementar un sistema de templates jerárquico:
- `ProgramTemplate` → `ProgramWeekTemplate` → `ProgramDayTemplate` → `ProgramExerciseTemplate`
- `AthleteProgram` como instancia de un template asignado a un atleta
- `AthleteWorkout` y `AthleteExerciseLog` para tracking de progreso

**Consecuencias**:
- ✅ Templates reutilizables para múltiples atletas
- ✅ Flexibilidad para customización por atleta
- ✅ Tracking granular de progreso (sets, reps, pesos)
- ⚠️ Estructura jerárquica compleja (4 niveles)
- ⚠️ Requiere eager loading cuidadoso para evitar N+1

---

### ADR-009: Next.js 14 con App Router para Frontend

**Estado**: ✅ Aceptada  
**Fecha**: 2026-03-12

**Contexto**:  
Necesitamos un framework moderno para el dashboard del coach con buen DX y rendimiento.

**Decisión**:  
Usar Next.js 14 con:
- App Router (RSC ready, layouts anidados)
- TailwindCSS + ShadCN UI (componentes accesibles)
- TanStack Query (server state management)
- Axios (HTTP client con interceptors)

**Consecuencias**:
- ✅ Server Components para mejor rendimiento
- ✅ Layouts compartidos y navegación optimizada
- ✅ Componentes UI accesibles y consistentes
- ✅ Caching y revalidación automática de datos
- ⚠️ Curva de aprendizaje con App Router
- ⚠️ Potenciales conflictos RSC vs Client Components

---

### ADR-010: Program Builder con useReducer + Context

**Estado**: ✅ Aceptada  
**Fecha**: 2026-03-13

**Contexto**:  
El Program Builder necesita manejar estado complejo y anidado (semanas → días → ejercicios) con múltiples acciones de usuario (editar, duplicar, reordenar, colapsar).

**Decisión**:  
Implementar el estado del builder usando:
- `useReducer` para manejar 18+ acciones de estado predecibles
- React Context (`BuilderProvider`) para compartir estado entre componentes
- Tipos locales (`BuilderWeek`, `BuilderDay`, `BuilderExercise`) con flags de estado (`tempId`, `isNew`, `isDirty`, `isCollapsed`)
- Sincronización explícita con API (no optimistic updates automáticos)

**Arquitectura de Componentes**:
```
ProgramBuilderPage
└── BuilderProvider (Context + Reducer)
    ├── Header (Save status, Ctrl+S)
    └── WeekBlock[]
        └── DayBlock[]
            └── ExerciseTable
                └── ExerciseRow[] (inline editing, TAB navigation)
```

**Consecuencias**:
- ✅ Estado predecible con acciones explícitas (debugging con React DevTools)
- ✅ Componentes desacoplados vía Context
- ✅ Control total sobre cuándo sincronizar con API
- ✅ UX fluida sin latencia de red en cada keystroke
- ⚠️ Más boilerplate que useState para casos simples
- ⚠️ Requiere gestión manual de "unsaved changes"

---

## 🗃️ Modelo de Datos

### Diagrama Entidad-Relación

```
┌─────────────┐       ┌─────────────┐       ┌─────────────┐
│   COACH     │       │   ATHLETE   │       │  CHECK_IN   │
├─────────────┤       ├─────────────┤       ├─────────────┤
│ Id (PK)     │◄──┐   │ Id (PK)     │◄──────│ Id (PK)     │
│ Name        │   │   │ CoachId(FK) │───┐   │ AthleteId   │
│ Email       │   │   │ Name        │   │   │ Date        │
│ Bio         │   │   │ Email       │   │   │ Weight      │
│ IsActive    │   │   │ Phone       │   │   │ Notes       │
│ CreatedAt   │   │   │ Goals       │   │   │ PhotoUrls   │
│ UpdatedAt   │   │   │ StartDate   │   │   │ CoachFeedback│
└─────────────┘   │   │ CreatedAt   │   │   │ CreatedAt   │
                  │   │ UpdatedAt   │   │   │ UpdatedAt   │
                  │   └─────────────┘   │   └─────────────┘
                  │          │          │
                  │          │          │
┌─────────────┐   │   ┌──────▼──────┐   │
│ APPLICATION │   │   │SUBSCRIPTION │   │
├─────────────┤   │   ├─────────────┤   │
│ Id (PK)     │   │   │ Id (PK)     │   │
│ CoachId(FK) │───┘   │ AthleteId   │───┘
│ Name        │       │ PlanId (FK) │───┐
│ Email       │       │ StartDate   │   │
│ Goals       │       │ EndDate     │   │
│ Status      │       │ Status      │   │
│ Notes       │       │ CreatedAt   │   │
│ CreatedAt   │       │ UpdatedAt   │   │
│ UpdatedAt   │       └─────────────┘   │
└─────────────┘              │          │
                             │          │
                      ┌──────▼──────┐   │    ┌─────────────┐
                      │   PAYMENT   │   │    │    PLAN     │
                      ├─────────────┤   │    ├─────────────┤
                      │ Id (PK)     │   │    │ Id (PK)     │
                      │SubscriptionId   │    │ CoachId(FK) │
                      │ Amount      │   └────│ Name        │
                      │ Currency    │        │ Description │
                      │ PaymentDate │        │ Price       │
                      │ Status      │        │ Currency    │
                      │ Reference   │        │ DurationDays│
                      │ CreatedAt   │        │ PlanType    │
                      │ UpdatedAt   │        │ IsActive    │
                      └─────────────┘        │ CreatedAt   │
                                             │ UpdatedAt   │
                                             └─────────────┘
```

### Relaciones

| Relación | Tipo | Descripción |
|----------|------|-------------|
| Coach → Athletes | 1:N | Un coach tiene muchos atletas |
| Coach → Applications | 1:N | Un coach recibe muchas postulaciones |
| Coach → Plans | 1:N | Un coach define múltiples planes |
| Coach → ProgramTemplates | 1:N | Un coach crea múltiples plantillas de programas |
| Athlete → Subscriptions | 1:N | Un atleta puede tener múltiples suscripciones (histórico) |
| Athlete → CheckIns | 1:N | Un atleta registra múltiples check-ins |
| Athlete → AthletePrograms | 1:N | Un atleta puede tener múltiples programas asignados |
| Plan → Subscriptions | 1:N | Un plan puede tener múltiples suscripciones |
| Subscription → Payments | 1:N | Una suscripción puede tener múltiples pagos |
| ProgramTemplate → Weeks | 1:N | Un programa tiene múltiples semanas |
| ProgramWeekTemplate → Days | 1:N | Una semana tiene múltiples días |
| ProgramDayTemplate → Exercises | 1:N | Un día tiene múltiples ejercicios |
| AthleteProgram → AthleteWorkouts | 1:N | Un programa asignado tiene múltiples workouts |
| AthleteWorkout → AthleteExerciseLogs | 1:N | Un workout tiene múltiples logs de ejercicios |

---

## 🔐 Seguridad (Futuro)

> Pendiente de implementación en fase posterior

- **Autenticación**: JWT Bearer tokens
- **Autorización**: Role-based (Coach, Athlete, Admin)
- **Multi-tenant**: Filtro global por CoachId
- **Rate Limiting**: Por IP y por usuario
- **HTTPS**: Obligatorio en producción

---

## 📊 Métricas y Observabilidad (Futuro)

> Pendiente de implementación en fase posterior

- **Logging**: Serilog con structured logging
- **Health Checks**: Endpoints /health, /ready
- **Tracing**: OpenTelemetry
- **Métricas**: Prometheus/Grafana

---

## 🚀 Deployment

### Ambientes

| Ambiente | Base de Datos | URL |
|----------|---------------|-----|
| Development | PostgreSQL local / Docker | localhost:5001 |
| Staging | Supabase (proyecto staging) | TBD |
| Production | Supabase (proyecto prod) | TBD |

### Variables de Entorno

```bash
# Database
ConnectionStrings__DefaultConnection=Host=...;Database=...;Username=...;Password=...

# Application
ASPNETCORE_ENVIRONMENT=Development|Staging|Production

# Future: Auth
# JWT__Secret=...
# JWT__Issuer=...
# JWT__Audience=...
```

---

## 📚 Referencias

- [Clean Architecture - Jason Taylor Template](https://github.com/jasontaylordev/CleanArchitecture)
- [MediatR Documentation](https://github.com/jbogard/MediatR)
- [Entity Framework Core Docs](https://docs.microsoft.com/ef/core/)
- [PostgreSQL Documentation](https://www.postgresql.org/docs/)
- [Supabase Documentation](https://supabase.com/docs)
