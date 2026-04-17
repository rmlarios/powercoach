using CoachPlatform.Application.Shared.Exceptions;
using CoachPlatform.Application.Shared.Interfaces;
using CoachPlatform.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CoachPlatform.Application.Features.WorkoutLogs.Commands.UpdateSet;

/// <summary>
/// Handler for UpdateSetCommand.
/// Updates the actual weight, reps, RPE, and notes for a specific exercise log (set).
/// </summary>
public class UpdateSetCommandHandler : IRequestHandler<UpdateSetCommand, Unit>
{
    private readonly IApplicationDbContext _context;

    public UpdateSetCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Unit> Handle(UpdateSetCommand request, CancellationToken cancellationToken)
    {
        var exerciseLog = await _context.AthleteExerciseLogs
            .Include(l => l.Workout)
                .ThenInclude(w => w.AthleteProgram)
            .FirstOrDefaultAsync(l => l.Id == request.ExerciseLogId
                                      && l.WorkoutId == request.WorkoutId
                                      && l.Workout.AthleteProgram.AthleteId == request.AthleteId,
                cancellationToken);

        if (exerciseLog is null)
            throw new NotFoundException(nameof(AthleteExerciseLog), request.ExerciseLogId);

        exerciseLog.Update(request.Reps, request.Weight, request.Rpe, request.Notes);

        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
