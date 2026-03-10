using CoachPlatform.Application.Shared.Interfaces;
using CoachPlatform.Domain.Enums;
using MediatR;

namespace CoachPlatform.Application.Features.Exercises.Commands.CreateExercise;

/// <summary>
/// Command to create a new exercise in the coach's library.
/// </summary>
public record CreateExerciseCommand : IRequest<Guid>, ITenantRequest
{
    public Guid CoachId { get; init; }
    public string Name { get; init; } = null!;
    public string? Description { get; init; }
    public ExerciseCategory Category { get; init; }
    public MuscleGroup PrimaryMuscleGroup { get; init; }
    public List<MuscleGroup>? SecondaryMuscleGroups { get; init; }
    public string? VideoUrl { get; init; }
    public string? Equipment { get; init; }
    public bool IsCompound { get; init; } = true;
}
