namespace CoachPlatform.Application.Shared.DTOs;

/// <summary>
/// Data Transfer Object for TrainingCycle entity.
/// </summary>
public record TrainingCycleDto
{
    public Guid Id { get; init; }
    public Guid AthleteId { get; init; }
    public string AthleteName { get; init; } = null!;
    public string Name { get; init; } = null!;
    public int DurationWeeks { get; init; }
    public DateTime StartDate { get; init; }
    public DateTime EndDate { get; init; }
    public bool IsActive { get; init; }
    public int CurrentWeek { get; init; }
    public DateTime CreatedAt { get; init; }
}

/// <summary>
/// Lightweight DTO for listing training cycles.
/// </summary>
public record TrainingCycleListItemDto
{
    public Guid Id { get; init; }
    public Guid AthleteId { get; init; }
    public string Name { get; init; } = null!;
    public int DurationWeeks { get; init; }
    public DateTime StartDate { get; init; }
    public DateTime EndDate { get; init; }
    public bool IsActive { get; init; }
}
