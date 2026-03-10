using CoachPlatform.Application.Shared.DTOs;
using CoachPlatform.Application.Shared.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CoachPlatform.Application.Features.Payments.Queries.GetPaymentsByAthlete;

/// <summary>
/// Handler for GetPaymentsByAthleteQuery.
/// </summary>
public class GetPaymentsByAthleteQueryHandler : IRequestHandler<GetPaymentsByAthleteQuery, IReadOnlyList<PaymentListItemDto>>
{
    private readonly IApplicationDbContext _context;

    public GetPaymentsByAthleteQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<PaymentListItemDto>> Handle(
        GetPaymentsByAthleteQuery request,
        CancellationToken cancellationToken)
    {
        var payments = await _context.Payments
            .AsNoTracking()
            .Include(p => p.Subscription)
                .ThenInclude(s => s.Athlete)
            .Where(p => p.Subscription.AthleteId == request.AthleteId)
            .OrderByDescending(p => p.PaymentDate)
            .ToListAsync(cancellationToken);

        return payments.Select(p => new PaymentListItemDto
        {
            Id = p.Id,
            AthleteName = $"{p.Subscription.Athlete.Name.FirstName} {p.Subscription.Athlete.Name.LastName}",
            Amount = p.Amount.Amount,
            Currency = p.Amount.Currency,
            Status = p.Status,
            PaymentDate = p.PaymentDate,
            PaymentMethod = p.PaymentMethod
        }).ToList();
    }
}
