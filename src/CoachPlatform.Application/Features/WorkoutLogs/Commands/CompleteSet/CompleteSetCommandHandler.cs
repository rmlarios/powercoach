using CoachPlatform.Application.Shared.Exceptions;
using CoachPlatform.Application.Shared.Interfaces;
using CoachPlatform.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CoachPlatform.Application.Features.WorkoutLogs.Commands.CompleteSet;

/// <summary>
/// Handler for CompleteSetCommand.
/// Marks a specific exercise log (set) as completed.
/// </summary>
public class CompleteSetCommandHandler : IRequestHandler<CompleteSetCommand, Unit>
{
    private readonly IApplicationDbContext _context;

    public CompleteSetCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Unit> Handle(CompleteSetCommand request, CancellationToken cancellationToken)
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

        exerciseLog.Complete();

        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
