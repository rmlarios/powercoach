using CoachPlatform.Application.Shared.DTOs;
using CoachPlatform.Application.Shared.Interfaces;
using MediatR;

namespace CoachPlatform.Application.Features.TrainingPrograms.Queries.GetProgramTemplateById;

/// <summary>
/// Query to get a program template by ID with full details.
/// </summary>
public record GetProgramTemplateByIdQuery : IRequest<ProgramTemplateDetailDto?>, ITenantRequest
{
    public Guid CoachId { get; init; }
    public Guid Id { get; init; }
}
