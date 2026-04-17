using CoachPlatform.Application.Shared.Interfaces;
using MediatR;

namespace CoachPlatform.Application.Features.Exercises.Commands.DeleteExercise;

/// <summary>
/// Command to soft-delete (deactivate) an exercise.
/// </summary>
public record DeleteExerciseCommand : IRequest, ITenantRequest
{
    public Guid CoachId { get; init; }
    public Guid ExerciseId { get; init; }
}
