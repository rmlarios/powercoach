using CoachPlatform.Domain.Entities;
using CoachPlatform.Domain.Enums;
using CoachPlatform.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CoachPlatform.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository implementation for Subscription entity.
/// </summary>
public class SubscriptionRepository : BaseRepository<Subscription>, ISubscriptionRepository
{
    public SubscriptionRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<Subscription>> GetByAthleteIdAsync(
        Guid athleteId,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(s => s.Plan)
            .Where(s => s.AthleteId == athleteId)
            .OrderByDescending(s => s.StartDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<Subscription?> GetActiveByAthleteIdAsync(
        Guid athleteId,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(s => s.Plan)
            .FirstOrDefaultAsync(s => s.AthleteId == athleteId &&
                                      s.Status == SubscriptionStatus.Active,
                                cancellationToken);
    }

    public async Task<IReadOnlyList<Subscription>> GetByPlanIdAsync(
        Guid planId,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(s => s.Athlete)
            .Where(s => s.PlanId == planId)
            .OrderByDescending(s => s.StartDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Subscription>> GetByStatusAsync(
        SubscriptionStatus status,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(s => s.Athlete)
            .Include(s => s.Plan)
            .Where(s => s.Status == status)
            .OrderByDescending(s => s.StartDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Subscription>> GetExpiringAsync(
        int daysUntilExpiration,
        CancellationToken cancellationToken = default)
    {
        var expirationDate = DateTime.UtcNow.AddDays(daysUntilExpiration);

        return await DbSet
            .Include(s => s.Athlete)
            .Include(s => s.Plan)
            .Where(s => s.Status == SubscriptionStatus.Active &&
                       s.EndDate <= expirationDate &&
                       s.EndDate >= DateTime.UtcNow)
            .OrderBy(s => s.EndDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Subscription>> GetExpiredActiveAsync(
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(s => s.Athlete)
            .Where(s => s.Status == SubscriptionStatus.Active &&
                       s.EndDate < DateTime.UtcNow)
            .ToListAsync(cancellationToken);
    }

    public async Task<Subscription?> GetWithPaymentsAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(s => s.Payments.OrderByDescending(p => p.PaymentDate))
            .Include(s => s.Plan)
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
    }
}
