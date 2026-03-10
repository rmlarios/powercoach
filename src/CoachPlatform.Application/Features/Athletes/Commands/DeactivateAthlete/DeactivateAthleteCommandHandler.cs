using CoachPlatform.Application.Shared.Exceptions;
using CoachPlatform.Application.Shared.Interfaces;
using CoachPlatform.Domain.Entities;
using CoachPlatform.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CoachPlatform.Application.Features.Athletes.Commands.DeactivateAthlete;

/// <summary>
/// Handler for DeactivateAthleteCommand.
/// </summary>
public class DeactivateAthleteCommandHandler : IRequestHandler<DeactivateAthleteCommand, Unit>
{
    private readonly IApplicationDbContext _context;

    public DeactivateAthleteCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Unit> Handle(DeactivateAthleteCommand request, CancellationToken cancellationToken)
    {
        var athlete = await _context.Athletes
            .FirstOrDefaultAsync(a => a.Id == request.Id, cancellationToken);

        if (athlete is null)
        {
            throw new NotFoundException(nameof(Athlete), request.Id);
        }

        if (athlete.Status == AthleteStatus.Inactive)
        {
            throw new InvalidOperationException("Athlete is already inactive.");
        }

        athlete.Deactivate();

        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
