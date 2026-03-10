using CoachPlatform.Application.Shared.DTOs;
using MediatR;

namespace CoachPlatform.Application.Features.Payments.Queries.GetPayments;

/// <summary>
/// Query to get all payments with optional filtering.
/// </summary>
public record GetPaymentsQuery : IRequest<IReadOnlyList<PaymentListItemDto>>
{
    public Guid? AthleteId { get; init; }
    public DateTime? FromDate { get; init; }
    public DateTime? ToDate { get; init; }
    public string? PaymentMethod { get; init; }
}
