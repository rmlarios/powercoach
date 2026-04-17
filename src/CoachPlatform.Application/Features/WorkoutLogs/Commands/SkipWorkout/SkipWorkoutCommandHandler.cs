using CoachPlatform.Application.Shared.Exceptions;
using CoachPlatform.Application.Shared.Interfaces;
using CoachPlatform.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CoachPlatform.Application.Features.WorkoutLogs.Commands.SkipWorkout;

/// <summary>
/// Handler for SkipWorkoutCommand.
/// Marks a workout as skipped with an optional reason.
/// </summary>
public class SkipWorkoutCommandHandler : IRequestHandler<SkipWorkoutCommand, Unit>
{
    private readonly IApplicationDbContext _context;

    public SkipWorkoutCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Unit> Handle(SkipWorkoutCommand request, CancellationToken cancellationToken)
    {
        var workout = await _context.AthleteWorkouts
            .Include(w => w.AthleteProgram)
            .FirstOrDefaultAsync(w => w.Id == request.WorkoutId
                                      && w.AthleteProgram.AthleteId == request.AthleteId,
                cancellationToken);

        if (workout is null)
            throw new NotFoundException(nameof(AthleteWorkout), request.WorkoutId);

        workout.Skip(request.Reason);

        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
