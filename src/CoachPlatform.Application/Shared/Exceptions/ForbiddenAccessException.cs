namespace CoachPlatform.Application.Shared.Exceptions;

/// <summary>
/// Exception thrown when a user attempts to access a resource they don't have permission for.
/// Results in HTTP 403 Forbidden response.
/// </summary>
public class ForbiddenAccessException : Exception
{
    public ForbiddenAccessException() 
        : base("Access to this resource is forbidden.")
    {
    }

    public ForbiddenAccessException(string message) 
        : base(message)
    {
    }

    public ForbiddenAccessException(string message, Exception innerException) 
        : base(message, innerException)
    {
    }
}
