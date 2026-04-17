using CoachPlatform.Application.Shared.Interfaces;
using MediatR;

namespace CoachPlatform.Application.Features.TrainingPrograms.Commands.DeleteProgramWeek;

/// <summary>
/// Command to delete a week from a program template.
/// </summary>
public record DeleteProgramWeekCommand : IRequest<Unit>, ITenantRequest
{
    public Guid CoachId { get; init; }
    public Guid ProgramTemplateId { get; init; }
    public Guid WeekId { get; init; }
}
