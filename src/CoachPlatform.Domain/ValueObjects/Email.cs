using System.Text.RegularExpressions;
using CoachPlatform.Domain.Common;

namespace CoachPlatform.Domain.ValueObjects;

/// <summary>
/// Value object representing a valid email address.
/// </summary>
public sealed partial class Email : ValueObject
{
    /// <summary>
    /// Email address value.
    /// </summary>
    public string Value { get; }

    private Email(string value)
    {
        Value = value.ToLowerInvariant().Trim();
    }

    /// <summary>
    /// Creates a new Email value object.
    /// </summary>
    /// <param name="email">The email address string.</param>
    /// <returns>A valid Email value object.</returns>
    /// <exception cref="ArgumentException">Thrown when the email is invalid.</exception>
    public static Email Create(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email cannot be empty.", nameof(email));

        email = email.Trim();

        if (email.Length > 256)
            throw new ArgumentException("Email cannot exceed 256 characters.", nameof(email));

        if (!EmailRegex().IsMatch(email))
            throw new ArgumentException("Email format is invalid.", nameof(email));

        return new Email(email);
    }

    /// <summary>
    /// Tries to create an Email value object without throwing exceptions.
    /// </summary>
    public static bool TryCreate(string email, out Email? result)
    {
        try
        {
            result = Create(email);
            return true;
        }
        catch
        {
            result = null;
            return false;
        }
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;

    public static implicit operator string(Email email) => email.Value;

    [GeneratedRegex(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$", RegexOptions.Compiled)]
    private static partial Regex EmailRegex();
}
