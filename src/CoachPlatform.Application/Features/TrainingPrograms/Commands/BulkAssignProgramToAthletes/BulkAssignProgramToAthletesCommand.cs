using CoachPlatform.Application.Shared.Interfaces;
using MediatR;

namespace CoachPlatform.Application.Features.TrainingPrograms.Commands.BulkAssignProgramToAthletes;

/// <summary>
/// Command to assign a program template to multiple athletes at once.
/// </summary>
public record BulkAssignProgramToAthletesCommand : IRequest<List<Guid>>, ITenantRequest
{
    public Guid CoachId { get; init; }
    public Guid ProgramTemplateId { get; init; }
    public List<Guid> AthleteIds { get; init; } = new();
    public DateTime StartDate { get; init; }
    public string? Notes { get; init; }
}
