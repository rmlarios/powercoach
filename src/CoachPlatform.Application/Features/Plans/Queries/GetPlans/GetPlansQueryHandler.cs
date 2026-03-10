using CoachPlatform.Application.Shared.DTOs;
using CoachPlatform.Application.Shared.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CoachPlatform.Application.Features.Plans.Queries.GetPlans;

/// <summary>
/// Handler for GetPlansQuery.
/// </summary>
public class GetPlansQueryHandler : IRequestHandler<GetPlansQuery, IReadOnlyList<PlanListItemDto>>
{
    private readonly IApplicationDbContext _context;

    public GetPlansQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<PlanListItemDto>> Handle(
        GetPlansQuery request,
        CancellationToken cancellationToken)
    {
        var query = _context.Plans
            .AsNoTracking()
            .Where(p => p.CoachId == request.CoachId);

        // Apply active filter if specified
        if (request.IsActive.HasValue)
        {
            query = query.Where(p => p.IsActive == request.IsActive.Value);
        }

        var plans = await query
            .OrderBy(p => p.DisplayOrder)
            .ThenBy(p => p.Name)
            .ToListAsync(cancellationToken);

        return plans.Select(p => new PlanListItemDto
        {
            Id = p.Id,
            Name = p.Name,
            Price = p.Price.Amount,
            Currency = p.Price.Currency,
            PlanType = p.PlanType,
            IsActive = p.IsActive,
            DisplayOrder = p.DisplayOrder
        }).ToList();
    }
}
