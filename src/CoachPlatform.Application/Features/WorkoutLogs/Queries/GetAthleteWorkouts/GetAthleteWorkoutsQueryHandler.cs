using CoachPlatform.Application.Shared.DTOs;
using CoachPlatform.Application.Shared.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CoachPlatform.Application.Features.WorkoutLogs.Queries.GetAthleteWorkouts;

/// <summary>
/// Handler for GetAthleteWorkoutsQuery.
/// </summary>
public class GetAthleteWorkoutsQueryHandler : IRequestHandler<GetAthleteWorkoutsQuery, IEnumerable<WorkoutLogListItemDto>>
{
    private readonly IApplicationDbContext _context;

    public GetAthleteWorkoutsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<WorkoutLogListItemDto>> Handle(GetAthleteWorkoutsQuery request, CancellationToken cancellationToken)
    {
        var workouts = await _context.WorkoutLogs
            .AsNoTracking()
            .Where(w => w.AthleteId == request.AthleteId)
            .OrderByDescending(w => w.WorkoutDate)
            .ThenBy(w => w.ExerciseName)
            .Select(w => new WorkoutLogListItemDto
            {
                Id = w.Id,
                AthleteId = w.AthleteId,
                ExerciseName = w.ExerciseName,
                Sets = w.Sets,
                Reps = w.Reps,
                Weight = w.Weight,
                RPE = w.RPE,
                WorkoutDate = w.WorkoutDate
            })
            .ToListAsync(cancellationToken);

        return workouts;
    }
}
