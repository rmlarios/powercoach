namespace CoachPlatform.Application.Shared.DTOs;

/// <summary>
/// Data Transfer Object for WorkoutLog entity.
/// </summary>
public record WorkoutLogDto
{
    public Guid Id { get; init; }
    public Guid AthleteId { get; init; }
    public string AthleteName { get; init; } = null!;
    public string ExerciseName { get; init; } = null!;
    public int Sets { get; init; }
    public int Reps { get; init; }
    public decimal? Weight { get; init; }
    public int? RPE { get; init; }
    public string? Notes { get; init; }
    public DateTime WorkoutDate { get; init; }
    public decimal? TotalVolume { get; init; }
    public DateTime CreatedAt { get; init; }
}

/// <summary>
/// Lightweight DTO for listing workout logs.
/// </summary>
public record WorkoutLogListItemDto
{
    public Guid Id { get; init; }
    public Guid AthleteId { get; init; }
    public string ExerciseName { get; init; } = null!;
    public int Sets { get; init; }
    public int Reps { get; init; }
    public decimal? Weight { get; init; }
    public int? RPE { get; init; }
    public DateTime WorkoutDate { get; init; }
}
