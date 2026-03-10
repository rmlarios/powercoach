using CoachPlatform.Application.Shared.DTOs;
using CoachPlatform.Application.Shared.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CoachPlatform.Application.Features.CheckIns.Queries.GetAthleteCheckIns;

/// <summary>
/// Handler for GetAthleteCheckInsQuery.
/// </summary>
public class GetAthleteCheckInsQueryHandler : IRequestHandler<GetAthleteCheckInsQuery, IEnumerable<CheckInListItemDto>>
{
    private readonly IApplicationDbContext _context;

    public GetAthleteCheckInsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<CheckInListItemDto>> Handle(GetAthleteCheckInsQuery request, CancellationToken cancellationToken)
    {
        var checkIns = await _context.CheckIns
            .AsNoTracking()
            .Where(c => c.AthleteId == request.AthleteId)
            .OrderByDescending(c => c.CheckInDate)
            .Select(c => new CheckInListItemDto
            {
                Id = c.Id,
                AthleteId = c.AthleteId,
                CheckInDate = c.CheckInDate,
                Weight = c.Weight,
                IsReviewed = c.IsReviewed
            })
            .ToListAsync(cancellationToken);

        return checkIns;
    }
}
