using CoachPlatform.Domain.Enums;

namespace CoachPlatform.Application.Shared.DTOs;

/// <summary>
/// Data Transfer Object for Athlete entity.
/// </summary>
public record AthleteDto
{
    public Guid Id { get; init; }
    public Guid CoachId { get; init; }
    public string FirstName { get; init; } = null!;
    public string LastName { get; init; } = null!;
    public string FullName { get; init; } = null!;
    public string Email { get; init; } = null!;
    public string? Phone { get; init; }
    public string? Goals { get; init; }
    public string? Notes { get; init; }
    public string? Country { get; init; }
    public string? Gender { get; init; }
    public DateTime? DateOfBirth { get; init; }
    public decimal? Height { get; init; }
    public decimal? Weight { get; init; }
    public string? ExperienceLevel { get; init; }
    public DateTime StartDate { get; init; }
    public DateTime? EndDate { get; init; }
    public AthleteStatus Status { get; init; }
    public string? ProfilePictureUrl { get; init; }
    public Guid? ApplicationId { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}

/// <summary>
/// DTO for creating a new Athlete.
/// </summary>
public record CreateAthleteDto
{
    public Guid CoachId { get; init; }
    public string FirstName { get; init; } = null!;
    public string LastName { get; init; } = null!;
    public string Email { get; init; } = null!;
    public string? Phone { get; init; }
    public string? Goals { get; init; }
    public string? Country { get; init; }
    public string? Gender { get; init; }
    public DateTime? DateOfBirth { get; init; }
    public decimal? Height { get; init; }
    public decimal? Weight { get; init; }
    public string? ExperienceLevel { get; init; }
    public Guid? ApplicationId { get; init; }
}

/// <summary>
/// DTO for updating an existing Athlete.
/// </summary>
public record UpdateAthleteDto
{
    public string? Country { get; init; }
    public decimal? Height { get; init; }
    public decimal? Weight { get; init; }
    public string? ExperienceLevel { get; init; }
}

/// <summary>
/// Lightweight DTO for listing athletes.
/// </summary>
public record AthleteListItemDto
{
    public Guid Id { get; init; }
    public string FullName { get; init; } = null!;
    public string Email { get; init; } = null!;
    public string? Country { get; init; }
    public AthleteStatus Status { get; init; }
    public DateTime StartDate { get; init; }
    public string? ProfilePictureUrl { get; init; }
}

/// <summary>
/// Detailed DTO for single athlete view with related data.
/// </summary>
public record AthleteDetailDto : AthleteDto
{
    /// <summary>
    /// Active subscriptions for this athlete.
    /// </summary>
    public IReadOnlyList<SubscriptionSummaryDto> ActiveSubscriptions { get; init; } = [];

    /// <summary>
    /// Recent check-ins (last 5).
    /// </summary>
    public IReadOnlyList<CheckInSummaryDto> RecentCheckIns { get; init; } = [];
}

/// <summary>
/// Summary DTO for subscription in athlete detail view.
/// </summary>
public record SubscriptionSummaryDto
{
    public Guid Id { get; init; }
    public string PlanName { get; init; } = null!;
    public DateTime StartDate { get; init; }
    public DateTime EndDate { get; init; }
    public string Status { get; init; } = null!;
}

/// <summary>
/// Summary DTO for check-in in athlete detail view.
/// </summary>
public record CheckInSummaryDto
{
    public Guid Id { get; init; }
    public DateTime CheckInDate { get; init; }
    public decimal? Weight { get; init; }
    public bool HasCoachFeedback { get; init; }
}
