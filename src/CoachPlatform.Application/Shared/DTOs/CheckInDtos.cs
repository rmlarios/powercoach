namespace CoachPlatform.Application.Shared.DTOs;

/// <summary>
/// Data Transfer Object for CheckIn entity.
/// </summary>
public record CheckInDto
{
    public Guid Id { get; init; }
    public Guid AthleteId { get; init; }
    public string AthleteName { get; init; } = null!;
    public DateTime CheckInDate { get; init; }
    public decimal? Weight { get; init; }
    public string? WeightUnit { get; init; }
    public string? Notes { get; init; }
    public List<string> PhotoUrls { get; init; } = [];
    public string? CoachFeedback { get; init; }
    public DateTime? ReviewedAt { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
    public int? EnergyLevel { get; init; }
    public int? SleepQuality { get; init; }
    public decimal? SleepHours { get; init; }
    public int? StressLevel { get; init; }
    public int? NutritionAdherence { get; init; }
    public int? TrainingAdherence { get; init; }
}

/// <summary>
/// DTO for creating a new check-in (athlete submission).
/// </summary>
public record CreateCheckInDto
{
    public Guid AthleteId { get; init; }
    public DateTime? CheckInDate { get; init; }
    public decimal? Weight { get; init; }
    public string? WeightUnit { get; init; }
    public string? Notes { get; init; }
    public string? PhotoUrl { get; init; }
}

/// <summary>
/// DTO for coach to add feedback to a check-in.
/// </summary>
public record AddCheckInFeedbackDto
{
    public string Feedback { get; init; } = null!;
}

/// <summary>
/// Lightweight DTO for listing check-ins.
/// </summary>
public record CheckInListItemDto
{
    public Guid Id { get; init; }
    public Guid AthleteId { get; init; }
    public string? AthleteName { get; init; }
    public DateTime CheckInDate { get; init; }
    public decimal? Weight { get; init; }
    public bool HasPhoto { get; init; }
    public bool HasFeedback { get; init; }
    public bool IsReviewed { get; init; }
}

/// <summary>
/// DTO for check-in statistics/progress tracking.
/// </summary>
public record CheckInProgressDto
{
    public Guid AthleteId { get; init; }
    public int TotalCheckIns { get; init; }
    public int CheckInsThisMonth { get; init; }
    public decimal? StartingWeight { get; init; }
    public decimal? CurrentWeight { get; init; }
    public decimal? WeightChange { get; init; }
    public DateTime? LastCheckInDate { get; init; }
}
