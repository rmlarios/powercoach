using CoachPlatform.Application.Shared.Exceptions;
using CoachPlatform.Application.Shared.Interfaces;
using CoachPlatform.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CoachPlatform.Application.Features.CheckIns.Commands.CreateCheckIn;

/// <summary>
/// Handler for CreateCheckInCommand.
/// </summary>
public class CreateCheckInCommandHandler : IRequestHandler<CreateCheckInCommand, Guid>
{
    private readonly IApplicationDbContext _context;

    public CreateCheckInCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(CreateCheckInCommand request, CancellationToken cancellationToken)
    {
        // Verify athlete exists
        var athleteExists = await _context.Athletes
            .AnyAsync(a => a.Id == request.AthleteId, cancellationToken);

        if (!athleteExists)
        {
            throw new NotFoundException(nameof(Athlete), request.AthleteId);
        }

        // Create the check-in
        var checkIn = CheckIn.Create(request.AthleteId);

        // Set weight if provided
        if (request.Weight.HasValue)
        {
            checkIn.SetWeight(request.Weight.Value);
        }

        // Set fatigue level (using stress level as inverse - high fatigue = high stress)
        if (request.FatigueLevel.HasValue)
        {
            checkIn.SetStressLevel(request.FatigueLevel.Value);
        }

        // Set sleep quality
        if (request.SleepQuality.HasValue)
        {
            checkIn.SetSleepInfo(request.SleepQuality.Value, 0);
        }

        // Set motivation level (using energy level)
        if (request.MotivationLevel.HasValue)
        {
            checkIn.SetEnergyLevel(request.MotivationLevel.Value);
        }

        // Set notes
        if (!string.IsNullOrWhiteSpace(request.Notes))
        {
            checkIn.SetNotes(request.Notes);
        }

        _context.CheckIns.Add(checkIn);
        await _context.SaveChangesAsync(cancellationToken);

        return checkIn.Id;
    }
}
