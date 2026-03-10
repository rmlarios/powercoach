using CoachPlatform.Application.Shared.DTOs;
using CoachPlatform.Application.Shared.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CoachPlatform.Application.Features.Subscriptions.Queries.GetAthleteSubscriptions;

/// <summary>
/// Handler for GetAthleteSubscriptionsQuery.
/// </summary>
public class GetAthleteSubscriptionsQueryHandler : IRequestHandler<GetAthleteSubscriptionsQuery, IReadOnlyList<SubscriptionListItemDto>>
{
    private readonly IApplicationDbContext _context;

    public GetAthleteSubscriptionsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<SubscriptionListItemDto>> Handle(
        GetAthleteSubscriptionsQuery request,
        CancellationToken cancellationToken)
    {
        var subscriptions = await _context.Subscriptions
            .AsNoTracking()
            .Include(s => s.Athlete)
            .Include(s => s.Plan)
            .Where(s => s.AthleteId == request.AthleteId)
            .OrderByDescending(s => s.StartDate)
            .ToListAsync(cancellationToken);

        return subscriptions.Select(s => new SubscriptionListItemDto
        {
            Id = s.Id,
            AthleteName = $"{s.Athlete.Name.FirstName} {s.Athlete.Name.LastName}",
            PlanName = s.Plan.Name,
            Status = s.Status,
            StartDate = s.StartDate,
            EndDate = s.EndDate,
            RemainingDays = s.GetRemainingDays()
        }).ToList();
    }
}
