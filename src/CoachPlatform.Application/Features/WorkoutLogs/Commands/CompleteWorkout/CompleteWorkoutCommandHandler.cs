using CoachPlatform.Application.Shared.Exceptions;
using CoachPlatform.Application.Shared.Interfaces;
using CoachPlatform.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CoachPlatform.Application.Features.WorkoutLogs.Commands.CompleteWorkout;

/// <summary>
/// Handler for CompleteWorkoutCommand.
/// Marks the workout as Completed or PartiallyCompleted based on set completion.
/// </summary>
public class CompleteWorkoutCommandHandler : IRequestHandler<CompleteWorkoutCommand, Unit>
{
    private readonly IApplicationDbContext _context;

    public CompleteWorkoutCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Unit> Handle(CompleteWorkoutCommand request, CancellationToken cancellationToken)
    {
        var workout = await _context.AthleteWorkouts
            .Include(w => w.AthleteProgram)
            .Include(w => w.ExerciseLogs)
            .FirstOrDefaultAsync(w => w.Id == request.WorkoutId
                                      && w.AthleteProgram.AthleteId == request.AthleteId,
                cancellationToken);

        if (workout is null)
            throw new NotFoundException(nameof(AthleteWorkout), request.WorkoutId);

        workout.Complete(request.DurationMinutes, request.FatigueRating, request.Notes);

        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
