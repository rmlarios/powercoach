using CoachPlatform.Application.Features.Exercises.Shared;
using MediatR;

namespace CoachPlatform.Application.Features.Exercises.Queries.GetExerciseById;

/// <summary>
/// Query to get an exercise by ID.
/// </summary>
public record GetExerciseByIdQuery : IRequest<ExerciseDto?>
{
    public Guid ExerciseId { get; init; }
}
