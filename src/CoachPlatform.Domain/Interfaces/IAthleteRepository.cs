using CoachPlatform.Domain.Entities;
using CoachPlatform.Domain.Enums;

namespace CoachPlatform.Domain.Interfaces;

/// <summary>
/// Repository interface for Athlete entity.
/// </summary>
public interface IAthleteRepository : IRepository<Athlete>
{
    /// <summary>
    /// Gets all athletes for a specific coach.
    /// </summary>
    Task<IReadOnlyList<Athlete>> GetByCoachIdAsync(
        Guid coachId, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets athletes by coach with optional status filter.
    /// </summary>
    Task<IReadOnlyList<Athlete>> GetByCoachIdAsync(
        Guid coachId, 
        AthleteStatus? status, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets an athlete by email within a coach's program.
    /// </summary>
    Task<Athlete?> GetByEmailAsync(
        Guid coachId, 
        string email, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets an athlete with all related data (subscriptions, check-ins).
    /// </summary>
    Task<Athlete?> GetWithDetailsAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if an email is already registered for a coach.
    /// </summary>
    Task<bool> EmailExistsAsync(
        Guid coachId, 
        string email, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets athletes with active subscriptions.
    /// </summary>
    Task<IReadOnlyList<Athlete>> GetActiveWithSubscriptionsAsync(
        Guid coachId, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Searches athletes by name or email.
    /// </summary>
    Task<IReadOnlyList<Athlete>> SearchAsync(
        Guid coachId, 
        string searchTerm, 
        CancellationToken cancellationToken = default);
}
