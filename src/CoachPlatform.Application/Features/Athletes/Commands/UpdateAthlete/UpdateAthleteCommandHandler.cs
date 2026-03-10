using CoachPlatform.Application.Shared.Exceptions;
using CoachPlatform.Application.Shared.Interfaces;
using CoachPlatform.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CoachPlatform.Application.Features.Athletes.Commands.UpdateAthlete;

/// <summary>
/// Handler for UpdateAthleteCommand.
/// </summary>
public class UpdateAthleteCommandHandler : IRequestHandler<UpdateAthleteCommand, Unit>
{
    private readonly IApplicationDbContext _context;

    public UpdateAthleteCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Unit> Handle(UpdateAthleteCommand request, CancellationToken cancellationToken)
    {
        var athlete = await _context.Athletes
            .FirstOrDefaultAsync(a => a.Id == request.Id, cancellationToken);

        if (athlete is null)
        {
            throw new NotFoundException(nameof(Athlete), request.Id);
        }

        athlete.UpdatePhysicalData(
            request.Country,
            request.Height,
            request.Weight,
            request.ExperienceLevel);

        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
