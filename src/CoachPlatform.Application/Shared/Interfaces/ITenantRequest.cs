namespace CoachPlatform.Application.Shared.Interfaces;

/// <summary>
/// Marker interface for requests that require tenant context.
/// Requests implementing this interface will have tenant validation applied automatically.
/// </summary>
public interface ITenantRequest
{
    /// <summary>
    /// The CoachId that this request belongs to.
    /// Used to validate that the current tenant has access to the resource.
    /// </summary>
    Guid CoachId { get; }
}

/// <summary>
/// Marker interface for requests that operate on athlete-owned resources.
/// These requests will be validated to ensure the athlete belongs to the current tenant.
/// </summary>
public interface IAthleteOwnedRequest
{
    /// <summary>
    /// The AthleteId that this request operates on.
    /// </summary>
    Guid AthleteId { get; }
}
