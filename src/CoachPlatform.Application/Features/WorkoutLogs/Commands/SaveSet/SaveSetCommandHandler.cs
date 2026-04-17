using CoachPlatform.Application.Shared.DTOs;
using CoachPlatform.Application.Shared.Exceptions;
using CoachPlatform.Application.Shared.Interfaces;
using CoachPlatform.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CoachPlatform.Application.Features.WorkoutLogs.Commands.SaveSet;

/// <summary>
/// Handler for SaveSetCommand.
/// Creates a new exercise log entry or updates an existing one (upsert pattern).
/// </summary>
public class SaveSetCommandHandler : IRequestHandler<SaveSetCommand, WorkoutSetDto>
{
    private readonly IApplicationDbContext _context;

    public SaveSetCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<WorkoutSetDto> Handle(SaveSetCommand request, CancellationToken cancellationToken)
    {
        // Verify workout belongs to athlete
        var workout = await _context.AthleteWorkouts
            .Include(w => w.AthleteProgram)
            .Include(w => w.ExerciseLogs)
            .FirstOrDefaultAsync(w => w.Id == request.WorkoutId
                                      && w.AthleteProgram.AthleteId == request.AthleteId,
                cancellationToken);

        if (workout is null)
            throw new NotFoundException(nameof(AthleteWorkout), request.WorkoutId);

        AthleteExerciseLog? exerciseLog = null;

        // Try to find existing log entry
        if (request.ExerciseLogId.HasValue && request.ExerciseLogId != Guid.Empty)
        {
            exerciseLog = workout.ExerciseLogs
                .FirstOrDefault(l => l.Id == request.ExerciseLogId.Value);
        }

        // Fallback: find by exercise + set number
        exerciseLog ??= workout.ExerciseLogs
            .FirstOrDefault(l => l.ExerciseId == request.ExerciseId && l.SetNumber == request.SetNumber);

        if (exerciseLog is not null)
        {
            // Update existing
            exerciseLog.Update(request.Reps, request.Weight, request.Rpe, request.Notes);
            exerciseLog.SetTargets(request.TargetReps, request.TargetWeight);

            if (request.IsCompleted && !exerciseLog.IsCompleted && request.Reps > 0)
                exerciseLog.Complete();
        }
        else
        {
            // Create new log entry via the workout's domain method
            exerciseLog = workout.LogExercise(
                request.ExerciseId,
                request.SetNumber,
                request.Reps,
                request.Weight,
                request.Rpe,
                request.Notes,
                request.TargetReps,
                request.TargetWeight);

            // Explicitly track the new entity as Added
            _context.AthleteExerciseLogs.Add(exerciseLog);

            if (request.IsCompleted && request.Reps > 0)
                exerciseLog.Complete();
        }

        await _context.SaveChangesAsync(cancellationToken);

        return new WorkoutSetDto
        {
            Id = exerciseLog.Id,
            SetNumber = exerciseLog.SetNumber,
            TargetReps = exerciseLog.TargetReps,
            TargetWeight = exerciseLog.TargetWeight,
            ActualReps = exerciseLog.Reps,
            ActualWeight = exerciseLog.Weight,
            ActualRpe = exerciseLog.Rpe,
            IsCompleted = exerciseLog.IsCompleted,
            SkippedReason = exerciseLog.SkippedReason,
            Notes = exerciseLog.Notes
        };
    }
}
