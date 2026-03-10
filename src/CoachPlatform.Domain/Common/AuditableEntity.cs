namespace CoachPlatform.Domain.Common;

/// <summary>
/// Entity with audit fields (CreatedAt, UpdatedAt).
/// All auditable entities should inherit from this class.
/// </summary>
public abstract class AuditableEntity : BaseEntity
{
    /// <summary>
    /// Date and time when the entity was created (UTC).
    /// </summary>
    public DateTime CreatedAt { get; protected set; }

    /// <summary>
    /// Date and time when the entity was last updated (UTC).
    /// </summary>
    public DateTime UpdatedAt { get; protected set; }

    protected AuditableEntity() : base()
    {
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    protected AuditableEntity(Guid id) : base(id)
    {
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Updates the UpdatedAt timestamp to current UTC time.
    /// Called automatically by the DbContext on SaveChanges.
    /// </summary>
    public void SetUpdatedAt()
    {
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Sets the audit timestamps. Used for seeding or data migration.
    /// </summary>
    public void SetAuditTimestamps(DateTime createdAt, DateTime updatedAt)
    {
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
    }
}
