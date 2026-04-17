using CoachPlatform.Application.Shared.Interfaces;
using MediatR;

namespace CoachPlatform.Application.Features.TrainingPrograms.Commands.AddProgramWeek;

/// <summary>
/// Command to add a week to a program template.
/// </summary>
public record AddProgramWeekCommand : IRequest<Guid>, ITenantRequest
{
    public Guid CoachId { get; init; }
    public Guid ProgramTemplateId { get; init; }
    public int WeekNumber { get; init; }
    public string? Name { get; init; }
    public string? Notes { get; init; }
}
