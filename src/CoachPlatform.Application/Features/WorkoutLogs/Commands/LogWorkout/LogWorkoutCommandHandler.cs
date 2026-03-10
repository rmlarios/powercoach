using CoachPlatform.Application.Shared.Exceptions;
using CoachPlatform.Application.Shared.Interfaces;
using CoachPlatform.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CoachPlatform.Application.Features.WorkoutLogs.Commands.LogWorkout;

/// <summary>
/// Handler for LogWorkoutCommand.
/// </summary>
public class LogWorkoutCommandHandler : IRequestHandler<LogWorkoutCommand, Guid>
{
    private readonly IApplicationDbContext _context;

    public LogWorkoutCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(LogWorkoutCommand request, CancellationToken cancellationToken)
    {
        // Verify athlete exists
        var athleteExists = await _context.Athletes
            .AnyAsync(a => a.Id == request.AthleteId, cancellationToken);

        if (!athleteExists)
        {
            throw new NotFoundException(nameof(Athlete), request.AthleteId);
        }

        // Get the exercise
        var exercise = await _context.Exercises
            .FirstOrDefaultAsync(e => e.Id == request.ExerciseId, cancellationToken);

        if (exercise is null)
        {
            throw new NotFoundException(nameof(Exercise), request.ExerciseId);
        }

        // Create the workout log
        var workoutLog = WorkoutLog.Create(
            request.AthleteId,
            exercise,
            request.Sets,
            request.Reps,
            request.WorkoutDate,
            request.Weight,
            request.RPE,
            request.Notes);

        _context.WorkoutLogs.Add(workoutLog);
        await _context.SaveChangesAsync(cancellationToken);

        return workoutLog.Id;
    }
}
