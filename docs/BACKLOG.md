# CoachPlatform - Product Backlog

> Sistema backend para gestión de atletas, pagos y postulaciones de coach de powerlifting/fitness.
> Arquitectura: Clean Architecture + DDD | Stack: .NET 9, PostgreSQL, Docker

---

## 📊 Resumen del Proyecto

| Métrica | Valor |
|---------|-------|
| **Features Totales** | 9 |
| **Tasks Totales** | 78 |
| **✅ Completadas** | 78 |
| **🟡 En Progreso** | 0 |
| **⬜ Pendientes** | 0 |

**Última actualización**: 2026-03-10

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

## 📝 Changelog

| Fecha | Cambios |
|-------|---------|| 2026-03-10 | ✅ F-009 completada - Multi-tenancy: ITenantService, TenantValidationBehavior, TenantMiddleware |
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
- [ ] **Dashboard del Coach** - Métricas, reportes visuales
- [ ] **Integración con Pasarelas de Pago** - Stripe, PayPal, MercadoPago
- [ ] **API de Reportes y Analytics** - Estadísticas de atletas, ingresos
- [ ] **App Mobile** - React Native o Flutter
- [ ] **Calendario y Scheduling** - Sesiones, recordatorios
- [ ] **Integración con Wearables** - Garmin, Apple Watch, Fitbit

---

## 📌 Notas de Sesión

> Espacio para notas temporales durante el desarrollo

_Sin notas actuales_
