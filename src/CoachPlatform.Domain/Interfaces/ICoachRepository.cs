using CoachPlatform.Domain.Entities;

namespace CoachPlatform.Domain.Interfaces;

/// <summary>
/// Repository interface for Coach entity.
/// </summary>
public interface ICoachRepository : IRepository<Coach>
{
    /// <summary>
    /// Gets a coach by email.
    /// </summary>
    Task<Coach?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a coach with all related data (athletes, plans, applications).
    /// </summary>
    Task<Coach?> GetWithDetailsAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if an email is already registered.
    /// </summary>
    Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default);
}
