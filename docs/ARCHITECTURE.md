# CoachPlatform — Arquitectura

> Documentación técnica de arquitectura y decisiones de diseño.
> **Última actualización**: 2026-04-24

---

## 📐 Stack Tecnológico

### Backend

| Categoría | Tecnología | Versión |
|-----------|------------|---------|
| **Runtime** | .NET | 9.0 |
| **Framework** | ASP.NET Core Web API | 9.0 |
| **ORM** | Entity Framework Core | 9.x |
| **CQRS** | MediatR | 14.1 |
| **Validación** | FluentValidation | 12.1 |
| **Base de Datos** | PostgreSQL | 16+ |
| **Autenticación** | JWT Bearer (HS256) | 8.x |
| **Hashing** | BCrypt.Net | 1.6 |
| **Contenedores** | Docker + Docker Compose | - |
| **Testing** | xUnit + FluentAssertions + Moq | - |

### Frontend (coach-dashboard)

| Categoría | Tecnología | Versión |
|-----------|------------|---------|
| **Framework** | Next.js (App Router + Turbopack) | 16.2 |
| **UI Library** | React | 19.2 |
| **Lenguaje** | TypeScript | 5.x |
| **Estilos** | TailwindCSS | 3.4 |
| **Componentes** | Radix UI + CVA (ShadCN pattern) | - |
| **Data Fetching** | TanStack Query | 5.x |
| **HTTP Client** | Axios | 1.13 |
| **Charts** | Recharts | 3.8 |
| **DnD** | @dnd-kit/core + sortable | - |
| **PDF** | @react-pdf/renderer | - |
| **Excel** | xlsx | 0.18 |
| **Toasts** | Sonner | 2.0 |
| **Dates** | date-fns | 4.1 |
| **Testing** | Jest + Testing Library | 30.x |

---

## 🏗️ Diagrama de Arquitectura

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                              FRONTEND                                       │
│  ┌───────────────────────────────────────────────────────────────────────┐  │
│  │                   coach-dashboard (Next.js 16 + Turbopack)            │  │
│  │  Pages │ Components │ Hooks │ Providers │ API Client │ TanStack Query│  │
│  │                (React 19 + TypeScript + TailwindCSS)                  │  │
│  └───────────────────────────────────────────────────────────────────────┘  │
├─────────────────────────────────────────────────────────────────────────────┤
│                                  ▼  REST API (JWT Bearer)                   │
├─────────────────────────────────────────────────────────────────────────────┤
│                              PRESENTATION                                   │
│  ┌───────────────────────────────────────────────────────────────────────┐  │
│  │                     CoachPlatform.API                                 │  │
│  │  Controllers │ Middleware │ Auth Policies │ Swagger │ Health Checks  │  │
│  └───────────────────────────────────────────────────────────────────────┘  │
├─────────────────────────────────────────────────────────────────────────────┤
│                              APPLICATION                                    │
│  ┌───────────────────────────────────────────────────────────────────────┐  │
│  │                   CoachPlatform.Application                           │  │
│  │  Commands │ Queries │ Handlers │ DTOs │ Validators │ Behaviors       │  │
│  │                    (MediatR + FluentValidation)                       │  │
│  └───────────────────────────────────────────────────────────────────────┘  │
├─────────────────────────────────────────────────────────────────────────────┤
│                                DOMAIN                                       │
│  ┌───────────────────────────────────────────────────────────────────────┐  │
│  │                     CoachPlatform.Domain                              │  │
│  │  Entities (19) │ Value Objects (3) │ Enums (11) │ Interfaces (10)   │  │
│  │                    (Sin dependencias externas)                        │  │
│  └───────────────────────────────────────────────────────────────────────┘  │
├─────────────────────────────────────────────────────────────────────────────┤
│                             INFRASTRUCTURE                                  │
│  ┌───────────────────────────────────────────────────────────────────────┐  │
│  │                   CoachPlatform.Infrastructure                        │  │
│  │  DbContext │ Repositories (10) │ Configurations (19) │ Services (3) │  │
│  │                    (EF Core + Npgsql + JWT)                           │  │
│  └───────────────────────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────────────────────┘
                                      │
                                      ▼
                        ┌───────────────────────┐
                        │   PostgreSQL 16        │
                        │   (Docker / Supabase)  │
                        └───────────────────────┘
```

---

## 📦 Estructura de Proyectos

### Solución .NET

```
CoachPlatform/
├── src/
│   ├── CoachPlatform.Domain/              # Entidades, VOs, Enums, Interfaces
│   │   ├── Common/
│   │   │   ├── BaseEntity.cs              # Guid Id, equality
│   │   │   ├── AuditableEntity.cs         # CreatedAt, UpdatedAt (UTC)
│   │   │   ├── IAggregateRoot.cs          # Marker interface
│   │   │   └── ValueObject.cs             # Structural equality
│   │   ├── Entities/
│   │   │   ├── Coach.cs                   # Aggregate Root
│   │   │   ├── Athlete.cs                 # Aggregate Root
│   │   │   ├── User.cs                    # Aggregate Root (Auth)
│   │   │   ├── Application.cs             # Aggregate Root (Postulaciones)
│   │   │   ├── Exercise.cs                # Aggregate Root (Catálogo global)
│   │   │   ├── Plan.cs                    # Aggregate Root (Membresías)
│   │   │   ├── Subscription.cs            # Suscripción Athlete-Plan
│   │   │   ├── Payment.cs                 # Pagos
│   │   │   ├── CheckIn.cs                 # Check-ins periódicos
│   │   │   ├── TrainingCycle.cs           # Ciclos de entrenamiento
│   │   │   ├── WorkoutLog.cs              # Logs históricos
│   │   │   ├── ProgramTemplate.cs         # Plantilla de programa
│   │   │   ├── ProgramWeekTemplate.cs     # Semana del programa
│   │   │   ├── ProgramDayTemplate.cs      # Día del programa
│   │   │   ├── ProgramExerciseTemplate.cs # Ejercicio planificado
│   │   │   ├── AthleteProgram.cs          # Programa asignado
│   │   │   ├── AthleteWorkout.cs          # Workout ejecutado
│   │   │   ├── AthleteExerciseLog.cs      # Log de ejercicio individual
│   │   │   └── AthleteMaxLift.cs          # 1RM records
│   │   ├── ValueObjects/
│   │   │   ├── Email.cs                   # Validación regex, lowercase
│   │   │   ├── Money.cs                   # Amount + Currency
│   │   │   └── PersonName.cs              # FirstName + LastName
│   │   ├── Enums/
│   │   │   ├── UserRole.cs                # Coach, Athlete, Admin
│   │   │   ├── ApplicationStatus.cs       # Pending → Accepted/Rejected/Withdrawn
│   │   │   ├── AthleteStatus.cs           # Active, OnHold, Inactive
│   │   │   ├── ExerciseCategory.cs        # Squat, Bench, Deadlift, etc.
│   │   │   ├── ExerciseType.cs            # Standard, Emom, Tempo, Superset, Circuit
│   │   │   ├── MuscleGroup.cs             # 13 grupos musculares
│   │   │   ├── DayFocus.cs                # Squat, Bench, Deadlift, Hypertrophy, etc.
│   │   │   ├── ProgramStatus.cs           # NotStarted → Active → Completed
│   │   │   ├── WorkoutStatus.cs           # NotStarted → InProgress → Completed/Skipped
│   │   │   ├── PlanType.cs                # Monthly, Quarterly, etc.
│   │   │   ├── SubscriptionStatus.cs      # Active, Paused, Cancelled, Expired
│   │   │   └── PaymentStatus.cs           # Pending, Completed, Failed, Refunded
│   │   ├── DomainEvents/                  # (vacío — preparado para futuro)
│   │   └── Interfaces/
│   │       ├── IRepository.cs             # CRUD genérico
│   │       ├── IUnitOfWork.cs             # Transacciones
│   │       ├── ICoachRepository.cs
│   │       ├── IAthleteRepository.cs
│   │       ├── IApplicationRepository.cs
│   │       ├── ICheckInRepository.cs
│   │       ├── IPlanRepository.cs
│   │       ├── ISubscriptionRepository.cs
│   │       ├── IPaymentRepository.cs
│   │       └── IUserRepository.cs
│   │
│   ├── CoachPlatform.Application/         # CQRS, DTOs, Validators
│   │   ├── Shared/
│   │   │   ├── Behaviors/
│   │   │   │   ├── ValidationBehavior.cs          # FluentValidation pipeline
│   │   │   │   ├── LoggingBehavior.cs             # Request logging
│   │   │   │   └── TenantValidationBehavior.cs    # Multi-tenant validation
│   │   │   ├── DTOs/
│   │   │   │   ├── AthleteDtos.cs
│   │   │   │   ├── ApplicationDtos.cs
│   │   │   │   ├── CheckInDtos.cs
│   │   │   │   ├── CoachDtos.cs
│   │   │   │   ├── DashboardDtos.cs
│   │   │   │   ├── MaxLiftDto.cs
│   │   │   │   ├── PagedResult.cs
│   │   │   │   ├── PaymentDtos.cs
│   │   │   │   ├── PlanDtos.cs
│   │   │   │   ├── SubscriptionDtos.cs
│   │   │   │   ├── TrainingCycleDtos.cs
│   │   │   │   ├── TrainingProgramDtos.cs
│   │   │   │   ├── WorkoutLogDtos.cs
│   │   │   │   └── WorkoutTrackingDtos.cs
│   │   │   ├── Exceptions/
│   │   │   │   ├── ValidationException.cs         # → 400
│   │   │   │   ├── NotFoundException.cs           # → 404
│   │   │   │   ├── ConflictException.cs           # → 409
│   │   │   │   └── ForbiddenAccessException.cs    # → 403
│   │   │   └── Interfaces/
│   │   │       ├── IApplicationDbContext.cs
│   │   │       ├── ICurrentUserService.cs
│   │   │       ├── IJwtService.cs
│   │   │       ├── ITenantService.cs
│   │   │       ├── ITenantRequest.cs
│   │   │       └── IAthleteOwnedRequest.cs
│   │   ├── Features/
│   │   │   ├── Applications/     # CreateApplication, Approve, Reject, GetAll, GetById
│   │   │   ├── Athletes/         # Create, Update, Deactivate, RegisterMaxLift, GetByCoach, GetById, GetMaxLifts, GetExerciseHistory
│   │   │   ├── Authentication/   # Login, CreateUser, UpdateUser, ResetPassword, RefreshToken, Logout, GetUsers
│   │   │   ├── CheckIns/         # Create, Review, GetCoach, GetById, GetByAthlete
│   │   │   ├── Exercises/        # Create, Update, Delete, GetAll, GetById (global, sin tenant)
│   │   │   ├── Payments/         # Register, GetAll, GetByAthlete
│   │   │   ├── Plans/            # Create, Update, Deactivate, GetAll
│   │   │   ├── Subscriptions/    # Create, Cancel, GetByAthlete
│   │   │   ├── TrainingCycles/   # Create, GetByAthlete
│   │   │   ├── TrainingPrograms/ # Create, Update, SaveFull, AddWeek/Day/Exercise, Delete, Assign, GetAll, GetById, GetAthleteProgram
│   │   │   └── WorkoutLogs/      # StartWorkout, SaveSet, UpdateSet, CompleteSet, SkipWorkout, CompleteWorkout, GetToday, GetDetail, GetWeek, GetHistory, GetLiftHistory, GetAll
│   │   └── DependencyInjection.cs
│   │
│   ├── CoachPlatform.Infrastructure/      # EF Core, Repos, Services
│   │   ├── Persistence/
│   │   │   ├── ApplicationDbContext.cs    # 18 DbSets, auto-audit
│   │   │   ├── Configurations/            # 19 archivos Fluent API
│   │   │   ├── Repositories/
│   │   │   │   ├── BaseRepository.cs
│   │   │   │   ├── CoachRepository.cs
│   │   │   │   ├── AthleteRepository.cs
│   │   │   │   ├── ApplicationRepository.cs
│   │   │   │   ├── PlanRepository.cs
│   │   │   │   ├── SubscriptionRepository.cs
│   │   │   │   ├── PaymentRepository.cs
│   │   │   │   ├── CheckInRepository.cs
│   │   │   │   ├── UserRepository.cs
│   │   │   │   └── UnitOfWork.cs
│   │   │   └── Migrations/
│   │   │       ├── AddProgramBuilderFields
│   │   │       ├── AddWorkoutTrackingFields
│   │   │       ├── AddUserAuthentication
│   │   │       └── MakeExercisesGlobal
│   │   ├── Services/
│   │   │   ├── JwtService.cs              # Access + Refresh tokens (HS256)
│   │   │   ├── CurrentUserService.cs      # HttpContext claims
│   │   │   └── TenantService.cs           # AsyncLocal<Guid?> tenant
│   │   └── DependencyInjection.cs
│   │
│   └── CoachPlatform.API/
│       ├── Program.cs                     # JWT Auth, Policies, Swagger, CORS, Auto-migration, Admin seed
│       ├── Middleware/
│       │   ├── ExceptionHandlingMiddleware.cs  # Exception → HTTP status mapping
│       │   └── TenantMiddleware.cs             # JWT claims → TenantService
│       ├── Controllers/
│       │   ├── AuthController.cs               # /api/auth
│       │   ├── DashboardController.cs          # /api/dashboard
│       │   ├── ApplicationsController.cs       # /api/applications
│       │   ├── AthletesController.cs           # /api/athletes
│       │   ├── ExercisesController.cs          # /api/exercises (global)
│       │   ├── CheckInsController.cs           # /api/check-ins
│       │   ├── TrainingCyclesController.cs     # /api/trainingcycles
│       │   ├── PlansController.cs              # /api/plans
│       │   ├── SubscriptionsController.cs      # /api/subscriptions
│       │   ├── PaymentsController.cs           # /api/payments
│       │   └── WorkoutsController.cs           # /api/workouts
│       ├── appsettings.json
│       └── appsettings.Development.json
│
├── tests/
│   ├── CoachPlatform.UnitTests/               # 204+ tests
│   │   ├── Domain/
│   │   │   ├── Entities/ (5 test files)
│   │   │   └── ValueObjects/ (2 test files)
│   │   └── Application/
│   │       ├── Commands/ (19 test files)
│   │       └── Queries/ (3 test files)
│   └── CoachPlatform.IntegrationTests/        # Skeleton
│
├── docs/
│   ├── ARCHITECTURE.md
│   ├── BACKLOG.md
│   └── F-015_WORKOUT_TRACKING_PLAN.md
│
├── CoachPlatform.sln
├── docker-compose.yml
├── Dockerfile
└── nuget.config
```

### Estructura Frontend (coach-dashboard)

```
coach-dashboard/
├── src/
│   ├── app/
│   │   ├── layout.tsx                         # Root layout
│   │   ├── page.tsx                           # Redirect → /dashboard
│   │   ├── globals.css
│   │   ├── (auth)/
│   │   │   └── login/page.tsx                 # Login
│   │   ├── apply/page.tsx                     # Formulario público de postulación
│   │   └── (dashboard)/
│   │       ├── layout.tsx                     # Sidebar + Topbar
│   │       ├── dashboard/
│   │       │   ├── page.tsx                   # Coach dashboard
│   │       │   ├── components/                # AlertsPanel, ActivityFeed, AthleteStatusTable
│   │       │   └── lib/mock-dashboard-data.ts
│   │       ├── admin/users/page.tsx           # Admin: user management
│   │       ├── applications/
│   │       │   ├── page.tsx                   # Lista de postulaciones
│   │       │   └── [id]/page.tsx              # Detalle de postulación
│   │       ├── athletes/
│   │       │   ├── page.tsx                   # Lista de atletas
│   │       │   └── [id]/page.tsx              # Detalle de atleta
│   │       ├── check-ins/
│   │       │   ├── page.tsx                   # Coach: lista de check-ins
│   │       │   └── [id]/page.tsx              # Detalle de check-in
│   │       ├── programs/
│   │       │   ├── page.tsx                   # Lista de programas
│   │       │   └── [id]/builder/page.tsx      # Program Builder (editor tipo Notion)
│   │       ├── exercises/page.tsx             # Catálogo global de ejercicios
│   │       ├── plans/page.tsx                 # Planes de membresía
│   │       ├── subscriptions/page.tsx         # Suscripciones
│   │       ├── payments/page.tsx              # Pagos
│   │       ├── settings/page.tsx              # Configuración
│   │       ├── athlete/
│   │       │   ├── page.tsx                   # Athlete dashboard
│   │       │   └── check-in/page.tsx          # Athlete: formulario check-in
│   │       └── workout/
│   │           ├── page.tsx                   # Workout tracking en tiempo real
│   │           ├── components/                # WorkoutHeader, ExerciseCard, SetRow,
│   │           │                              # RestTimer, PRBadge, WeekStrip, etc.
│   │           └── lib/                       # workout-utils.ts
│   │
│   ├── components/
│   │   ├── builder/                           # ExerciseRow, ExerciseTable, DayBlock, WeekBlock
│   │   ├── charts/                            # Componentes de gráficos
│   │   ├── common/                            # DataTable, PageHeader, StatusBadge, StatsCard, etc.
│   │   ├── athletes/                          # Componentes específicos de atletas
│   │   ├── excel/                             # Exportación Excel
│   │   ├── pdf/                               # Generación PDF
│   │   ├── layout/                            # Sidebar, Topbar
│   │   └── ui/                                # Radix UI primitives (ShadCN pattern)
│   │
│   ├── hooks/
│   │   ├── applications/                      # useApplications, useApplication, etc.
│   │   ├── athletes/                          # useAthletes, useAthlete, etc.
│   │   ├── check-ins/                         # useCheckIns, etc.
│   │   ├── dashboard/                         # useDashboard
│   │   ├── exercises/                         # useExercises, useExercise, etc.
│   │   ├── programs/                          # usePrograms, useProgram, etc.
│   │   └── workouts/                          # useWorkout, useTodayWorkout, etc.
│   │
│   ├── lib/api/
│   │   ├── client.ts                          # Axios instance + interceptors
│   │   ├── endpoints.ts                       # URL definitions
│   │   ├── auth-api.ts
│   │   ├── applications-api.ts
│   │   ├── athletes-api.ts
│   │   ├── dashboard-api.ts
│   │   ├── exercises-api.ts                   # Global (sin coachId)
│   │   ├── programs-api.ts
│   │   ├── workout-tracking-api.ts
│   │   ├── plans-api.ts
│   │   ├── subscriptions-api.ts
│   │   ├── payments-api.ts
│   │   ├── services.ts
│   │   └── types.ts
│   │
│   ├── providers/
│   │   ├── auth-provider.tsx                  # Auth context (login/logout/tokens)
│   │   ├── coach-provider.tsx                 # Coach context
│   │   ├── builder-provider.tsx               # Program Builder state (useReducer)
│   │   └── query-provider.tsx                 # TanStack Query client
│   │
│   ├── config/
│   │   └── navigation.ts                     # Role-based navigation
│   │
│   ├── types/                                 # 13 TypeScript type files
│   │   ├── application.ts, athlete.ts, builder.ts, checkIn.ts
│   │   ├── common.ts, dashboard.ts, exercise.ts, payment.ts
│   │   ├── plan.ts, program.ts, subscription.ts, workout-tracking.ts
│   │   └── index.ts
│   │
│   ├── utils/
│   │   ├── cn.ts                              # clsx + tailwind-merge
│   │   ├── format-date.ts                     # Date formatting helpers
│   │   ├── set-notation-parser.ts             # "1x1 3x4" parsing
│   │   └── weight-calculator.ts               # %RM calculations
│   │
│   ├── proxy.ts                               # Middleware: auth + role routing
│   └── __tests__/                             # Jest tests
│
├── package.json
├── tailwind.config.ts
├── tsconfig.json
├── jest.config.js
├── Dockerfile                                 # node:20-alpine, standalone
└── next.config.ts
```

---

## 🔗 Dependencias entre Proyectos

```
                    ┌─────────────────┐
                    │       API       │
                    └────────┬────────┘
                             │ references
              ┌──────────────┼──────────────┐
              ▼              │              ▼
    ┌─────────────────┐      │    ┌─────────────────┐
    │  Application    │      │    │ Infrastructure  │
    └────────┬────────┘      │    └────────┬────────┘
             │               │             │
             └───────────────┼─────────────┘
                             ▼
                    ┌─────────────────┐
                    │     Domain      │
                    └─────────────────┘
```

| Proyecto | Referencias | Paquetes principales |
|----------|-------------|---------------------|
| **Domain** | (ninguna) | — |
| **Application** | Domain | MediatR 14.1, FluentValidation 12.1, BCrypt.Net 1.6 |
| **Infrastructure** | Application, Domain | Npgsql.EFCore 9.0, JWT 8.x, BCrypt.Net 1.6 |
| **API** | Application, Infrastructure | JwtBearer 9.0, Swashbuckle 7.2 |
| **UnitTests** | Domain, Application | xUnit 2.9, Moq 4.20, FluentAssertions 8.8, EF InMemory |
| **IntegrationTests** | Todos | xUnit, Moq, FluentAssertions |

---

## 🗃️ Modelo de Datos

### Entidades (19 total)

| Entidad | Aggregate Root | Propiedades clave | Relaciones |
|---------|:--------------:|-------------------|-----------|
| **Coach** | ✅ | Name (PersonName), Email (Email VO), Bio, Phone, IsActive | → Athletes, Plans, Applications, Exercises? |
| **Athlete** | ✅ | CoachId, Name, Email, Phone, Goals, Gender, Weight, Height, ExperienceLevel, Status | → Coach, Subscriptions, CheckIns, TrainingCycles, WorkoutLogs |
| **User** | ✅ | Email, Username, PasswordHash, Role (UserRole), CoachId?, AthleteId?, RefreshTokenHash | → Coach?, Athlete? |
| **Exercise** | ✅ | CoachId? (nullable, global), Name, Category, PrimaryMuscleGroup, Equipment, IsCompound, IsActive | → Coach? |
| **Application** | ✅ | CoachId, ApplicantName, Email, Status (ApplicationStatus), Lifts, Motivation | → Coach |
| **ProgramTemplate** | ✅ | CoachId, Name, DurationWeeks (1-52), IsActive | → Coach, Weeks |
| **ProgramWeekTemplate** | ❌ | ProgramTemplateId, WeekNumber, Name | → ProgramTemplate, Days |
| **ProgramDayTemplate** | ❌ | WeekTemplateId, DayNumber (1-7), Name, Focus (DayFocus) | → Week, Exercises |
| **ProgramExerciseTemplate** | ❌ | DayTemplateId, ExerciseId, Sets, Reps (string), TargetRpe, ExerciseType, PercentageRM, Weight | → Day, Exercise |
| **AthleteProgram** | ✅ | AthleteId, ProgramTemplateId, StartDate, EndDate, CurrentWeek, Status (ProgramStatus) | → Athlete, ProgramTemplate, Workouts |
| **AthleteWorkout** | ❌ | AthleteProgramId, WeekNumber, DayNumber, ScheduledDate, Status (WorkoutStatus), DurationMinutes | → AthleteProgram, ExerciseLogs |
| **AthleteExerciseLog** | ❌ | WorkoutId, ExerciseId, SetNumber, TargetReps/Weight, Reps, Weight, Rpe, IsCompleted | → Workout, Exercise |
| **AthleteMaxLift** | ❌ | AthleteId, ExerciseId, Weight (1RM), IsTested, RecordedAt | → Athlete, Exercise |
| **Plan** | ✅ | CoachId, Name, Price (Money VO), DurationDays, PlanType, Features, MaxAthletes, IsActive | → Coach, Subscriptions |
| **Subscription** | ❌ | AthleteId, PlanId, StartDate, EndDate, Status (SubscriptionStatus), Price (Money snapshot), AutoRenew | → Athlete, Plan, Payments |
| **Payment** | ❌ | SubscriptionId, Amount (Money VO), PaymentDate, Status (PaymentStatus), TransactionId | → Subscription |
| **CheckIn** | ❌ | AthleteId, CheckInDate, Weight, EnergyLevel, SleepQuality, SleepHours, StressLevel, CoachFeedback | → Athlete |
| **TrainingCycle** | ❌ | AthleteId, Name, DurationWeeks, StartDate, EndDate | → Athlete |
| **WorkoutLog** | ❌ | AthleteId, ExerciseId, ExerciseName (denorm), Sets, Reps, Weight, RPE, WorkoutDate | → Athlete, Exercise |

### Value Objects (3)

| Value Object | Propiedades | Validaciones |
|-------------|-------------|-------------|
| **Email** | `Value` (string) | Regex, lowercase, max 256 chars |
| **PersonName** | `FirstName`, `LastName`, `FullName` (computed) | Normalized, max 100 chars each |
| **Money** | `Amount` (decimal), `Currency` (3-letter code) | Non-negative. Helpers: `Usd()`, `Eur()`, `Mxn()` |

### Enums (11)

| Enum | Valores |
|------|---------|
| `UserRole` | Coach, Athlete, Admin |
| `ApplicationStatus` | Pending, UnderReview, Accepted, Rejected, Withdrawn |
| `AthleteStatus` | Active, OnHold, Inactive |
| `ExerciseCategory` | Squat, Bench, Deadlift, OverheadPress, Row, Accessory, Core, Conditioning, Stretching |
| `ExerciseType` | Standard, Emom, Tempo, Superset, Circuit |
| `MuscleGroup` | Chest, Back, Shoulders, Legs, Quads, Hamstrings, Glutes, Biceps, Triceps, Core, Forearms, Calves, FullBody |
| `DayFocus` | Squat, Bench, Deadlift, Hypertrophy, UpperBody, LowerBody, FullBody, Accessory, Recovery, Competition |
| `ProgramStatus` | NotStarted, Active, Paused, Completed, Cancelled |
| `WorkoutStatus` | NotStarted, InProgress, Completed, Skipped, Partial |
| `PlanType` | Monthly, Quarterly, SemiAnnual, Annual, Custom |
| `SubscriptionStatus` | Active, Paused, Cancelled, Expired, PendingPayment |
| `PaymentStatus` | Pending, Completed, Failed, Refunded, Cancelled |

### Diagrama Entidad-Relación

```
┌──────────┐          ┌──────────────┐         ┌──────────────┐
│   USER   │          │    COACH     │         │  APPLICATION │
├──────────┤          ├──────────────┤         ├──────────────┤
│ Id (PK)  │    ┌────▶│ Id (PK)      │◀────────│ CoachId (FK) │
│ Email    │    │     │ Name (VO)    │         │ Name, Email  │
│ Role     │    │     │ Email (VO)   │         │ Status       │
│ CoachId? │────┘     │ Bio, Phone   │         │ Lifts, Goals │
│ AthleteId│──┐      │ IsActive     │         └──────────────┘
└──────────┘  │      └──────┬───────┘
              │             │ 1:N
              │      ┌──────▼───────┐         ┌──────────────┐
              │      │   ATHLETE    │         │   CHECK_IN   │
              │      ├──────────────┤         ├──────────────┤
              └─────▶│ Id (PK)      │◀────────│ AthleteId    │
                     │ CoachId (FK) │    1:N  │ Weight, Sleep│
                     │ Name, Email  │         │ CoachFeedback│
                     │ Status       │         └──────────────┘
                     │ Goals, Weight│
                     └──────┬───────┘
                            │
           ┌────────────────┼────────────────┐
           │ 1:N            │ 1:N            │ 1:N
    ┌──────▼───────┐ ┌──────▼───────┐ ┌──────▼───────┐
    │ SUBSCRIPTION │ │ATHLETE_PROG  │ │ TRAINING_    │
    ├──────────────┤ ├──────────────┤ │ CYCLE        │
    │ PlanId (FK)  │ │ TemplateId   │ └──────────────┘
    │ StartDate    │ │ StartDate    │
    │ Status       │ │ CurrentWeek  │
    │ Price (VO)   │ │ Status       │
    └──────┬───────┘ └──────┬───────┘
           │ 1:N            │ 1:N
    ┌──────▼───────┐ ┌──────▼───────┐
    │   PAYMENT    │ │ATHLETE_WKOUT │
    ├──────────────┤ ├──────────────┤
    │ Amount (VO)  │ │ Week, Day    │
    │ Status       │ │ Status       │
    │ TransactionId│ │ Duration     │
    └──────────────┘ └──────┬───────┘
                            │ 1:N
                     ┌──────▼───────┐
                     │ EXERCISE_LOG │
                     ├──────────────┤
                     │ ExerciseId   │
                     │ Set, Reps    │
                     │ Weight, RPE  │
                     └──────────────┘

┌──────────────┐         ┌──────────────────────┐
│  EXERCISE    │         │  PROGRAM_TEMPLATE    │
│  (GLOBAL)    │         ├──────────────────────┤
├──────────────┤         │ CoachId (FK)         │
│ Id (PK)      │    ┌───▶│ Name, DurationWeeks  │
│ CoachId? (FK)│    │    └──────┬───────────────┘
│ Name (unique)│    │           │ 1:N
│ Category     │    │    ┌──────▼───────────────┐
│ MuscleGroup  │    │    │ PROGRAM_WEEK         │
│ Equipment    │    │    │ WeekNumber, Name     │
│ IsCompound   │    │    └──────┬───────────────┘
└──────────────┘    │           │ 1:N
                    │    ┌──────▼───────────────┐
    ┌────────────┐  │    │ PROGRAM_DAY          │
    │    PLAN    │  │    │ DayNumber, Focus     │
    ├────────────┤  │    └──────┬───────────────┘
    │ CoachId    │  │           │ 1:N
    │ Name       │  │    ┌──────▼───────────────┐
    │ Price (VO) │  │    │ PROGRAM_EXERCISE     │
    │ PlanType   │  │    │ ExerciseId, Sets     │
    │ Features   │  │    │ Reps, RPE, Type      │
    │ MaxAthletes│  │    │ %RM, Weight          │
    └────────────┘  │    └─────────────────────┘
                    │
                    └─── AthleteProgram.ProgramTemplateId
```

### Relaciones principales

| Relación | Tipo | FK | Descripción |
|----------|------|----|-------------|
| Coach → Athletes | 1:N | `Athlete.CoachId` | Coach gestiona múltiples atletas |
| Coach → Applications | 1:N | `Application.CoachId` | Coach recibe postulaciones |
| Coach → Plans | 1:N | `Plan.CoachId` | Coach define planes de membresía |
| Coach → ProgramTemplates | 1:N | `ProgramTemplate.CoachId` | Coach crea programas |
| Exercise → Coach | N:1? | `Exercise.CoachId?` | **Opcional** — catálogo global compartido |
| Athlete → Subscriptions | 1:N | `Subscription.AthleteId` | Historial de suscripciones |
| Athlete → CheckIns | 1:N | `CheckIn.AthleteId` | Check-ins periódicos |
| Athlete → AthletePrograms | 1:N | `AthleteProgram.AthleteId` | Programas asignados |
| Athlete → WorkoutLogs | 1:N | `WorkoutLog.AthleteId` | Historial de workouts |
| Athlete → AthleteMaxLifts | 1:N | `AthleteMaxLift.AthleteId` | Records de 1RM |
| Plan → Subscriptions | 1:N | `Subscription.PlanId` | Suscripciones a un plan |
| Subscription → Payments | 1:N | `Payment.SubscriptionId` | Pagos de una suscripción |
| ProgramTemplate → Weeks → Days → Exercises | 1:N:N:N | Cascade | Jerarquía del programa |
| AthleteProgram → Workouts → ExerciseLogs | 1:N:N | Cascade | Ejecución del programa |
| User → Coach | 1:1? | `User.CoachId?` | Cuenta de usuario del coach |
| User → Athlete | 1:1? | `User.AthleteId?` | Cuenta de usuario del atleta |

---

## 🔐 Seguridad y Autenticación

### JWT Authentication

- **Algoritmo**: HS256
- **Access Token**: 15 min TTL
- **Refresh Token**: 7 días TTL (hash almacenado en DB)
- **Claims**: `NameIdentifier`, `Email`, `Role`, `coach_id`, `athlete_id`
- **Clock Skew**: Zero (validación estricta)

### Authorization Policies

| Policy | Roles permitidos |
|--------|-----------------|
| `CoachOnly` | Coach |
| `AthleteOnly` | Athlete |
| `AdminOnly` | Admin |
| `CoachOrAdmin` | Coach, Admin |

### Multi-tenancy

- **TenantMiddleware** resuelve `CoachId` desde:
  1. JWT claim `coach_id`
  2. Rol Coach del usuario
  3. Relación Athlete → Coach (DB lookup)
  4. Header `X-Coach-Id` (dev fallback)
- **TenantValidationBehavior** valida que requests con `ITenantRequest` tengan tenant válido
- **Admin bypass**: Admin puede acceder sin tenant
- **Exercises**: Catálogo **global** (no aplica tenant)

### Exception → HTTP Mapping

| Exception | HTTP Status | Uso |
|-----------|-------------|-----|
| `ValidationException` | 400 Bad Request | Validación FluentValidation |
| `ForbiddenAccessException` | 403 Forbidden | Acceso no autorizado |
| `NotFoundException` | 404 Not Found | Recurso no encontrado |
| `ConflictException` | 409 Conflict | Duplicados, conflictos de estado |
| Unhandled | 500 Internal Server Error | Error inesperado |

---

## 🌐 API REST — Endpoints

### Autenticación (`/api/auth`)

| Método | Ruta | Descripción | Auth |
|--------|------|-------------|------|
| POST | `/api/auth/login` | Login con email/password | ❌ |
| POST | `/api/auth/refresh-token` | Renovar access token | ❌ |
| POST | `/api/auth/logout` | Logout (invalidar refresh) | ✅ |
| GET | `/api/auth/me` | Perfil del usuario actual | ✅ |
| GET | `/api/auth/users` | Listar usuarios | Admin |
| POST | `/api/auth/users` | Crear usuario | Admin |
| PATCH | `/api/auth/users/{userId}` | Actualizar usuario | Admin |
| POST | `/api/auth/users/{userId}/reset-password` | Resetear password | Admin |

### Dashboard (`/api/dashboard`)

| Método | Ruta | Descripción | Auth |
|--------|------|-------------|------|
| GET | `/api/dashboard/coach` | Métricas del coach | Coach |

### Postulaciones (`/api/applications`)

| Método | Ruta | Descripción | Auth |
|--------|------|-------------|------|
| GET | `/api/applications` | Listar postulaciones | Coach |
| GET | `/api/applications/{id}` | Detalle de postulación | Coach |
| POST | `/api/applications` | Enviar postulación | ❌ (público) |
| POST | `/api/applications/{id}/approve` | Aprobar postulación | Coach |
| POST | `/api/applications/{id}/reject` | Rechazar postulación | Coach |

### Atletas (`/api/athletes`)

| Método | Ruta | Descripción | Auth |
|--------|------|-------------|------|
| GET | `/api/athletes` | Listar atletas del coach | Coach |
| GET | `/api/athletes/{id}` | Detalle de atleta | Coach |
| POST | `/api/athletes` | Crear atleta | Coach |
| PUT | `/api/athletes/{id}` | Actualizar atleta | Coach |
| POST | `/api/athletes/{id}/deactivate` | Desactivar atleta | Coach |
| GET | `/api/athletes/{id}/max-lifts` | 1RM records del atleta | Coach |
| POST | `/api/athletes/{id}/max-lifts` | Registrar 1RM | Coach |
| GET | `/api/athletes/{id}/exercise-history/{exerciseId}` | Historial de ejercicio | Coach |

### Ejercicios — Global (`/api/exercises`)

| Método | Ruta | Descripción | Auth |
|--------|------|-------------|------|
| GET | `/api/exercises` | Listar todos los ejercicios | ✅ |
| GET | `/api/exercises/{exerciseId}` | Detalle de ejercicio | ✅ |
| POST | `/api/exercises` | Crear ejercicio | ✅ |
| PUT | `/api/exercises/{exerciseId}` | Actualizar ejercicio | ✅ |
| DELETE | `/api/exercises/{exerciseId}` | Eliminar ejercicio | ✅ |

### Programas de Entrenamiento (`/api/programs`)

| Método | Ruta | Descripción | Auth |
|--------|------|-------------|------|
| GET | `/api/programs` | Listar programas del coach | Coach |
| GET | `/api/programs/{id}` | Detalle con estructura completa | Coach |
| POST | `/api/programs` | Crear programa | Coach |
| PUT | `/api/programs/{id}` | Actualizar metadata | Coach |
| PUT | `/api/programs/{id}/full` | **Bulk save** (toda la estructura) | Coach |
| POST | `/api/programs/{pid}/weeks` | Agregar semana | Coach |
| DELETE | `/api/programs/{pid}/weeks/{wid}` | Eliminar semana | Coach |
| POST | `/api/programs/{pid}/weeks/{wid}/days` | Agregar día | Coach |
| DELETE | `/api/programs/{pid}/days/{did}` | Eliminar día | Coach |
| POST | `/api/programs/{pid}/weeks/{wid}/days/{did}/exercises` | Agregar ejercicio | Coach |
| DELETE | `/api/programs/{pid}/exercises/{eid}` | Eliminar ejercicio del programa | Coach |
| POST | `/api/athletes/{aid}/assign-program` | Asignar programa a atleta | Coach |
| GET | `/api/athletes/{aid}/program` | Programa activo del atleta | Coach |

### Workout Tracking (`/api/athletes/{athleteId}/workouts`)

| Método | Ruta | Descripción | Auth |
|--------|------|-------------|------|
| GET | `.../workouts/today` | Workout del día | ✅ |
| GET | `.../workouts/{workoutId}` | Detalle con sets | ✅ |
| GET | `.../workouts/week` | Workouts de la semana | ✅ |
| GET | `.../workouts/history` | Historial paginado | ✅ |
| GET | `.../workouts/logs` | Todos los logs | ✅ |
| GET | `.../workouts/exercises/{exerciseId}/lift-history` | Historial de levantamientos | ✅ |
| POST | `.../workouts/{wid}/start` | Iniciar workout | ✅ |
| PUT | `.../workouts/{wid}/sets` | Guardar set | ✅ |
| PUT | `.../workouts/{wid}/sets/{logId}` | Actualizar set | ✅ |
| POST | `.../workouts/{wid}/sets/{logId}/complete` | Completar set | ✅ |
| POST | `.../workouts/{wid}/skip` | Saltar workout | ✅ |
| POST | `.../workouts/{wid}/complete` | Completar workout | ✅ |

### Check-ins (`/api/check-ins`)

| Método | Ruta | Descripción | Auth |
|--------|------|-------------|------|
| POST | `/api/check-ins` | Enviar check-in | ✅ |
| GET | `/api/check-ins/coach` | Check-ins del coach | Coach |
| GET | `/api/check-ins/{id}` | Detalle de check-in | ✅ |
| PUT | `/api/check-ins/{id}/review` | Revisar check-in | Coach |
| GET | `/api/athletes/{aid}/checkins` | Check-ins de un atleta | Coach |

### Planes (`/api/plans`)

| Método | Ruta | Descripción | Auth |
|--------|------|-------------|------|
| GET | `/api/plans` | Listar planes | Coach |
| POST | `/api/plans` | Crear plan | Coach |
| PUT | `/api/plans/{id}` | Actualizar plan | Coach |
| POST | `/api/plans/{id}/deactivate` | Desactivar plan | Coach |

### Suscripciones (`/api/subscriptions`)

| Método | Ruta | Descripción | Auth |
|--------|------|-------------|------|
| POST | `/api/subscriptions` | Crear suscripción | Coach |
| POST | `/api/subscriptions/{id}/cancel` | Cancelar suscripción | Coach |
| GET | `/api/athletes/{aid}/subscriptions` | Suscripciones del atleta | Coach |

### Pagos (`/api/payments`)

| Método | Ruta | Descripción | Auth |
|--------|------|-------------|------|
| GET | `/api/payments` | Listar pagos | Coach |
| POST | `/api/payments` | Registrar pago | Coach |
| GET | `/api/athletes/{aid}/payments` | Pagos del atleta | Coach |

### Training Cycles (`/api/trainingcycles`)

| Método | Ruta | Descripción | Auth |
|--------|------|-------------|------|
| POST | `/api/trainingcycles` | Crear ciclo | Coach |
| GET | `/api/athletes/{aid}/training-cycles` | Ciclos del atleta | Coach |

### Otros

| Método | Ruta | Descripción |
|--------|------|-------------|
| GET | `/health` | Health check |
| POST | `/api/workouts` | Log workout (legacy) |

---

## 📋 Decisiones de Arquitectura (ADRs)

### ADR-001: Clean Architecture
**Estado**: ✅ Aceptada | **Fecha**: 2026-03-05

4 capas (Domain, Application, Infrastructure, API). Domain sin dependencias externas. Fácil testing y cambio de infraestructura.

### ADR-002: CQRS Ligero con MediatR
**Estado**: ✅ Aceptada | **Fecha**: 2026-03-05

Commands/Queries separados via MediatR. Pipeline behaviors para cross-cutting (validation, logging, tenant). Misma DB para lectura/escritura (no Event Sourcing).

### ADR-003: Multi-tenant (CoachId en Entidades)
**Estado**: ✅ Aceptada | **Fecha**: 2026-03-05

`CoachId` como FK en entidades principales (Athlete, Application, Plan, ProgramTemplate). Queries filtradas por tenant. **Excepción**: Exercise es global (ver ADR-011).

### ADR-004: Repository Pattern
**Estado**: ✅ Aceptada | **Fecha**: 2026-03-05

`IRepository<T>` genérico + interfaces específicas. `IUnitOfWork` para transacciones. Handlers no dependen de EF Core directamente.

### ADR-005: Value Objects para Validación
**Estado**: ✅ Aceptada | **Fecha**: 2026-03-05

`Email`, `Money`, `PersonName` como VOs inmutables con validación encapsulada. EF Core mapea con `OwnsOne`.

### ADR-006: PostgreSQL
**Estado**: ✅ Aceptada | **Fecha**: 2026-03-05

PostgreSQL 16 como DB principal. Compatible con Supabase. Docker para desarrollo local.

### ADR-007: Fluent API sobre Data Annotations
**Estado**: ✅ Aceptada | **Fecha**: 2026-03-05

Configuración EF Core en archivos `*Configuration.cs` separados. Entidades del dominio limpias sin atributos de infraestructura.

### ADR-008: Training Programs con Templates
**Estado**: ✅ Aceptada | **Fecha**: 2026-03-12

Jerarquía: `ProgramTemplate → Weeks → Days → Exercises`. `AthleteProgram` como instancia asignada. `AthleteWorkout` + `AthleteExerciseLog` para tracking.

### ADR-009: Next.js con App Router
**Estado**: ✅ Aceptada | **Fecha**: 2026-03-12

Next.js 16 con App Router, TailwindCSS + Radix UI (ShadCN pattern), TanStack Query, Axios.

### ADR-010: Program Builder con useReducer + Context
**Estado**: ✅ Aceptada | **Fecha**: 2026-03-13

Estado complejo del builder via `useReducer` + `BuilderProvider`. 18+ acciones de estado. Sincronización explícita con API (bulk save via `PUT /programs/{id}/full`).

```
ProgramBuilderPage
└── BuilderProvider (Context + Reducer)
    ├── Header (Save status, Ctrl+S)
    └── WeekBlock[]
        └── DayBlock[]
            └── ExerciseTable
                └── ExerciseRow[] (inline editing, TAB navigation)
```

### ADR-011: Catálogo de Ejercicios Global
**Estado**: ✅ Aceptada | **Fecha**: 2026-04-24

**Contexto**: Cada coach tenía su propio catálogo de ejercicios, generando duplicados innecesarios.

**Decisión**: Hacer el catálogo de ejercicios global y compartido entre todos los usuarios:
- `Exercise.CoachId` → `Guid?` (nullable)
- FK opcional con `OnDelete(SetNull)`
- Índice único solo por `Name` (no por Coach+Name)
- Endpoints en `/api/exercises` (sin prefijo de coach)
- Sin `ITenantRequest` — cualquier usuario autenticado puede CRUD

**Consecuencias**:
- ✅ Sin ejercicios duplicados entre coaches
- ✅ Catálogo centralizado y mantenible
- ✅ Cualquier coach puede usar cualquier ejercicio
- ⚠️ Un coach podría editar/eliminar ejercicios que otro coach usa
- ⚠️ Sin ownership — considerar roles de edición a futuro

---

## 🐳 Docker

### docker-compose.yml

| Servicio | Imagen/Build | Puerto | Notas |
|----------|-------------|--------|-------|
| **api** | Build: `./Dockerfile` | 5000 → 8080 | ASP.NET Core 9. Depends: postgres (healthy) |
| **frontend** | Build: `./coach-dashboard/Dockerfile` | 3000 → 3000 | Next.js 16. Depends: api |
| **postgres** | `postgres:16` | 5432 → 5432 | Volume `postgres_data`. Health: `pg_isready` |
| **pgadmin** | `dpage/pgadmin4` | 5050 → 80 | Profile `tools` (opcional) |

### Dockerfiles

**API** (`./Dockerfile`):
- Multi-stage: SDK 9.0 (build) → ASP.NET 9.0 (runtime)
- Non-root user (`appuser:1000`)
- Health check: `/dev/tcp` on 8080

**Frontend** (`./coach-dashboard/Dockerfile`):
- Multi-stage: node:20-alpine (deps → build → run)
- `NEXT_PUBLIC_API_URL` baked at build time
- Non-root user (`nextjs:1001`)
- Standalone output

---

## 🧪 Testing

### Unit Tests (204+ tests)

| Área | Tests | Archivos |
|------|-------|---------|
| Domain Entities | ~40 | ApplicationTests, AthleteTests, ExerciseTests, AthleteWorkoutTrackingTests, AthleteExerciseLogTrackingTests |
| Domain VOs | ~15 | EmailTests, MoneyTests |
| Application Commands | ~120 | 19 archivos de handler tests |
| Application Queries | ~25 | 3 archivos de query tests |

**Framework**: xUnit 2.9 + FluentAssertions 8.8 + Moq 4.20
**Coverage**: EF Core InMemory para mocking de DbContext

### Frontend Tests

**Framework**: Jest 30 + Testing Library
**Ubicación**: `coach-dashboard/src/__tests__/`
Subdirectorios: `components/`, `types/`, `utils/`

---

## 🚀 Deployment

### Ambientes

| Ambiente | Base de Datos | API URL | Frontend URL |
|----------|---------------|---------|-------------|
| Development | PostgreSQL Docker (localhost:5432) | http://localhost:5000 | http://localhost:3000 |
| Docker Compose | PostgreSQL container | http://api:8080 (internal) | http://localhost:3000 |
| Production | Supabase / PostgreSQL managed | TBD | TBD |

### Variables de Entorno

```bash
# Database
ConnectionStrings__DefaultConnection=Host=...;Port=5432;Database=coachplatform;Username=...;Password=...

# JWT
TokenSettings__Secret=<your-256bit-secret>
TokenSettings__Issuer=CoachPlatform
TokenSettings__Audience=CoachPlatformAPI
TokenSettings__AccessTokenExpirationMinutes=15
TokenSettings__RefreshTokenExpirationDays=7

# CORS
Cors__AllowedOrigins__0=http://localhost:3000

# Environment
ASPNETCORE_ENVIRONMENT=Development|Production

# Frontend
NEXT_PUBLIC_API_URL=http://localhost:5000/api
```

### Startup Automático

1. Auto-migración: `db.Database.MigrateAsync()` al iniciar
2. Admin seeding: crea `admin@powercoach.com` / `Admin@123456` si no existe

---

## 📚 Referencias

- [Clean Architecture - Jason Taylor Template](https://github.com/jasontaylordev/CleanArchitecture)
- [MediatR Documentation](https://github.com/jbogard/MediatR)
- [Entity Framework Core Docs](https://docs.microsoft.com/ef/core/)
- [PostgreSQL Documentation](https://www.postgresql.org/docs/)
- [Next.js Documentation](https://nextjs.org/docs)
- [TanStack Query](https://tanstack.com/query)
- [Radix UI](https://www.radix-ui.com/)
