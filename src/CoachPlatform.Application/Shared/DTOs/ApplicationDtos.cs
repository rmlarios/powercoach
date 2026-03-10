using CoachPlatform.Domain.Enums;

namespace CoachPlatform.Application.Shared.DTOs;

/// <summary>
/// Data Transfer Object for Application entity (postulaciones).
/// </summary>
public record ApplicationDto
{
    public Guid Id { get; init; }
    public Guid CoachId { get; init; }
    public string FirstName { get; init; } = null!;
    public string LastName { get; init; } = null!;
    public string FullName { get; init; } = null!;
    public string Email { get; init; } = null!;
    public string? Phone { get; init; }
    public int? Age { get; init; }
    public string? Gender { get; init; }
    public string? Country { get; init; }
    public string? TrainingExperience { get; init; }
    public decimal? CurrentSquat { get; init; }
    public decimal? CurrentBench { get; init; }
    public decimal? CurrentDeadlift { get; init; }
    public decimal? TotalLifts => (CurrentSquat ?? 0) + (CurrentBench ?? 0) + (CurrentDeadlift ?? 0);
    public string? Motivation { get; init; }
    public string? Goals { get; init; }
    public string? Message { get; init; }
    public string? ReferralSource { get; init; }
    public ApplicationStatus Status { get; init; }
    public string? CoachNotes { get; init; }
    public DateTime? ReviewedAt { get; init; }
    public string? RejectionReason { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}

/// <summary>
/// DTO for creating a new Application (public form submission).
/// </summary>
public record CreateApplicationDto
{
    public Guid CoachId { get; init; }
    public string FirstName { get; init; } = null!;
    public string LastName { get; init; } = null!;
    public string Email { get; init; } = null!;
    public string? Phone { get; init; }
    public int? Age { get; init; }
    public string? Gender { get; init; }
    public string? Country { get; init; }
    public string? TrainingExperience { get; init; }
    public decimal? CurrentSquat { get; init; }
    public decimal? CurrentBench { get; init; }
    public decimal? CurrentDeadlift { get; init; }
    public string? Motivation { get; init; }
    public string? Goals { get; init; }
    public string? Message { get; init; }
    public string? ReferralSource { get; init; }
}

/// <summary>
/// DTO for approving/accepting an application.
/// </summary>
public record ApproveApplicationDto
{
    public string? Notes { get; init; }
}

/// <summary>
/// DTO for rejecting an application.
/// </summary>
public record RejectApplicationDto
{
    public string? Reason { get; init; }
    public string? Notes { get; init; }
}

/// <summary>
/// Lightweight DTO for listing applications.
/// </summary>
public record ApplicationListItemDto
{
    public Guid Id { get; init; }
    public string FullName { get; init; } = null!;
    public string Email { get; init; } = null!;
    public string? Country { get; init; }
    public decimal? TotalLifts { get; init; }
    public ApplicationStatus Status { get; init; }
    public DateTime CreatedAt { get; init; }
}
