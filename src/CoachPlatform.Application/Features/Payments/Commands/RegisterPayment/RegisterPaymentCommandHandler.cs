using CoachPlatform.Application.Shared.Exceptions;
using CoachPlatform.Application.Shared.Interfaces;
using CoachPlatform.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CoachPlatform.Application.Features.Payments.Commands.RegisterPayment;

/// <summary>
/// Handler for RegisterPaymentCommand.
/// </summary>
public class RegisterPaymentCommandHandler : IRequestHandler<RegisterPaymentCommand, Guid>
{
    private readonly IApplicationDbContext _context;

    public RegisterPaymentCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(RegisterPaymentCommand request, CancellationToken cancellationToken)
    {
        // Verify athlete exists
        var athleteExists = await _context.Athletes
            .AnyAsync(a => a.Id == request.AthleteId, cancellationToken);

        if (!athleteExists)
        {
            throw new NotFoundException(nameof(Athlete), request.AthleteId);
        }

        // Verify subscription exists and belongs to the athlete
        var subscription = await _context.Subscriptions
            .FirstOrDefaultAsync(s => s.Id == request.SubscriptionId, cancellationToken);

        if (subscription is null)
        {
            throw new NotFoundException(nameof(Subscription), request.SubscriptionId);
        }

        if (subscription.AthleteId != request.AthleteId)
        {
            throw new InvalidOperationException("Subscription does not belong to the specified athlete.");
        }

        // Create the payment
        var payment = Payment.Create(
            subscriptionId: request.SubscriptionId,
            amount: request.Amount,
            currency: request.Currency,
            paymentMethod: request.PaymentMethod);

        // Update notes if provided
        if (!string.IsNullOrWhiteSpace(request.Notes))
        {
            payment.UpdateNotes(request.Notes);
        }

        // Mark payment as completed (manual registration means payment is done)
        payment.Complete();

        _context.Payments.Add(payment);
        await _context.SaveChangesAsync(cancellationToken);

        return payment.Id;
    }
}
