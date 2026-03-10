using CoachPlatform.Domain.Enums;

namespace CoachPlatform.Application.Shared.DTOs;

/// <summary>
/// Data Transfer Object for Plan entity.
/// </summary>
public record PlanDto
{
    public Guid Id { get; init; }
    public Guid CoachId { get; init; }
    public string Name { get; init; } = null!;
    public string? Description { get; init; }
    public decimal Price { get; init; }
    public string Currency { get; init; } = null!;
    public int DurationDays { get; init; }
    public PlanType PlanType { get; init; }
    public List<string> Features { get; init; } = [];
    public int? MaxAthletes { get; init; }
    public bool IsActive { get; init; }
    public int DisplayOrder { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}

/// <summary>
/// DTO for creating a new Plan.
/// </summary>
public record CreatePlanDto
{
    public Guid CoachId { get; init; }
    public string Name { get; init; } = null!;
    public string? Description { get; init; }
    public decimal Price { get; init; }
    public string Currency { get; init; } = "USD";
    public int DurationDays { get; init; }
    public PlanType PlanType { get; init; }
    public List<string>? Features { get; init; }
    public int? MaxAthletes { get; init; }
}

/// <summary>
/// DTO for updating a Plan.
/// </summary>
public record UpdatePlanDto
{
    public string Name { get; init; } = null!;
    public string? Description { get; init; }
    public decimal Price { get; init; }
    public string Currency { get; init; } = null!;
    public int DurationDays { get; init; }
    public List<string>? Features { get; init; }
    public int? MaxAthletes { get; init; }
}

/// <summary>
/// Lightweight DTO for listing plans.
/// </summary>
public record PlanListItemDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = null!;
    public decimal Price { get; init; }
    public string Currency { get; init; } = null!;
    public PlanType PlanType { get; init; }
    public bool IsActive { get; init; }
    public int DisplayOrder { get; init; }
}
