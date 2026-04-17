using CoachPlatform.Application.Shared.Interfaces;
using MediatR;

namespace CoachPlatform.Application.Features.TrainingPrograms.Commands.UpdateProgramTemplate;

/// <summary>
/// Command to update program template metadata (name, description, duration).
/// </summary>
public record UpdateProgramTemplateCommand : IRequest<Unit>, ITenantRequest
{
    public Guid CoachId { get; init; }
    public Guid ProgramTemplateId { get; init; }
    public string Name { get; init; } = null!;
    public string? Description { get; init; }
    public int DurationWeeks { get; init; }
}
