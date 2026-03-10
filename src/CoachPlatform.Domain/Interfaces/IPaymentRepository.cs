using CoachPlatform.Domain.Entities;
using CoachPlatform.Domain.Enums;

namespace CoachPlatform.Domain.Interfaces;

/// <summary>
/// Repository interface for Payment entity.
/// </summary>
public interface IPaymentRepository : IRepository<Payment>
{
    /// <summary>
    /// Gets all payments for a subscription.
    /// </summary>
    Task<IReadOnlyList<Payment>> GetBySubscriptionIdAsync(
        Guid subscriptionId, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets payments by status.
    /// </summary>
    Task<IReadOnlyList<Payment>> GetByStatusAsync(
        PaymentStatus status, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a payment by external reference.
    /// </summary>
    Task<Payment?> GetByExternalReferenceAsync(
        string externalReference, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a payment by transaction ID.
    /// </summary>
    Task<Payment?> GetByTransactionIdAsync(
        string transactionId, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets payments within a date range.
    /// </summary>
    Task<IReadOnlyList<Payment>> GetByDateRangeAsync(
        DateTime startDate, 
        DateTime endDate, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets total revenue for a coach within a date range.
    /// </summary>
    Task<decimal> GetTotalRevenueAsync(
        Guid coachId, 
        DateTime startDate, 
        DateTime endDate, 
        CancellationToken cancellationToken = default);
}
