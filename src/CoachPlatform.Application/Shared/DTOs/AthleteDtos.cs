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

/// <summary>
/// DTO for exercise history of an athlete.
/// Contains historical performance data for a specific exercise.
/// </summary>
public record ExerciseHistoryDto
{
    public Guid AthleteId { get; init; }
    public string AthleteName { get; init; } = null!;
    public Guid ExerciseId { get; init; }
    public string ExerciseName { get; init; } = null!;
    
    /// <summary>
    /// Current 1RM for this exercise (most recent).
    /// </summary>
    public MaxLiftSummaryDto? Current1RM { get; init; }
    
    /// <summary>
    /// Personal record (highest 1RM ever recorded).
    /// </summary>
    public MaxLiftSummaryDto? PersonalRecord { get; init; }
    
    /// <summary>
    /// Last programmed prescription (from most recent program).
    /// </summary>
    public LastProgrammedDto? LastProgrammed { get; init; }
    
    /// <summary>
    /// Weight trend over the last 3 months (positive = improvement).
    /// </summary>
    public decimal? TrendKg { get; init; }
    
    /// <summary>
    /// Suggested starting percentage based on history.
    /// </summary>
    public int? SuggestedStartPercentage { get; init; }
    
    /// <summary>
    /// Suggested starting weight based on 1RM.
    /// </summary>
    public decimal? SuggestedStartWeight { get; init; }
    
    /// <summary>
    /// Recent performance logs (last 10).
    /// </summary>
    public IReadOnlyList<ExerciseLogSummaryDto> RecentLogs { get; init; } = [];
    
    /// <summary>
    /// Whether the athlete has any history for this exercise.
    /// </summary>
    public bool HasHistory { get; init; }
}

/// <summary>
/// Summary of a max lift record.
/// </summary>
public record MaxLiftSummaryDto
{
    public decimal Weight { get; init; }
    public DateTime RecordedAt { get; init; }
    public bool IsTested { get; init; }
    public string? Source { get; init; }
}

/// <summary>
/// Summary of last programmed exercise.
/// </summary>
public record LastProgrammedDto
{
    public string ProgramName { get; init; } = null!;
    public int WeekNumber { get; init; }
    public int DayNumber { get; init; }
    public string Prescription { get; init; } = null!; // e.g., "3x5 @275kg"
    public DateTime ProgramDate { get; init; }
}

/// <summary>
/// Summary of an exercise performance log.
/// </summary>
public record ExerciseLogSummaryDto
{
    public DateTime PerformedAt { get; init; }
    public int Sets { get; init; }
    public int Reps { get; init; }
    public decimal Weight { get; init; }
    public decimal? Rpe { get; init; }
    public string? Notes { get; init; }
}
