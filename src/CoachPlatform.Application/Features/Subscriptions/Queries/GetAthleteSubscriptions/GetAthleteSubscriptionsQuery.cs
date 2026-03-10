using CoachPlatform.Application.Shared.DTOs;
using CoachPlatform.Application.Shared.Interfaces;
using MediatR;

namespace CoachPlatform.Application.Features.Subscriptions.Queries.GetAthleteSubscriptions;

/// <summary>
/// Query to get all subscriptions for a specific athlete.
/// </summary>
public record GetAthleteSubscriptionsQuery : IRequest<IReadOnlyList<SubscriptionListItemDto>>, IAthleteOwnedRequest
{
    public Guid AthleteId { get; init; }
}
