using MediatR;

namespace CoachPlatform.Application.Features.Subscriptions.Commands.CancelSubscription;

/// <summary>
/// Command to cancel a Subscription.
/// </summary>
public record CancelSubscriptionCommand : IRequest<Unit>
{
    public Guid Id { get; init; }
    public string? Reason { get; init; }
}
