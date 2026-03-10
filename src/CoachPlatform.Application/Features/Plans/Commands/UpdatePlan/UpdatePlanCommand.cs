using MediatR;

namespace CoachPlatform.Application.Features.Plans.Commands.UpdatePlan;

/// <summary>
/// Command to update an existing Plan.
/// </summary>
public record UpdatePlanCommand : IRequest<Unit>
{
    public Guid Id { get; init; }
    public string Name { get; init; } = null!;
    public string? Description { get; init; }
    public decimal Price { get; init; }
    public string Currency { get; init; } = null!;
    public int DurationInMonths { get; init; }
}
