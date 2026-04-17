using CoachPlatform.Application.Shared.DTOs;
using CoachPlatform.Application.Shared.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CoachPlatform.Application.Features.WorkoutLogs.Queries.GetWorkoutHistory;

/// <summary>
/// Handler for GetWorkoutHistoryQuery.
/// Returns paginated workout history for an athlete, ordered by most recent first.
/// </summary>
public class GetWorkoutHistoryQueryHandler : IRequestHandler<GetWorkoutHistoryQuery, PagedResult<WorkoutHistoryItemDto>>
{
    private readonly IApplicationDbContext _context;

    public GetWorkoutHistoryQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<WorkoutHistoryItemDto>> Handle(GetWorkoutHistoryQuery request, CancellationToken cancellationToken)
    {
        var query = _context.AthleteWorkouts
            .AsNoTracking()
            .Include(w => w.ExerciseLogs)
            .Where(w => w.AthleteProgram.AthleteId == request.AthleteId);

        // Optional filter by specific program
        if (request.AthleteProgramId.HasValue)
        {
            query = query.Where(w => w.AthleteProgramId == request.AthleteProgramId.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        // Get day templates for names/focus (join on week+day numbers)
        var workouts = await query
            .OrderByDescending(w => w.ScheduledDate)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        // Get program template IDs for these workouts to fetch day templates
        var programIds = workouts
            .Select(w => w.AthleteProgramId)
            .Distinct()
            .ToList();

        var athletePrograms = await _context.AthletePrograms
            .AsNoTracking()
            .Where(ap => programIds.Contains(ap.Id))
            .Select(ap => new { ap.Id, ap.ProgramTemplateId })
            .ToListAsync(cancellationToken);

        var templateIds = athletePrograms.Select(ap => ap.ProgramTemplateId).Distinct().ToList();

        var dayTemplates = await _context.ProgramDayTemplates
            .AsNoTracking()
            .Where(d => templateIds.Contains(d.WeekTemplate.ProgramTemplateId))
            .Select(d => new
            {
                d.WeekTemplate.ProgramTemplateId,
                d.WeekTemplate.WeekNumber,
                d.DayNumber,
                d.Name,
                Focus = d.Focus.ToString()
            })
            .ToListAsync(cancellationToken);

        var items = workouts.Select(w =>
        {
            var programTemplateId = athletePrograms
                .FirstOrDefault(ap => ap.Id == w.AthleteProgramId)?.ProgramTemplateId;

            var dayTemplate = dayTemplates.FirstOrDefault(d =>
                d.ProgramTemplateId == programTemplateId
                && d.WeekNumber == w.WeekNumber
                && d.DayNumber == w.DayNumber);

            return new WorkoutHistoryItemDto
            {
                Id = w.Id,
                WeekNumber = w.WeekNumber,
                DayNumber = w.DayNumber,
                DayName = dayTemplate?.Name,
                Focus = dayTemplate?.Focus,
                ScheduledDate = w.ScheduledDate,
                Status = w.Status,
                CompletedDate = w.CompletedDate,
                DurationMinutes = w.DurationMinutes,
                FatigueRating = w.FatigueRating,
                ExerciseCount = w.ExerciseLogs.Select(l => l.ExerciseId).Distinct().Count(),
                CompletedSets = w.ExerciseLogs.Count(l => l.IsCompleted),
                TotalSets = w.ExerciseLogs.Count
            };
        }).ToList();

        return new PagedResult<WorkoutHistoryItemDto>
        {
            Items = items,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalCount = totalCount
        };
    }
}
