namespace CoachPlatform.Application.Shared.Exceptions;

/// <summary>
/// Exception thrown when an entity is not found.
/// </summary>
public class NotFoundException : Exception
{
    public string EntityName { get; }
    public object Key { get; }

    public NotFoundException()
        : base("Entity was not found.")
    {
        EntityName = string.Empty;
        Key = string.Empty;
    }

    public NotFoundException(string message)
        : base(message)
    {
        EntityName = string.Empty;
        Key = string.Empty;
    }

    public NotFoundException(string entityName, object key)
        : base($"Entity \"{entityName}\" ({key}) was not found.")
    {
        EntityName = entityName;
        Key = key;
    }

    public NotFoundException(string message, Exception innerException)
        : base(message, innerException)
    {
        EntityName = string.Empty;
        Key = string.Empty;
    }
}
