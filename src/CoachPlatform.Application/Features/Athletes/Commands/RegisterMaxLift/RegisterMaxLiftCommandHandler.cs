using CoachPlatform.Application.Shared.Exceptions;
using CoachPlatform.Application.Shared.Interfaces;
using CoachPlatform.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CoachPlatform.Application.Features.Athletes.Commands.RegisterMaxLift;

/// <summary>
/// Handler for RegisterMaxLiftCommand.
/// </summary>
public class RegisterMaxLiftCommandHandler : IRequestHandler<RegisterMaxLiftCommand, Guid>
{
    private readonly IApplicationDbContext _context;

    public RegisterMaxLiftCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(RegisterMaxLiftCommand request, CancellationToken cancellationToken)
    {
        // Validate athlete exists
        var athleteExists = await _context.Athletes
            .AnyAsync(a => a.Id == request.AthleteId, cancellationToken);
        
        if (!athleteExists)
        {
            throw new NotFoundException("Athlete", request.AthleteId);
        }

        // Validate exercise exists
        var exerciseExists = await _context.Exercises
            .AnyAsync(e => e.Id == request.ExerciseId, cancellationToken);
        
        if (!exerciseExists)
        {
            throw new NotFoundException("Exercise", request.ExerciseId);
        }

        // Create the max lift record
        AthleteMaxLift maxLift;
        
        if (request.IsTested)
        {
            maxLift = AthleteMaxLift.CreateTested(
                request.AthleteId,
                request.ExerciseId,
                request.Weight,
                request.RecordedAt,
                request.Notes);
        }
        else
        {
            maxLift = AthleteMaxLift.CreateEstimated(
                request.AthleteId,
                request.ExerciseId,
                request.Weight,
                request.EstimationDetails ?? "Manual estimation",
                request.RecordedAt,
                request.Notes);
        }

        _context.AthleteMaxLifts.Add(maxLift);
        await _context.SaveChangesAsync(cancellationToken);

        return maxLift.Id;
    }
}
