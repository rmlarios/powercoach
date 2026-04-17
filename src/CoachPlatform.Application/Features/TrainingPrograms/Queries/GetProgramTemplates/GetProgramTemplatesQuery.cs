using CoachPlatform.Application.Shared.DTOs;
using CoachPlatform.Application.Shared.Interfaces;
using MediatR;

namespace CoachPlatform.Application.Features.TrainingPrograms.Queries.GetProgramTemplates;

/// <summary>
/// Query to get program templates for a coach.
/// </summary>
public record GetProgramTemplatesQuery : IRequest<PagedResult<ProgramTemplateListItemDto>>, ITenantRequest
{
    public Guid CoachId { get; init; }
    public bool? IsActive { get; init; }
    public string? SearchTerm { get; init; }
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}
