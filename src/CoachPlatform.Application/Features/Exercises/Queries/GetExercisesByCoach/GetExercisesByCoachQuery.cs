using CoachPlatform.Application.Features.Exercises.Shared;
using CoachPlatform.Domain.Enums;
using MediatR;

namespace CoachPlatform.Application.Features.Exercises.Queries.GetExercisesByCoach;

/// <summary>
/// Query to get all exercises from the global catalog.
/// </summary>
public record GetExercisesByCoachQuery : IRequest<IEnumerable<ExerciseListItemDto>>
{
    public ExerciseCategory? Category { get; init; }
    public MuscleGroup? MuscleGroup { get; init; }
    public bool? IsActive { get; init; }
}
