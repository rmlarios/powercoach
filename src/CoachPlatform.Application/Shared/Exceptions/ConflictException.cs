namespace CoachPlatform.Application.Shared.Exceptions;

/// <summary>
/// Exception thrown when there is a conflict (e.g., duplicate email).
/// </summary>
public class ConflictException : Exception
{
    public string EntityName { get; }
    public string ConflictProperty { get; }

    public ConflictException()
        : base("A conflict occurred.")
    {
        EntityName = string.Empty;
        ConflictProperty = string.Empty;
    }

    public ConflictException(string message)
        : base(message)
    {
        EntityName = string.Empty;
        ConflictProperty = string.Empty;
    }

    public ConflictException(string entityName, string conflictProperty, string value)
        : base($"Entity \"{entityName}\" with {conflictProperty} \"{value}\" already exists.")
    {
        EntityName = entityName;
        ConflictProperty = conflictProperty;
    }

    public ConflictException(string message, Exception innerException)
        : base(message, innerException)
    {
        EntityName = string.Empty;
        ConflictProperty = string.Empty;
    }
}
