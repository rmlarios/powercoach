using CoachPlatform.Application.Shared.Interfaces;
using MediatR;

namespace CoachPlatform.Application.Features.TrainingPrograms.Commands.CreateProgramTemplate;

/// <summary>
/// Command to create a new training program template.
/// </summary>
public record CreateProgramTemplateCommand : IRequest<Guid>, ITenantRequest
{
    public Guid CoachId { get; init; }
    public string Name { get; init; } = null!;
    public string? Description { get; init; }
    public int DurationWeeks { get; init; }
    public int? DaysPerWeek { get; init; }
}
