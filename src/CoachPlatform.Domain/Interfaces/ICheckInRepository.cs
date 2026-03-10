using CoachPlatform.Domain.Entities;

namespace CoachPlatform.Domain.Interfaces;

/// <summary>
/// Repository interface for CheckIn entity.
/// </summary>
public interface ICheckInRepository : IRepository<CheckIn>
{
    /// <summary>
    /// Gets all check-ins for an athlete.
    /// </summary>
    Task<IReadOnlyList<CheckIn>> GetByAthleteIdAsync(
        Guid athleteId, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets check-ins for an athlete within a date range.
    /// </summary>
    Task<IReadOnlyList<CheckIn>> GetByAthleteIdAsync(
        Guid athleteId, 
        DateTime startDate, 
        DateTime endDate, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the latest check-in for an athlete.
    /// </summary>
    Task<CheckIn?> GetLatestByAthleteIdAsync(
        Guid athleteId, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets unreviewed check-ins for a coach.
    /// </summary>
    Task<IReadOnlyList<CheckIn>> GetUnreviewedByCoachIdAsync(
        Guid coachId, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets check-ins pending review for a coach.
    /// </summary>
    Task<int> GetPendingReviewCountAsync(
        Guid coachId, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets weight history for an athlete.
    /// </summary>
    Task<IReadOnlyList<(DateTime Date, decimal Weight)>> GetWeightHistoryAsync(
        Guid athleteId, 
        CancellationToken cancellationToken = default);
}
