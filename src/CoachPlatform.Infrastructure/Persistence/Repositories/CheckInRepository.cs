using CoachPlatform.Domain.Entities;
using CoachPlatform.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CoachPlatform.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository implementation for CheckIn entity.
/// </summary>
public class CheckInRepository : BaseRepository<CheckIn>, ICheckInRepository
{
    public CheckInRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<CheckIn>> GetByAthleteIdAsync(
        Guid athleteId,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(c => c.AthleteId == athleteId)
            .OrderByDescending(c => c.CheckInDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<CheckIn>> GetByAthleteIdAsync(
        Guid athleteId,
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(c => c.AthleteId == athleteId &&
                       c.CheckInDate >= startDate &&
                       c.CheckInDate <= endDate)
            .OrderByDescending(c => c.CheckInDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<CheckIn?> GetLatestByAthleteIdAsync(
        Guid athleteId,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(c => c.AthleteId == athleteId)
            .OrderByDescending(c => c.CheckInDate)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<CheckIn>> GetUnreviewedByCoachIdAsync(
        Guid coachId,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(c => c.Athlete)
            .Where(c => c.Athlete.CoachId == coachId &&
                       string.IsNullOrEmpty(c.CoachFeedback))
            .OrderBy(c => c.CheckInDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> GetPendingReviewCountAsync(
        Guid coachId,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(c => c.Athlete)
            .CountAsync(c => c.Athlete.CoachId == coachId &&
                            string.IsNullOrEmpty(c.CoachFeedback),
                       cancellationToken);
    }

    public async Task<IReadOnlyList<(DateTime Date, decimal Weight)>> GetWeightHistoryAsync(
        Guid athleteId,
        CancellationToken cancellationToken = default)
    {
        var checkIns = await DbSet
            .Where(c => c.AthleteId == athleteId && c.Weight.HasValue)
            .OrderBy(c => c.CheckInDate)
            .Select(c => new { c.CheckInDate, c.Weight })
            .ToListAsync(cancellationToken);

        return checkIns
            .Select(c => (c.CheckInDate, c.Weight!.Value))
            .ToList();
    }
}
