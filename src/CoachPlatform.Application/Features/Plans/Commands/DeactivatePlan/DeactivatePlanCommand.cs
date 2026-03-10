using MediatR;

namespace CoachPlatform.Application.Features.Plans.Commands.DeactivatePlan;

/// <summary>
/// Command to deactivate a Plan (mark as inactive without deleting).
/// </summary>
public record DeactivatePlanCommand : IRequest<Unit>
{
    public Guid Id { get; init; }
}
