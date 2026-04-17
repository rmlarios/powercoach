using CoachPlatform.Application.Shared.Exceptions;
using CoachPlatform.Application.Shared.Interfaces;
using CoachPlatform.Domain.Entities;
using CoachPlatform.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CoachPlatform.Application.Features.TrainingPrograms.Commands.LogWorkout;

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
        // Get the workout with athlete program
        var workout = await _context.AthleteWorkouts
            .Include(w => w.AthleteProgram)
            .Include(w => w.ExerciseLogs)
            .FirstOrDefaultAsync(w => w.Id == request.WorkoutId, cancellationToken);

        if (workout == null)
        {
            throw new NotFoundException(nameof(AthleteWorkout), request.WorkoutId);
        }

        // Verify the program is active
        if (workout.AthleteProgram.Status != ProgramStatus.Active && 
            workout.AthleteProgram.Status != ProgramStatus.NotStarted)
        {
            throw new InvalidOperationException("Cannot log workout for a program that is not active.");
        }

        // If program hasn't started yet, start it
        if (workout.AthleteProgram.Status == ProgramStatus.NotStarted)
        {
            workout.AthleteProgram.Start();
        }

        // Verify exercises exist
        var exerciseIds = request.ExerciseSets.Select(s => s.ExerciseId).Distinct().ToList();
        var validExerciseIds = await _context.Exercises
            .Where(e => exerciseIds.Contains(e.Id))
            .Select(e => e.Id)
            .ToListAsync(cancellationToken);

        var invalidExerciseIds = exerciseIds.Except(validExerciseIds).ToList();
        if (invalidExerciseIds.Any())
        {
            throw new NotFoundException(nameof(Exercise), string.Join(", ", invalidExerciseIds));
        }

        // Log each exercise set
        foreach (var exerciseSet in request.ExerciseSets)
        {
            workout.LogExercise(
                exerciseId: exerciseSet.ExerciseId,
                setNumber: exerciseSet.SetNumber,
                reps: exerciseSet.Reps,
                weight: exerciseSet.Weight,
                rpe: exerciseSet.Rpe,
                notes: exerciseSet.Notes);
        }

        // Complete the workout
        workout.Complete(
            durationMinutes: request.DurationMinutes,
            fatigueRating: request.FatigueRating,
            notes: request.Notes);

        await _context.SaveChangesAsync(cancellationToken);

        return workout.Id;
    }
}
