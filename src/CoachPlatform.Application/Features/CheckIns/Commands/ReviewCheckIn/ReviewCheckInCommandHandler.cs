using CoachPlatform.Application.Shared.Exceptions;
using CoachPlatform.Application.Shared.Interfaces;
using CoachPlatform.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CoachPlatform.Application.Features.CheckIns.Commands.ReviewCheckIn;

/// <summary>
/// Handler for ReviewCheckInCommand.
/// </summary>
public class ReviewCheckInCommandHandler : IRequestHandler<ReviewCheckInCommand, Unit>
{
    private readonly IApplicationDbContext _context;

    public ReviewCheckInCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Unit> Handle(ReviewCheckInCommand request, CancellationToken cancellationToken)
    {
        var checkIn = await _context.CheckIns
            .Include(c => c.Athlete)
            .FirstOrDefaultAsync(c => c.Id == request.CheckInId, cancellationToken);

        if (checkIn == null)
        {
            throw new NotFoundException(nameof(CheckIn), request.CheckInId);
        }

        // Verify the coach is assigned to this athlete
        if (checkIn.Athlete.CoachId != request.CoachId)
        {
            throw new UnauthorizedAccessException("You are not authorized to review this check-in.");
        }

        checkIn.AddCoachFeedback(request.Feedback);

        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
