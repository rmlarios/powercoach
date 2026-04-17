using CoachPlatform.Application.Shared.Interfaces;
using MediatR;

namespace CoachPlatform.Application.Features.TrainingPrograms.Commands.AssignProgramToAthlete;

/// <summary>
/// Command to assign a program template to an athlete.
/// </summary>
public record AssignProgramToAthleteCommand : IRequest<Guid>, ITenantRequest
{
    public Guid CoachId { get; init; }
    public Guid AthleteId { get; init; }
    public Guid ProgramTemplateId { get; init; }
    public DateTime StartDate { get; init; }
    public string? Notes { get; init; }
}
