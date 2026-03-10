using CoachPlatform.Domain.Entities;
using CoachPlatform.Domain.Enums;
using CoachPlatform.Domain.Interfaces;
using CoachPlatform.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace CoachPlatform.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository implementation for Athlete entity.
/// </summary>
public class AthleteRepository : BaseRepository<Athlete>, IAthleteRepository
{
    public AthleteRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<Athlete>> GetByCoachIdAsync(
        Guid coachId,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(a => a.CoachId == coachId)
            .OrderBy(a => a.Name.LastName)
            .ThenBy(a => a.Name.FirstName)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Athlete>> GetByCoachIdAsync(
        Guid coachId,
        AthleteStatus? status,
        CancellationToken cancellationToken = default)
    {
        var query = DbSet.Where(a => a.CoachId == coachId);

        if (status.HasValue)
            query = query.Where(a => a.Status == status.Value);

        return await query
            .OrderBy(a => a.Name.LastName)
            .ThenBy(a => a.Name.FirstName)
            .ToListAsync(cancellationToken);
    }

    public async Task<Athlete?> GetByEmailAsync(
        Guid coachId,
        string email,
        CancellationToken cancellationToken = default)
    {
        var emailVo = Email.Create(email);
        return await DbSet
            .FirstOrDefaultAsync(a => a.CoachId == coachId && a.Email == emailVo, cancellationToken);
    }

    public async Task<Athlete?> GetWithDetailsAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(a => a.Subscriptions)
                .ThenInclude(s => s.Plan)
            .Include(a => a.CheckIns.OrderByDescending(c => c.CheckInDate).Take(10))
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
    }

    public async Task<bool> EmailExistsAsync(
        Guid coachId,
        string email,
        CancellationToken cancellationToken = default)
    {
        var emailVo = Email.Create(email);
        return await DbSet.AnyAsync(
            a => a.CoachId == coachId && a.Email == emailVo,
            cancellationToken);
    }

    public async Task<IReadOnlyList<Athlete>> GetActiveWithSubscriptionsAsync(
        Guid coachId,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(a => a.Subscriptions.Where(s => s.Status == SubscriptionStatus.Active))
                .ThenInclude(s => s.Plan)
            .Where(a => a.CoachId == coachId && a.Status == AthleteStatus.Active)
            .OrderBy(a => a.Name.LastName)
            .ThenBy(a => a.Name.FirstName)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Athlete>> SearchAsync(
        Guid coachId,
        string searchTerm,
        CancellationToken cancellationToken = default)
    {
        var searchLower = searchTerm.ToLower();
        return await DbSet
            .Where(a => a.CoachId == coachId &&
                       (a.Name.FirstName.ToLower().Contains(searchLower) ||
                        a.Name.LastName.ToLower().Contains(searchLower) ||
                        a.Email.Value.ToLower().Contains(searchLower)))
            .OrderBy(a => a.Name.LastName)
            .ThenBy(a => a.Name.FirstName)
            .ToListAsync(cancellationToken);
    }
}
