using CoachPlatform.Domain.Entities;
using CoachPlatform.Domain.Enums;

namespace CoachPlatform.Domain.Interfaces;

/// <summary>
/// Repository interface for Plan entity.
/// </summary>
public interface IPlanRepository : IRepository<Plan>
{
    /// <summary>
    /// Gets all plans for a specific coach.
    /// </summary>
    Task<IReadOnlyList<Plan>> GetByCoachIdAsync(
        Guid coachId, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets active plans for a coach.
    /// </summary>
    Task<IReadOnlyList<Plan>> GetActiveByCoachIdAsync(
        Guid coachId, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets plans by type for a coach.
    /// </summary>
    Task<IReadOnlyList<Plan>> GetByTypeAsync(
        Guid coachId, 
        PlanType planType, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a plan with its subscriptions.
    /// </summary>
    Task<Plan?> GetWithSubscriptionsAsync(Guid id, CancellationToken cancellationToken = default);
}
