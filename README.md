# CoachPlatform

Plataforma web de gestión para coaches de powerlifting/fitness. Permite administrar atletas, programas de entrenamiento, suscripciones, pagos y seguimiento de entrenamientos en tiempo real.

## ✅ Estado del Proyecto

| Feature | Estado |
|---------|--------|
| Domain Layer (19 entidades, 3 VOs, 11 enums) | ✅ |
| Application Layer (12 módulos CQRS) | ✅ |
| Infrastructure Layer (EF Core + PostgreSQL) | ✅ |
| API Layer (70+ endpoints REST) | ✅ |
| JWT Authentication & Authorization | ✅ |
| Multi-tenancy (CoachId isolation) | ✅ |
| Exercise Library (catálogo global) | ✅ |
| Training Programs + Builder | ✅ |
| Workout Tracking (real-time) | ✅ |
| Frontend Dashboard (21 páginas) | ✅ |
| Program Builder (editor tipo Notion) | ✅ |
| Unit Tests (204+ tests) | ✅ |
| Docker (API + Frontend + PostgreSQL) | ✅ |

## 🏗️ Arquitectura

- **Clean Architecture** con 4 capas: Domain → Application → Infrastructure → API
- **Domain Driven Design (DDD)**: entidades, value objects, aggregates
- **CQRS ligero** con MediatR para separación commands/queries
- **Repository Pattern** + Unit of Work
- **Multi-tenant**: aislamiento de datos por coach (excepto ejercicios: catálogo global)

### Backend (.NET 9)

| Tecnología | Versión | Uso |
|------------|---------|-----|
| ASP.NET Core Web API | 9.0 | REST API |
| Entity Framework Core | 9.x | ORM + Migrations |
| MediatR | 14.1 | CQRS pipeline |
| FluentValidation | 12.1 | Validación de requests |
| PostgreSQL | 16+ | Base de datos |
| JWT Bearer | 8.x | Autenticación |
| BCrypt.Net | 1.6 | Hashing de passwords |
| Swashbuckle | 7.2 | Swagger/OpenAPI |

### Frontend (Next.js 16)

| Tecnología | Versión | Uso |
|------------|---------|-----|
| Next.js (App Router) | 16.2 | Framework |
| React | 19.2 | UI Library |
| TypeScript | 5.x | Tipado |
| TailwindCSS | 3.4 | Estilos |
| Radix UI + CVA | - | Componentes (ShadCN pattern) |
| TanStack Query | 5.x | Server state management |
| Axios | 1.13 | HTTP client |
| Recharts | 3.8 | Gráficos |
| @dnd-kit | - | Drag and Drop |
| Sonner | 2.0 | Toast notifications |

## 📁 Estructura del Proyecto

```
CoachPlatform/
├── src/
│   ├── CoachPlatform.Domain/         # Entidades, Value Objects, Enums, Interfaces
│   ├── CoachPlatform.Application/    # Commands, Queries, DTOs, Validators, Behaviors
│   ├── CoachPlatform.Infrastructure/ # EF Core, Repositories, JWT Service, Migrations
│   └── CoachPlatform.API/            # Controllers, Middleware, Swagger, Auth
├── coach-dashboard/                   # Frontend Next.js 16
│   ├── src/app/(dashboard)/          # 21 páginas (dashboard, athletes, programs, etc.)
│   ├── src/components/               # builder/, common/, layout/, ui/, charts/
│   ├── src/hooks/                    # TanStack Query hooks por módulo
│   ├── src/lib/api/                  # Axios client + API services
│   ├── src/providers/                # Auth, Coach, Builder, Query providers
│   └── src/types/                    # TypeScript interfaces
├── tests/
│   ├── CoachPlatform.UnitTests/      # 204+ tests (xUnit + Moq + FluentAssertions)
│   └── CoachPlatform.IntegrationTests/
├── docs/
│   ├── ARCHITECTURE.md               # Arquitectura detallada, ADRs, modelo de datos
│   ├── BACKLOG.md                    # Product backlog y tracking
│   └── F-015_WORKOUT_TRACKING_PLAN.md
├── docker-compose.yml                # API + Frontend + PostgreSQL + pgAdmin
├── Dockerfile                        # API multi-stage build
└── CoachPlatform.sln
```

## 🚀 Quick Start

### Prerrequisitos

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [Node.js 20+](https://nodejs.org/)
- [Docker](https://www.docker.com/products/docker-desktop)
- [PostgreSQL 16](https://www.postgresql.org/) (o usar Docker)

### Opción 1: Docker Compose (recomendado)

```bash
# Levantar todo (API + Frontend + PostgreSQL)
docker-compose up -d

# Ver logs
docker-compose logs -f api

# Con pgAdmin (opcional)
docker-compose --profile tools up -d
```

Acceder a:
- **Frontend**: http://localhost:3000
- **API / Swagger**: http://localhost:5000/swagger
- **pgAdmin**: http://localhost:5050 (admin@coach.local / admin)

### Opción 2: Desarrollo local

```bash
# 1. Base de datos (Docker)
docker-compose up -d postgres

# 2. Backend
cd powercoach
dotnet restore
dotnet ef database update -p src/CoachPlatform.Infrastructure -s src/CoachPlatform.API
dotnet run --project src/CoachPlatform.API --urls "http://localhost:5000"

# 3. Frontend (otra terminal)
cd coach-dashboard
npm install
npm run dev
```

### Usuarios por defecto

| Email | Password | Rol |
|-------|----------|-----|
| admin@powercoach.com | Admin@123456 | Admin |
| coach@powercoach.com | Coach@123456 | Coach |

> El usuario admin se crea automáticamente al iniciar la API.

## 🧪 Testing

```bash
# Todos los tests
dotnet test

# Solo unit tests
dotnet test tests/CoachPlatform.UnitTests

# Con cobertura
dotnet test --collect:"XPlat Code Coverage"

# Frontend tests
cd coach-dashboard && npm test
```

## 📖 API Documentation

Swagger UI disponible en: http://localhost:5000/swagger

### Endpoints principales

| Módulo | Ruta base | Endpoints | Auth |
|--------|-----------|-----------|------|
| **Auth** | `/api/auth` | Login, Refresh, Logout, Me, Users CRUD | Mixto |
| **Dashboard** | `/api/dashboard` | Coach metrics | Coach |
| **Applications** | `/api/applications` | CRUD + Approve/Reject | Público (POST) / Coach |
| **Athletes** | `/api/athletes` | CRUD + MaxLifts + History | Coach |
| **Exercises** | `/api/exercises` | CRUD (catálogo global) | ✅ |
| **Programs** | `/api/programs` | CRUD + Bulk Save + Assign | Coach |
| **Workouts** | `/api/athletes/{id}/workouts` | Start, Track, Complete, Skip | ✅ |
| **Check-ins** | `/api/check-ins` | Create + Review | ✅ |
| **Plans** | `/api/plans` | CRUD + Deactivate | Coach |
| **Subscriptions** | `/api/subscriptions` | Create + Cancel | Coach |
| **Payments** | `/api/payments` | Register + List | Coach |

## 🗃️ Módulos del Dominio

| Módulo | Entidades | Descripción |
|--------|-----------|-------------|
| **Coaches** | Coach | Perfil del coach |
| **Athletes** | Athlete, AthleteMaxLift | Gestión de atletas y records de 1RM |
| **Applications** | Application | Postulaciones públicas |
| **Exercises** | Exercise | Catálogo global de ejercicios (compartido) |
| **Programs** | ProgramTemplate, Week, Day, Exercise | Plantillas de entrenamiento |
| **Tracking** | AthleteProgram, AthleteWorkout, AthleteExerciseLog | Ejecución y seguimiento |
| **Plans** | Plan | Tipos de membresía |
| **Subscriptions** | Subscription | Relación atleta-plan |
| **Payments** | Payment | Registro de pagos |
| **CheckIns** | CheckIn | Seguimiento periódico |
| **Auth** | User | Autenticación JWT |

## 📊 Estado detallado

Ver [docs/BACKLOG.md](docs/BACKLOG.md) para el backlog completo y tracking de tasks.
Ver [docs/ARCHITECTURE.md](docs/ARCHITECTURE.md) para arquitectura, modelo de datos y ADRs.

| Métrica | Valor |
|---------|-------|
| Features completadas | 16 |
| Tasks completadas | 149 / 172 |
| Endpoints REST | 70+ |
| Entidades de dominio | 19 |
| Unit Tests | 204+ |
| Páginas frontend | 21 |

## 📝 Licencia

Privado — Todos los derechos reservados.

---

> Desarrollado con Clean Architecture, DDD y principios SOLID.
