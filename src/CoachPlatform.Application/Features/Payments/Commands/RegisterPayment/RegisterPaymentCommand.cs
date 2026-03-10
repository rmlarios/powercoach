using CoachPlatform.Application.Shared.Interfaces;
using MediatR;

namespace CoachPlatform.Application.Features.Payments.Commands.RegisterPayment;

/// <summary>
/// Command to register a new Payment.
/// </summary>
public record RegisterPaymentCommand : IRequest<Guid>, IAthleteOwnedRequest
{
    public Guid AthleteId { get; init; }
    public Guid SubscriptionId { get; init; }
    public decimal Amount { get; init; }
    public string Currency { get; init; } = "USD";
    public string? PaymentMethod { get; init; }
    public DateTime PaymentDate { get; init; }
    public string? Notes { get; init; }
}
