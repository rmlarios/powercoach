namespace CoachPlatform.Application.Shared.Interfaces;

/// <summary>
/// Service to manage multi-tenancy context.
/// Each coach represents a tenant in the system.
/// </summary>
public interface ITenantService
{
    /// <summary>
    /// Gets the CoachId (tenant identifier) for the current request.
    /// </summary>
    Guid? CoachId { get; }

    /// <summary>
    /// Gets whether a tenant context is currently set.
    /// </summary>
    bool HasTenant { get; }

    /// <summary>
    /// Sets the tenant context for the current scope.
    /// Used primarily by middleware to set the context from the authenticated user.
    /// </summary>
    void SetTenant(Guid coachId);

    /// <summary>
    /// Clears the current tenant context.
    /// </summary>
    void ClearTenant();
}
