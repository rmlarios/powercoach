# CoachPlatform - AI Agent Instructions

## Architecture Overview

This is a **.NET 9 Clean Architecture** backend for coaching management with **multi-tenancy** (data isolated per Coach).

```
API → Application (MediatR) → Domain ← Infrastructure (EF Core + PostgreSQL)
```

**Key principle**: Domain has zero external dependencies. All business logic lives in entity methods.

## Project Structure

| Layer | Project | Contains |
|-------|---------|----------|
| **Domain** | `CoachPlatform.Domain` | Entities, Value Objects, Enums, Interfaces |
| **Application** | `CoachPlatform.Application` | Commands, Queries, Handlers, Validators, DTOs |
| **Infrastructure** | `CoachPlatform.Infrastructure` | EF Core DbContext, Configurations, Services |
| **API** | `CoachPlatform.API` | Controllers, Middleware |

## CQRS Pattern (Critical)

Every feature uses MediatR with this folder structure under `Features/{Entity}/`:
```
Commands/
  CreateAthlete/
    CreateAthleteCommand.cs      # record with ITenantRequest
    CreateAthleteCommandHandler.cs
    CreateAthleteCommandValidator.cs  # FluentValidation
Queries/
  GetAthleteById/
    GetAthleteByIdQuery.cs
    GetAthleteByIdQueryHandler.cs
```

**Command pattern**:
```csharp
public record CreateAthleteCommand : IRequest<Guid>, ITenantRequest
{
    public Guid CoachId { get; init; }  // Required for tenant validation
    // ... other properties
}
```

## Multi-Tenancy (Critical)

All coach-owned resources require tenant validation:

| Interface | Use When | Property Required |
|-----------|----------|-------------------|
| `ITenantRequest` | Command/Query has `CoachId` | `Guid CoachId { get; }` |
| `IAthleteOwnedRequest` | Operating on athlete's sub-resources | `Guid AthleteId { get; }` |

The `TenantValidationBehavior` automatically validates access in the MediatR pipeline.

## Entity Patterns

**Always use static factory methods** - constructors are private:
```csharp
// ✅ Correct
var athlete = Athlete.Create(coachId, firstName, lastName, email);

// ❌ Never use new
var athlete = new Athlete();
```

**Value Objects** (`Email`, `Money`, `PersonName`) are immutable with `Create()` factory:
```csharp
var email = Email.Create("user@example.com");  // Throws if invalid
var money = Money.Create(99.99m, "USD");
```

## Validation

FluentValidation validators run automatically via `ValidationBehavior`. Every Command needs a Validator:
```csharp
public class CreateAthleteCommandValidator : AbstractValidator<CreateAthleteCommand>
{
    public CreateAthleteCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.CoachId).NotEmpty();
    }
}
```

## EF Core Configuration

Value Objects are configured as **Owned Types**:
```csharp
builder.OwnsOne(a => a.Email, emailBuilder =>
{
    emailBuilder.Property(e => e.Value)
        .HasColumnName("Email")
        .HasMaxLength(256);
});
```

## Testing Patterns

Unit tests use **FluentAssertions** and **Moq**:
```csharp
[Fact]
public void Create_WithValidData_ShouldCreateEntity()
{
    var entity = Entity.Create(validParams);
    
    entity.Should().NotBeNull();
    entity.Property.Should().Be(expectedValue);
}

[Fact]  
public void Create_WithInvalidData_ShouldThrow()
{
    var act = () => Entity.Create(invalidParams);
    
    act.Should().Throw<ArgumentException>()
        .WithMessage("*expected message*");
}
```

## Common Commands

```bash
# Build
dotnet build

# Run tests
dotnet test

# Run API
dotnet run --project src/CoachPlatform.API

# Docker (API + PostgreSQL)
docker-compose up -d
```

## Naming Conventions

### DTOs (in `Application/Shared/DTOs/`)

| Suffix | Purpose | Example |
|--------|---------|---------|
| `*Dto` | Full entity representation | `AthleteDto` |
| `*ListItemDto` | Lightweight for lists/grids | `AthleteListItemDto` |
| `*DetailDto` | Extended with related data | `AthleteDetailDto` |
| `*SummaryDto` | Minimal nested reference | `SubscriptionSummaryDto` |
| `Create*Dto` | Input for creation | `CreateAthleteDto` |
| `Update*Dto` | Input for updates (partial) | `UpdateAthleteDto` |

### Commands & Queries

| Pattern | Example |
|---------|---------|
| `Create{Entity}Command` | `CreateAthleteCommand` |
| `Update{Entity}Command` | `UpdateAthleteCommand` |
| `Delete{Entity}Command` | `DeleteAthleteCommand` |
| `{Action}{Entity}Command` | `ApproveApplicationCommand`, `CancelSubscriptionCommand` |
| `Get{Entity}ByIdQuery` | `GetAthleteByIdQuery` |
| `Get{Entity}sBy{Filter}Query` | `GetAthletesByCoachQuery` |

## Error Handling

Custom exceptions in `Application/Shared/Exceptions/` map to HTTP status codes:

| Exception | HTTP Status | When to Use |
|-----------|-------------|-------------|
| `NotFoundException` | 404 | Entity not found by ID |
| `ConflictException` | 409 | Duplicate/constraint violation |
| `ForbiddenAccessException` | 403 | Tenant/permission violation |
| `ValidationException` | 400 | FluentValidation failures |

**Usage in handlers**:
```csharp
// 404 - Entity not found
if (athlete == null)
    throw new NotFoundException(nameof(Athlete), request.Id);

// 409 - Duplicate constraint
if (emailExists)
    throw new ConflictException(nameof(Athlete), "Email", request.Email);

// 403 - Access denied (usually via TenantValidationBehavior)
throw new ForbiddenAccessException("You do not have access to this resource.");
```

The `ExceptionHandlingMiddleware` converts these to RFC 7807 JSON responses automatically.

## EF Core Migrations

```bash
# Create migration (from solution root)
dotnet ef migrations add MigrationName -p src/CoachPlatform.Infrastructure -s src/CoachPlatform.API

# Apply migrations
dotnet ef database update -p src/CoachPlatform.Infrastructure -s src/CoachPlatform.API

# Remove last migration (if not applied)
dotnet ef migrations remove -p src/CoachPlatform.Infrastructure -s src/CoachPlatform.API

# Generate SQL script
dotnet ef migrations script -p src/CoachPlatform.Infrastructure -s src/CoachPlatform.API
```

**Migration naming**: `Add{Feature}`, `Update{Entity}`, `Add{Column}To{Table}`
- Examples: `AddExerciseEntity`, `AddTenantIndexes`, `UpdateAthleteAddProfilePicture`

## Key Files to Reference

- Entity example: `src/CoachPlatform.Domain/Entities/Exercise.cs`
- CQRS example: `src/CoachPlatform.Application/Features/Athletes/Commands/CreateAthlete/`
- Value Object: `src/CoachPlatform.Domain/ValueObjects/Email.cs`
- EF Config: `src/CoachPlatform.Infrastructure/Persistence/Configurations/AthleteConfiguration.cs`
- Test example: `tests/CoachPlatform.UnitTests/Domain/Entities/ExerciseTests.cs`
- DTOs example: `src/CoachPlatform.Application/Shared/DTOs/AthleteDtos.cs`
- Exception handling: `src/CoachPlatform.API/Middleware/ExceptionHandlingMiddleware.cs`
