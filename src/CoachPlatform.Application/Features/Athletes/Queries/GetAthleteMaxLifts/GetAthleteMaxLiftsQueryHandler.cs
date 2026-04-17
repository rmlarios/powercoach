using CoachPlatform.Application.Shared.DTOs;
using CoachPlatform.Application.Shared.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CoachPlatform.Application.Features.Athletes.Queries.GetAthleteMaxLifts;

/// <summary>
/// Handler for GetAthleteMaxLiftsQuery.
/// Returns the latest max lift record for each exercise.
/// </summary>
public class GetAthleteMaxLiftsQueryHandler : IRequestHandler<GetAthleteMaxLiftsQuery, AthleteMaxLiftsDto?>
{
    private readonly IApplicationDbContext _context;

    public GetAthleteMaxLiftsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<AthleteMaxLiftsDto?> Handle(GetAthleteMaxLiftsQuery request, CancellationToken cancellationToken)
    {
        var athlete = await _context.Athletes
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == request.AthleteId, cancellationToken);

        if (athlete is null)
        {
            return null;
        }

        // Get the latest max lift for each exercise
        var maxLiftsQuery = _context.AthleteMaxLifts
            .Where(m => m.AthleteId == request.AthleteId);

        if (request.ExerciseId.HasValue)
        {
            maxLiftsQuery = maxLiftsQuery.Where(m => m.ExerciseId == request.ExerciseId.Value);
        }

        // Group by exercise and get the most recent record for each
        var maxLifts = await maxLiftsQuery
            .Include(m => m.Exercise)
            .GroupBy(m => m.ExerciseId)
            .Select(g => g.OrderByDescending(m => m.RecordedAt).First())
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return new AthleteMaxLiftsDto
        {
            AthleteId = athlete.Id,
            AthleteName = $"{athlete.Name.FirstName} {athlete.Name.LastName}",
            MaxLifts = maxLifts.Select(m => new MaxLiftDto
            {
                Id = m.Id,
                ExerciseId = m.ExerciseId,
                ExerciseName = m.Exercise.Name,
                Weight = m.Weight,
                IsTested = m.IsTested,
                RecordedAt = m.RecordedAt,
                Notes = m.Notes,
                EstimationDetails = m.EstimationDetails
            }).ToList()
        };
    }
}
