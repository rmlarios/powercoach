using CoachPlatform.Application.Shared.Interfaces;
using MediatR;

namespace CoachPlatform.Application.Features.Subscriptions.Commands.CreateSubscription;

/// <summary>
/// Command to create a new Subscription.
/// </summary>
public record CreateSubscriptionCommand : IRequest<Guid>, IAthleteOwnedRequest
{
    public Guid AthleteId { get; init; }
    public Guid PlanId { get; init; }
    public DateTime StartDate { get; init; }
}
