using CoachPlatform.Application.Shared.Exceptions;
using CoachPlatform.Application.Shared.Interfaces;
using CoachPlatform.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CoachPlatform.Application.Features.Subscriptions.Commands.CreateSubscription;

/// <summary>
/// Handler for CreateSubscriptionCommand.
/// </summary>
public class CreateSubscriptionCommandHandler : IRequestHandler<CreateSubscriptionCommand, Guid>
{
    private readonly IApplicationDbContext _context;

    public CreateSubscriptionCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(CreateSubscriptionCommand request, CancellationToken cancellationToken)
    {
        // Verify athlete exists
        var athleteExists = await _context.Athletes
            .AnyAsync(a => a.Id == request.AthleteId, cancellationToken);

        if (!athleteExists)
        {
            throw new NotFoundException(nameof(Athlete), request.AthleteId);
        }

        // Get plan with all details
        var plan = await _context.Plans
            .FirstOrDefaultAsync(p => p.Id == request.PlanId, cancellationToken);

        if (plan is null)
        {
            throw new NotFoundException(nameof(Plan), request.PlanId);
        }

        if (!plan.IsActive)
        {
            throw new InvalidOperationException("Cannot create subscription for an inactive plan.");
        }

        // Create the subscription using the factory method
        var subscription = Subscription.Create(
            athleteId: request.AthleteId,
            plan: plan,
            startDate: request.StartDate);

        // Activate the subscription immediately (Status = Active)
        subscription.Activate();

        _context.Subscriptions.Add(subscription);
        await _context.SaveChangesAsync(cancellationToken);

        return subscription.Id;
    }
}
