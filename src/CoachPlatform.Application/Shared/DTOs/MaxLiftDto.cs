namespace CoachPlatform.Application.Shared.DTOs;

/// <summary>
/// DTO for athlete's max lift (1RM) record.
/// </summary>
public record MaxLiftDto
{
    public Guid Id { get; init; }
    public Guid ExerciseId { get; init; }
    public string ExerciseName { get; init; } = string.Empty;
    public decimal Weight { get; init; }
    public bool IsTested { get; init; }
    public DateTime RecordedAt { get; init; }
    public string? Notes { get; init; }
    public string? EstimationDetails { get; init; }
}

/// <summary>
/// Response containing all max lifts for an athlete.
/// </summary>
public record AthleteMaxLiftsDto
{
    public Guid AthleteId { get; init; }
    public string AthleteName { get; init; } = string.Empty;
    public IReadOnlyList<MaxLiftDto> MaxLifts { get; init; } = [];
}
