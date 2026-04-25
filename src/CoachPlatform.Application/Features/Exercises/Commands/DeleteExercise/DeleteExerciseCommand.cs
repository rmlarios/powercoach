using MediatR;

namespace CoachPlatform.Application.Features.Exercises.Commands.DeleteExercise;

/// <summary>
/// Command to soft-delete (deactivate) an exercise.
/// </summary>
public record DeleteExerciseCommand : IRequest
{
    public Guid ExerciseId { get; init; }
}
