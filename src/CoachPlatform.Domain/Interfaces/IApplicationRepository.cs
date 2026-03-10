using CoachPlatform.Domain.Entities;
using CoachPlatform.Domain.Enums;

namespace CoachPlatform.Domain.Interfaces;

/// <summary>
/// Repository interface for Application entity.
/// </summary>
public interface IApplicationRepository : IRepository<Application>
{
    /// <summary>
    /// Gets all applications for a specific coach.
    /// </summary>
    Task<IReadOnlyList<Application>> GetByCoachIdAsync(
        Guid coachId, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets applications by coach with optional status filter.
    /// </summary>
    Task<IReadOnlyList<Application>> GetByCoachIdAsync(
        Guid coachId, 
        ApplicationStatus? status, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets pending applications for a coach.
    /// </summary>
    Task<IReadOnlyList<Application>> GetPendingByCoachIdAsync(
        Guid coachId, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets an application by email within a coach's applications.
    /// </summary>
    Task<Application?> GetByEmailAsync(
        Guid coachId, 
        string email, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if an email has already applied to a coach.
    /// </summary>
    Task<bool> EmailExistsAsync(
        Guid coachId, 
        string email, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the count of applications by status.
    /// </summary>
    Task<Dictionary<ApplicationStatus, int>> GetCountByStatusAsync(
        Guid coachId, 
        CancellationToken cancellationToken = default);
}
