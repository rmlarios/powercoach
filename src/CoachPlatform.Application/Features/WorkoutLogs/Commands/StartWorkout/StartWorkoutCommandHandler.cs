using CoachPlatform.Application.Shared.Exceptions;
using CoachPlatform.Application.Shared.Interfaces;
using CoachPlatform.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CoachPlatform.Application.Features.WorkoutLogs.Commands.StartWorkout;

/// <summary>
/// Handler for StartWorkoutCommand.
/// </summary>
public class StartWorkoutCommandHandler : IRequestHandler<StartWorkoutCommand, Unit>
{
    private readonly IApplicationDbContext _context;

    public StartWorkoutCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Unit> Handle(StartWorkoutCommand request, CancellationToken cancellationToken)
    {
        var workout = await _context.AthleteWorkouts
            .Include(w => w.AthleteProgram)
            .FirstOrDefaultAsync(w => w.Id == request.WorkoutId
                                      && w.AthleteProgram.AthleteId == request.AthleteId,
                cancellationToken);

        if (workout is null)
            throw new NotFoundException(nameof(AthleteWorkout), request.WorkoutId);

        workout.Start();

        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
