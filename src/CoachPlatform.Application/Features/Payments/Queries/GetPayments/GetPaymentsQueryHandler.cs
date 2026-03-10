using CoachPlatform.Application.Shared.DTOs;
using CoachPlatform.Application.Shared.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CoachPlatform.Application.Features.Payments.Queries.GetPayments;

/// <summary>
/// Handler for GetPaymentsQuery.
/// </summary>
public class GetPaymentsQueryHandler : IRequestHandler<GetPaymentsQuery, IReadOnlyList<PaymentListItemDto>>
{
    private readonly IApplicationDbContext _context;

    public GetPaymentsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<PaymentListItemDto>> Handle(
        GetPaymentsQuery request,
        CancellationToken cancellationToken)
    {
        var query = _context.Payments
            .AsNoTracking()
            .Include(p => p.Subscription)
                .ThenInclude(s => s.Athlete)
            .AsQueryable();

        // Apply athlete filter
        if (request.AthleteId.HasValue)
        {
            query = query.Where(p => p.Subscription.AthleteId == request.AthleteId.Value);
        }

        // Apply date filters
        if (request.FromDate.HasValue)
        {
            query = query.Where(p => p.PaymentDate >= request.FromDate.Value);
        }

        if (request.ToDate.HasValue)
        {
            query = query.Where(p => p.PaymentDate <= request.ToDate.Value);
        }

        // Apply payment method filter
        if (!string.IsNullOrWhiteSpace(request.PaymentMethod))
        {
            var methodLower = request.PaymentMethod.ToLower();
            query = query.Where(p => p.PaymentMethod != null && p.PaymentMethod.ToLower() == methodLower);
        }

        var payments = await query
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
