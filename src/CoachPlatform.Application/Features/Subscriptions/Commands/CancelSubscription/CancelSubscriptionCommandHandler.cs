using CoachPlatform.Application.Shared.Exceptions;
using CoachPlatform.Application.Shared.Interfaces;
using CoachPlatform.Domain.Entities;
using CoachPlatform.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CoachPlatform.Application.Features.Subscriptions.Commands.CancelSubscription;

/// <summary>
/// Handler for CancelSubscriptionCommand.
/// </summary>
public class CancelSubscriptionCommandHandler : IRequestHandler<CancelSubscriptionCommand, Unit>
{
    private readonly IApplicationDbContext _context;

    public CancelSubscriptionCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Unit> Handle(CancelSubscriptionCommand request, CancellationToken cancellationToken)
    {
        var subscription = await _context.Subscriptions
            .FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken);

        if (subscription is null)
        {
            throw new NotFoundException(nameof(Subscription), request.Id);
        }

        if (subscription.Status == SubscriptionStatus.Cancelled)
        {
            throw new InvalidOperationException("Subscription is already cancelled.");
        }

        if (subscription.Status == SubscriptionStatus.Expired)
        {
            throw new InvalidOperationException("Cannot cancel an expired subscription.");
        }

        subscription.Cancel(request.Reason);

        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
