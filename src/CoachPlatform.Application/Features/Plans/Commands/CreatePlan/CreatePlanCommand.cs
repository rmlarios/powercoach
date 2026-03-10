using CoachPlatform.Application.Shared.Interfaces;
using MediatR;

namespace CoachPlatform.Application.Features.Plans.Commands.CreatePlan;

/// <summary>
/// Command to create a new Plan.
/// </summary>
public record CreatePlanCommand : IRequest<Guid>, ITenantRequest
{
    public Guid CoachId { get; init; }
    public string Name { get; init; } = null!;
    public string? Description { get; init; }
    public decimal Price { get; init; }
    public string Currency { get; init; } = "USD";
    public int DurationInMonths { get; init; }
}
