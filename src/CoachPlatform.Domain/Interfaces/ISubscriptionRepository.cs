using CoachPlatform.Domain.Entities;
using CoachPlatform.Domain.Enums;

namespace CoachPlatform.Domain.Interfaces;

/// <summary>
/// Repository interface for Subscription entity.
/// </summary>
public interface ISubscriptionRepository : IRepository<Subscription>
{
    /// <summary>
    /// Gets all subscriptions for an athlete.
    /// </summary>
    Task<IReadOnlyList<Subscription>> GetByAthleteIdAsync(
        Guid athleteId, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets active subscription for an athlete.
    /// </summary>
    Task<Subscription?> GetActiveByAthleteIdAsync(
        Guid athleteId, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets subscriptions for a plan.
    /// </summary>
    Task<IReadOnlyList<Subscription>> GetByPlanIdAsync(
        Guid planId, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets subscriptions by status.
    /// </summary>
    Task<IReadOnlyList<Subscription>> GetByStatusAsync(
        SubscriptionStatus status, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets subscriptions expiring within a number of days.
    /// </summary>
    Task<IReadOnlyList<Subscription>> GetExpiringAsync(
        int daysUntilExpiration, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets expired subscriptions that need to be marked as expired.
    /// </summary>
    Task<IReadOnlyList<Subscription>> GetExpiredActiveAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a subscription with its payments.
    /// </summary>
    Task<Subscription?> GetWithPaymentsAsync(Guid id, CancellationToken cancellationToken = default);
}
