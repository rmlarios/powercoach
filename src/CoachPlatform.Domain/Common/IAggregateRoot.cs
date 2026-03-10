namespace CoachPlatform.Domain.Common;

/// <summary>
/// Marker interface for aggregate roots.
/// Aggregate roots are the entry points for accessing a cluster of domain objects.
/// Only aggregate roots should have repositories.
/// </summary>
public interface IAggregateRoot
{
}
