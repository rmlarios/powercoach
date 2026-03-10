using CoachPlatform.Application.Shared.DTOs;
using CoachPlatform.Application.Shared.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CoachPlatform.Application.Features.TrainingCycles.Queries.GetAthleteTrainingCycles;

/// <summary>
/// Handler for GetAthleteTrainingCyclesQuery.
/// </summary>
public class GetAthleteTrainingCyclesQueryHandler : IRequestHandler<GetAthleteTrainingCyclesQuery, IEnumerable<TrainingCycleListItemDto>>
{
    private readonly IApplicationDbContext _context;

    public GetAthleteTrainingCyclesQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<TrainingCycleListItemDto>> Handle(GetAthleteTrainingCyclesQuery request, CancellationToken cancellationToken)
    {
        var today = DateTime.UtcNow.Date;

        var trainingCycles = await _context.TrainingCycles
            .AsNoTracking()
            .Where(tc => tc.AthleteId == request.AthleteId)
            .OrderByDescending(tc => tc.StartDate)
            .Select(tc => new TrainingCycleListItemDto
            {
                Id = tc.Id,
                AthleteId = tc.AthleteId,
                Name = tc.Name,
                DurationWeeks = tc.DurationWeeks,
                StartDate = tc.StartDate,
                EndDate = tc.EndDate,
                IsActive = today >= tc.StartDate && today <= tc.EndDate
            })
            .ToListAsync(cancellationToken);

        return trainingCycles;
    }
}
