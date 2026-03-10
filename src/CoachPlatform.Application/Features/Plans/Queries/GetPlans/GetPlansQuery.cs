using CoachPlatform.Application.Shared.DTOs;
using CoachPlatform.Application.Shared.Interfaces;
using MediatR;

namespace CoachPlatform.Application.Features.Plans.Queries.GetPlans;

/// <summary>
/// Query to get all plans for a specific coach.
/// </summary>
public record GetPlansQuery : IRequest<IReadOnlyList<PlanListItemDto>>, ITenantRequest
{
    public Guid CoachId { get; init; }
    public bool? IsActive { get; init; }
}
