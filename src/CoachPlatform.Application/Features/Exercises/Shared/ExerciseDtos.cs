using CoachPlatform.Domain.Enums;

namespace CoachPlatform.Application.Features.Exercises.Shared;

/// <summary>
/// DTO for exercise details.
/// </summary>
public record ExerciseDto
{
    public Guid Id { get; init; }
    public Guid CoachId { get; init; }
    public string Name { get; init; } = null!;
    public string? Description { get; init; }
    public ExerciseCategory Category { get; init; }
    public MuscleGroup PrimaryMuscleGroup { get; init; }
    public List<MuscleGroup> SecondaryMuscleGroups { get; init; } = [];
    public string? VideoUrl { get; init; }
    public string? ImageUrl { get; init; }
    public string? Equipment { get; init; }
    public bool IsCompound { get; init; }
    public bool IsActive { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
}

/// <summary>
/// Lightweight DTO for listing exercises.
/// </summary>
public record ExerciseListItemDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = null!;
    public ExerciseCategory Category { get; init; }
    public MuscleGroup PrimaryMuscleGroup { get; init; }
    public string? Equipment { get; init; }
    public bool IsCompound { get; init; }
    public bool IsActive { get; init; }
}
