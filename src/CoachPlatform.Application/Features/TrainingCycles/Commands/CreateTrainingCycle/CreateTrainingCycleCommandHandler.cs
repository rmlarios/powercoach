using CoachPlatform.Application.Shared.Exceptions;
using CoachPlatform.Application.Shared.Interfaces;
using CoachPlatform.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CoachPlatform.Application.Features.TrainingCycles.Commands.CreateTrainingCycle;

/// <summary>
/// Handler for CreateTrainingCycleCommand.
/// </summary>
public class CreateTrainingCycleCommandHandler : IRequestHandler<CreateTrainingCycleCommand, Guid>
{
    private readonly IApplicationDbContext _context;

    public CreateTrainingCycleCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(CreateTrainingCycleCommand request, CancellationToken cancellationToken)
    {
        // Verify athlete exists
        var athleteExists = await _context.Athletes
            .AnyAsync(a => a.Id == request.AthleteId, cancellationToken);

        if (!athleteExists)
        {
            throw new NotFoundException(nameof(Athlete), request.AthleteId);
        }

        // Create the training cycle
        var trainingCycle = TrainingCycle.Create(
            request.AthleteId,
            request.Name,
            request.DurationWeeks,
            request.StartDate);

        _context.TrainingCycles.Add(trainingCycle);
        await _context.SaveChangesAsync(cancellationToken);

        return trainingCycle.Id;
    }
}
