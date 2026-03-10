using CoachPlatform.Domain.Enums;

namespace CoachPlatform.Application.Shared.DTOs;

/// <summary>
/// Data Transfer Object for Subscription entity.
/// </summary>
public record SubscriptionDto
{
    public Guid Id { get; init; }
    public Guid AthleteId { get; init; }
    public Guid PlanId { get; init; }
    public string PlanName { get; init; } = null!;
    public DateTime StartDate { get; init; }
    public DateTime EndDate { get; init; }
    public SubscriptionStatus Status { get; init; }
    public decimal Price { get; init; }
    public string Currency { get; init; } = null!;
    public bool AutoRenew { get; init; }
    public DateTime? CancelledAt { get; init; }
    public string? CancellationReason { get; init; }
    public string? Notes { get; init; }
    public int RemainingDays { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}

/// <summary>
/// DTO for creating a new Subscription.
/// </summary>
public record CreateSubscriptionDto
{
    public Guid AthleteId { get; init; }
    public Guid PlanId { get; init; }
    public DateTime? StartDate { get; init; }
    public bool AutoRenew { get; init; }
}

/// <summary>
/// DTO for cancelling a Subscription.
/// </summary>
public record CancelSubscriptionDto
{
    public string? Reason { get; init; }
}

/// <summary>
/// Lightweight DTO for listing subscriptions.
/// </summary>
public record SubscriptionListItemDto
{
    public Guid Id { get; init; }
    public string AthleteName { get; init; } = null!;
    public string PlanName { get; init; } = null!;
    public SubscriptionStatus Status { get; init; }
    public DateTime StartDate { get; init; }
    public DateTime EndDate { get; init; }
    public int RemainingDays { get; init; }
}
