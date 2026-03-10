# CoachPlatform

Sistema backend para gestión de atletas, pagos y postulaciones de coach de powerlifting/fitness.

## ✅ Estado del Proyecto

| Feature | Estado |
|---------|--------|
| Domain Layer | ✅ Completado |
| Application Layer | ✅ Completado |
| Infrastructure Layer | ✅ Completado |
| API Layer | ✅ Completado |
| Multi-tenancy | ✅ Completado |
| Exercise Library | ✅ Completado |
| Unit Tests | ✅ 147 tests pasando |
| Docker Support | ✅ Completado |

## 🏗️ Arquitectura

- **Clean Architecture** con 4 capas (Domain, Application, Infrastructure, API)
- **Domain Driven Design (DDD)** con entidades, value objects y aggregates
- **CQRS ligero** con MediatR para separación de commands/queries
- **Repository Pattern** para abstracción de persistencia

## 🛠️ Stack Tecnológico

| Tecnología | Versión |
|------------|---------|
| .NET | 9.0 |
| ASP.NET Core | 9.0 |
| Entity Framework Core | 9.x |
| PostgreSQL | 16+ |
| MediatR | 14.x |
| FluentValidation | 12.x |
| Docker | Latest |

## 📁 Estructura del Proyecto

```
CoachPlatform/
├── src/
│   ├── CoachPlatform.Domain         # Entidades, Value Objects, Interfaces
│   ├── CoachPlatform.Application    # Commands, Queries, DTOs, Validators
│   ├── CoachPlatform.Infrastructure # EF Core, Repositories, Configs
│   └── CoachPlatform.API            # Controllers, Middleware, Swagger
├── tests/
│   ├── CoachPlatform.UnitTests
│   └── CoachPlatform.IntegrationTests
└── docs/
    ├── BACKLOG.md                   # Product backlog y tracking
    └── ARCHITECTURE.md              # Decisiones de arquitectura
```

## 🚀 Quick Start

### Prerrequisitos

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [Docker](https://www.docker.com/products/docker-desktop) (opcional, para PostgreSQL local)
- [PostgreSQL](https://www.postgresql.org/) o cuenta en [Supabase](https://supabase.com/)

### Instalación

```bash
# Clonar repositorio
git clone <repository-url>
cd CoachPlatform

# Restaurar dependencias
dotnet restore

# Configurar connection string (ver appsettings.Development.json)

# Aplicar migraciones
dotnet ef database update -p src/CoachPlatform.Infrastructure -s src/CoachPlatform.API

# Ejecutar
dotnet run --project src/CoachPlatform.API
```

### Con Docker

```bash
# Levantar API + PostgreSQL
docker-compose up -d

# Ver logs
docker-compose logs -f api

# Para usar pgAdmin también:
docker-compose --profile tools up -d
```

## 🧪 Testing

```bash
# Ejecutar todos los tests
dotnet test

# Solo unit tests
dotnet test tests/CoachPlatform.UnitTests

# Solo integration tests
dotnet test tests/CoachPlatform.IntegrationTests

# Con cobertura
dotnet test --collect:"XPlat Code Coverage"
```

## 📖 API Documentation

Una vez ejecutando, acceder a Swagger UI:

- **Development**: http://localhost:5000 (con Docker)
- **Local**: https://localhost:7xxx/swagger

### Endpoints disponibles:

| Recurso | Endpoints |
|---------|-----------|
| Athletes | GET, POST, GET/{id} |
| Applications | GET, POST, POST/{id}/approve, POST/{id}/reject |
| Plans | GET, POST, PUT/{id}, DELETE/{id} |
| Subscriptions | GET, POST, POST/{id}/cancel, POST/{id}/renew |
| Payments | GET, POST, POST/{id}/complete, POST/{id}/fail |
| CheckIns | GET, POST, POST/{id}/feedback |
| Health | GET /health |

## 🗄️ Módulos del Dominio

| Módulo | Descripción |
|--------|-------------|
| **Athletes** | Gestión de atletas activos del coach |
| **Applications** | Postulaciones de personas al programa |
| **Plans** | Tipos de membresía (mensual, trimestral, anual) |
| **Subscriptions** | Relación entre atleta y plan |
| **Payments** | Registro de pagos |
| **CheckIns** | Formularios de seguimiento periódico |

## 📊 Estado del Proyecto

Ver [BACKLOG.md](docs/BACKLOG.md) para el estado actual del desarrollo.

| Métrica | Valor |
|---------|-------|
| Features | 7 |
| Tasks | 68 |
| Sprint Actual | 1 |

## 📝 Licencia

Privado - Todos los derechos reservados.

---

> Desarrollado siguiendo principios SOLID y buenas prácticas de Clean Architecture.
