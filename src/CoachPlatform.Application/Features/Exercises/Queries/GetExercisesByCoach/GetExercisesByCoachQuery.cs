using CoachPlatform.Application.Features.Exercises.Shared;
using CoachPlatform.Application.Shared.Interfaces;
using CoachPlatform.Domain.Enums;
using MediatR;

namespace CoachPlatform.Application.Features.Exercises.Queries.GetExercisesByCoach;

/// <summary>
/// Query to get all exercises for a specific coach.
/// </summary>
public record GetExercisesByCoachQuery : IRequest<IEnumerable<ExerciseListItemDto>>, ITenantRequest
{
    public Guid CoachId { get; init; }
    public ExerciseCategory? Category { get; init; }
    public MuscleGroup? MuscleGroup { get; init; }
    public bool? IsActive { get; init; }
}
