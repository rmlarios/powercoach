using CoachPlatform.Application.Shared.DTOs;
using CoachPlatform.Application.Shared.Interfaces;
using MediatR;

namespace CoachPlatform.Application.Features.TrainingPrograms.Commands.SaveProgramTemplate;

/// <summary>
/// Command to bulk-save an entire program template (weeks, days, exercises).
/// Replaces all existing structure with the provided data.
/// </summary>
public record SaveProgramTemplateCommand : IRequest<Guid>, ITenantRequest
{
    public Guid CoachId { get; init; }
    public Guid ProgramTemplateId { get; init; }
    public SaveProgramTemplateDto Data { get; init; } = null!;
}
