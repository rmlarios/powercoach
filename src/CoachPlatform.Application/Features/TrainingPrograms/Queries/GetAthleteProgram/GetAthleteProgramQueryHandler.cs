using CoachPlatform.Application.Shared.DTOs;
using CoachPlatform.Application.Shared.Interfaces;
using CoachPlatform.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CoachPlatform.Application.Features.TrainingPrograms.Queries.GetAthleteProgram;

/// <summary>
/// Handler for GetAthleteProgramQuery.
/// </summary>
public class GetAthleteProgramQueryHandler : IRequestHandler<GetAthleteProgramQuery, AthleteCurrentProgramDto?>
{
    private readonly IApplicationDbContext _context;

    public GetAthleteProgramQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<AthleteCurrentProgramDto?> Handle(
        GetAthleteProgramQuery request, 
        CancellationToken cancellationToken)
    {
        // Get the active or pending program for the athlete
        var athleteProgram = await _context.AthletePrograms
            .Include(ap => ap.ProgramTemplate)
                .ThenInclude(pt => pt.Weeks.OrderBy(w => w.WeekNumber))
                    .ThenInclude(w => w.Days.OrderBy(d => d.DayNumber))
                        .ThenInclude(d => d.Exercises.OrderBy(e => e.Order))
                            .ThenInclude(e => e.Exercise)
            .Include(ap => ap.Workouts)
            .FirstOrDefaultAsync(ap => 
                ap.AthleteId == request.AthleteId && 
                (ap.Status == ProgramStatus.Active || ap.Status == ProgramStatus.NotStarted), 
                cancellationToken);

        if (athleteProgram == null)
        {
            return null;
        }

        var weekSchedule = new List<AthleteWeekScheduleDto>();

        foreach (var weekTemplate in athleteProgram.ProgramTemplate.Weeks.OrderBy(w => w.WeekNumber))
        {
            var daySchedule = new List<AthleteDayScheduleDto>();

            foreach (var dayTemplate in weekTemplate.Days.OrderBy(d => d.DayNumber))
            {
                // Find the corresponding workout
                var workout = athleteProgram.Workouts
                    .FirstOrDefault(w => w.WeekNumber == weekTemplate.WeekNumber && w.DayNumber == dayTemplate.DayNumber);

                daySchedule.Add(new AthleteDayScheduleDto
                {
                    DayNumber = dayTemplate.DayNumber,
                    Focus = dayTemplate.Focus,
                    Notes = dayTemplate.Notes,
                    ScheduledDate = workout?.ScheduledDate ?? athleteProgram.StartDate.AddDays(
                        ((weekTemplate.WeekNumber - 1) * 7) + (dayTemplate.DayNumber - 1)),
                    IsCompleted = workout?.IsCompleted ?? false,
                    WorkoutId = workout?.Id,
                    Exercises = dayTemplate.Exercises.Select(e => new ProgramExerciseTemplateDto
                    {
                        Id = e.Id,
                        ExerciseId = e.ExerciseId,
                        ExerciseName = e.Exercise.Name,
                        Sets = e.Sets,
                        Reps = e.Reps,
                        TargetRpe = e.TargetRpe,
                        RestSeconds = e.RestSeconds,
                        Notes = e.Notes,
                        Order = e.Order
                    }).ToList()
                });
            }

            weekSchedule.Add(new AthleteWeekScheduleDto
            {
                WeekNumber = weekTemplate.WeekNumber,
                IsCurrentWeek = weekTemplate.WeekNumber == athleteProgram.CurrentWeek,
                Days = daySchedule
            });
        }

        return new AthleteCurrentProgramDto
        {
            AthleteProgramId = athleteProgram.Id,
            ProgramName = athleteProgram.ProgramTemplate.Name,
            ProgramDescription = athleteProgram.ProgramTemplate.Description,
            CurrentWeek = athleteProgram.CurrentWeek,
            TotalWeeks = athleteProgram.ProgramTemplate.DurationWeeks,
            Status = athleteProgram.Status,
            WeekSchedule = weekSchedule
        };
    }
}
