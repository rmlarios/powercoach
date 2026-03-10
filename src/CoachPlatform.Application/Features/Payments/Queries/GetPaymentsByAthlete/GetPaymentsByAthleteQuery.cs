using CoachPlatform.Application.Shared.DTOs;
using CoachPlatform.Application.Shared.Interfaces;
using MediatR;

namespace CoachPlatform.Application.Features.Payments.Queries.GetPaymentsByAthlete;

/// <summary>
/// Query to get all payments for a specific athlete.
/// </summary>
public record GetPaymentsByAthleteQuery : IRequest<IReadOnlyList<PaymentListItemDto>>, IAthleteOwnedRequest
{
    public Guid AthleteId { get; init; }
}
