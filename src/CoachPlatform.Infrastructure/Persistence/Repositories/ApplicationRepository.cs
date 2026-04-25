using CoachPlatform.Domain.Enums;
using CoachPlatform.Domain.Interfaces;
using CoachPlatform.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace CoachPlatform.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository implementation for Application entity.
/// </summary>
public class ApplicationRepository : BaseRepository<Domain.Entities.Application>, IApplicationRepository
{
    public ApplicationRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<Domain.Entities.Application>> GetByCoachIdAsync(
        Guid coachId,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(a => a.CoachId == coachId)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Domain.Entities.Application>> GetByCoachIdAsync(
        Guid coachId,
        ApplicationStatus? status,
        CancellationToken cancellationToken = default)
    {
        var query = DbSet.Where(a => a.CoachId == coachId);

        if (status.HasValue)
            query = query.Where(a => a.Status == status.Value);

        return await query
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Domain.Entities.Application>> GetPendingByCoachIdAsync(
        Guid coachId,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(a => a.CoachId == coachId && a.Status == ApplicationStatus.Pending)
            .OrderBy(a => a.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<Domain.Entities.Application?> GetByEmailAsync(
        Guid coachId,
        string email,
        CancellationToken cancellationToken = default)
    {
        var normalizedEmail = email.ToLowerInvariant().Trim();
        return await DbSet
            .FirstOrDefaultAsync(a => a.CoachId == coachId && a.Email.Value == normalizedEmail, cancellationToken);
    }

    public async Task<bool> EmailExistsAsync(
        Guid coachId,
        string email,
        CancellationToken cancellationToken = default)
    {
        var normalizedEmail = email.ToLowerInvariant().Trim();
        return await DbSet.AnyAsync(
            a => a.CoachId == coachId && a.Email.Value == normalizedEmail,
            cancellationToken);
    }

    public async Task<Dictionary<ApplicationStatus, int>> GetCountByStatusAsync(
        Guid coachId,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(a => a.CoachId == coachId)
            .GroupBy(a => a.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Status, x => x.Count, cancellationToken);
    }
}
