using CoachPlatform.Domain.Entities;
using CoachPlatform.Domain.Enums;
using CoachPlatform.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CoachPlatform.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository implementation for Plan entity.
/// </summary>
public class PlanRepository : BaseRepository<Plan>, IPlanRepository
{
    public PlanRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<Plan>> GetByCoachIdAsync(
        Guid coachId,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(p => p.CoachId == coachId)
            .OrderBy(p => p.DisplayOrder)
            .ThenBy(p => p.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Plan>> GetActiveByCoachIdAsync(
        Guid coachId,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(p => p.CoachId == coachId && p.IsActive)
            .OrderBy(p => p.DisplayOrder)
            .ThenBy(p => p.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Plan>> GetByTypeAsync(
        Guid coachId,
        PlanType planType,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(p => p.CoachId == coachId && p.PlanType == planType)
            .OrderBy(p => p.DisplayOrder)
            .ThenBy(p => p.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<Plan?> GetWithSubscriptionsAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(p => p.Subscriptions)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }
}
