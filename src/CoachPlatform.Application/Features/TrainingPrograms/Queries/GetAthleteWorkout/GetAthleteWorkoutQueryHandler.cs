using CoachPlatform.Application.Shared.DTOs;
using CoachPlatform.Application.Shared.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CoachPlatform.Application.Features.TrainingPrograms.Queries.GetAthleteWorkout;

/// <summary>
/// Handler for GetAthleteWorkoutQuery.
/// </summary>
public class GetAthleteWorkoutQueryHandler : IRequestHandler<GetAthleteWorkoutQuery, AthleteWorkoutDto?>
{
    private readonly IApplicationDbContext _context;

    public GetAthleteWorkoutQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<AthleteWorkoutDto?> Handle(
        GetAthleteWorkoutQuery request, 
        CancellationToken cancellationToken)
    {
        var workout = await _context.AthleteWorkouts
            .Include(w => w.ExerciseLogs)
                .ThenInclude(l => l.Exercise)
            .Include(w => w.AthleteProgram)
                .ThenInclude(ap => ap.ProgramTemplate)
                    .ThenInclude(pt => pt.Weeks)
                        .ThenInclude(wk => wk.Days)
                            .ThenInclude(d => d.Exercises)
                                .ThenInclude(e => e.Exercise)
            .FirstOrDefaultAsync(w => w.Id == request.WorkoutId, cancellationToken);

        if (workout == null)
        {
            return null;
        }

        // Find the corresponding day template
        var dayTemplate = workout.AthleteProgram.ProgramTemplate.Weeks
            .FirstOrDefault(w => w.WeekNumber == workout.WeekNumber)?
            .Days.FirstOrDefault(d => d.DayNumber == workout.DayNumber);

        return new AthleteWorkoutDto
        {
            Id = workout.Id,
            WeekNumber = workout.WeekNumber,
            DayNumber = workout.DayNumber,
            Focus = dayTemplate?.Focus ?? Domain.Enums.DayFocus.FullBody,
            ScheduledDate = workout.ScheduledDate,
            CompletedDate = workout.CompletedDate,
            IsCompleted = workout.IsCompleted,
            DurationMinutes = workout.DurationMinutes,
            FatigueRating = workout.FatigueRating,
            Notes = workout.Notes,
            ExerciseLogs = workout.ExerciseLogs
                .OrderBy(l => l.ExerciseId)
                .ThenBy(l => l.SetNumber)
                .Select(l => new AthleteExerciseLogDto
                {
                    Id = l.Id,
                    ExerciseId = l.ExerciseId,
                    ExerciseName = l.Exercise.Name,
                    SetNumber = l.SetNumber,
                    Reps = l.Reps,
                    Weight = l.Weight,
                    Rpe = l.Rpe,
                    Notes = l.Notes,
                    CreatedAt = l.CreatedAt
                }).ToList(),
            PrescribedExercises = dayTemplate?.Exercises
                .OrderBy(e => e.Order)
                .Select(e => new ProgramExerciseTemplateDto
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
                }).ToList() ?? []
        };
    }
}
