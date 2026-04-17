using CoachPlatform.Application.Shared.Interfaces;
using MediatR;

namespace CoachPlatform.Application.Features.TrainingPrograms.Commands.DeleteProgramExercise;

/// <summary>
/// Command to delete an exercise from a program day.
/// </summary>
public record DeleteProgramExerciseCommand : IRequest<Unit>, ITenantRequest
{
    public Guid CoachId { get; init; }
    public Guid ExerciseTemplateId { get; init; }
}
