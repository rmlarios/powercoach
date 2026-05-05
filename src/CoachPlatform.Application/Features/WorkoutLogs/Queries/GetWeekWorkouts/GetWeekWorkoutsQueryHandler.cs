using CoachPlatform.Application.Shared.DTOs;
using CoachPlatform.Application.Shared.Interfaces;
using CoachPlatform.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CoachPlatform.Application.Features.WorkoutLogs.Queries.GetWeekWorkouts;

/// <summary>
/// Handler for GetWeekWorkoutsQuery.
/// Returns all workouts for a specific week within the athlete's active program,
/// including lightweight status info for week-strip navigation.
/// </summary>
public class GetWeekWorkoutsQueryHandler : IRequestHandler<GetWeekWorkoutsQuery, WeekWorkoutsDto?>
{
    private readonly IApplicationDbContext _context;

    public GetWeekWorkoutsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<WeekWorkoutsDto?> Handle(GetWeekWorkoutsQuery request, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;

        // Find the athlete's active program
        var activeProgram = await _context.AthletePrograms
            .AsNoTracking()
            .Include(ap => ap.ProgramTemplate)
            .Where(ap => ap.AthleteId == request.AthleteId
                         && (ap.Status == ProgramStatus.Active
                             || (ap.Status == ProgramStatus.NotStarted
                                 && ap.StartDate <= now)))
            .FirstOrDefaultAsync(cancellationToken);

        if (activeProgram is null)
            return null;

        // Determine the week number
        var today = DateTime.UtcNow.Date;
        int weekNumber;

        if (request.WeekNumber.HasValue)
        {
            weekNumber = request.WeekNumber.Value;
        }
        else
        {
            // Find current week: prefer today's workout, then closest by date
            var todayWorkout = await _context.AthleteWorkouts
                .AsNoTracking()
                .Where(w => w.AthleteProgramId == activeProgram.Id
                            && w.ScheduledDate.Date == today)
                .FirstOrDefaultAsync(cancellationToken);

            if (todayWorkout != null)
            {
                weekNumber = todayWorkout.WeekNumber;
            }
            else
            {
                // Find closest upcoming workout
                var closest = await _context.AthleteWorkouts
                    .AsNoTracking()
                    .Where(w => w.AthleteProgramId == activeProgram.Id
                                && w.ScheduledDate.Date >= today)
                    .OrderBy(w => w.ScheduledDate)
                    .FirstOrDefaultAsync(cancellationToken);

                weekNumber = closest?.WeekNumber ?? 1;
            }
        }

        // Get total weeks in the program
        var totalWeeks = await _context.ProgramWeekTemplates
            .AsNoTracking()
            .Where(w => w.ProgramTemplateId == activeProgram.ProgramTemplateId)
            .CountAsync(cancellationToken);

        // Get all workouts for this week
        var weekWorkouts = await _context.AthleteWorkouts
            .AsNoTracking()
            .Include(w => w.ExerciseLogs)
            .Where(w => w.AthleteProgramId == activeProgram.Id
                        && w.WeekNumber == weekNumber)
            .OrderBy(w => w.DayNumber)
            .ToListAsync(cancellationToken);

        // Get day templates for names + focus
        var dayTemplates = await _context.ProgramDayTemplates
            .AsNoTracking()
            .Where(d => d.WeekTemplate.ProgramTemplateId == activeProgram.ProgramTemplateId
                        && d.WeekTemplate.WeekNumber == weekNumber)
            .Select(d => new
            {
                d.DayNumber,
                d.Name,
                Focus = d.Focus.ToString()
            })
            .ToListAsync(cancellationToken);

        var days = weekWorkouts.Select(w =>
        {
            var template = dayTemplates.FirstOrDefault(d => d.DayNumber == w.DayNumber);
            var isToday = w.ScheduledDate.Date == today;
            var exerciseCount = w.ExerciseLogs.Select(l => l.ExerciseId).Distinct().Count();
            var completedSets = w.ExerciseLogs.Count(l => l.IsCompleted);
            var totalSets = w.ExerciseLogs.Count;

            return new WeekDayDto
            {
                WorkoutId = w.Id,
                DayNumber = w.DayNumber,
                DayName = template?.Name,
                Focus = template?.Focus,
                ScheduledDate = w.ScheduledDate,
                Status = w.Status,
                IsToday = isToday,
                DurationMinutes = w.DurationMinutes,
                FatigueRating = w.FatigueRating,
                ExerciseCount = exerciseCount,
                CompletedSets = completedSets,
                TotalSets = totalSets
            };
        }).ToList();

        return new WeekWorkoutsDto
        {
            ProgramName = activeProgram.ProgramTemplate.Name,
            WeekNumber = weekNumber,
            TotalWeeks = totalWeeks,
            Days = days
        };
    }
}
