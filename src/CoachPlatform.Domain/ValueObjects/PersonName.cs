using CoachPlatform.Domain.Common;

namespace CoachPlatform.Domain.ValueObjects;

/// <summary>
/// Value object representing a person's name (first name and last name).
/// </summary>
public sealed class PersonName : ValueObject
{
    /// <summary>
    /// First name.
    /// </summary>
    public string FirstName { get; }

    /// <summary>
    /// Last name.
    /// </summary>
    public string LastName { get; }

    /// <summary>
    /// Full name (FirstName LastName).
    /// </summary>
    public string FullName => $"{FirstName} {LastName}";

    private PersonName(string firstName, string lastName)
    {
        FirstName = firstName;
        LastName = lastName;
    }

    /// <summary>
    /// Creates a new PersonName value object.
    /// </summary>
    /// <param name="firstName">The first name.</param>
    /// <param name="lastName">The last name.</param>
    /// <returns>A valid PersonName value object.</returns>
    /// <exception cref="ArgumentException">Thrown when validation fails.</exception>
    public static PersonName Create(string firstName, string lastName)
    {
        if (string.IsNullOrWhiteSpace(firstName))
            throw new ArgumentException("First name cannot be empty.", nameof(firstName));

        if (string.IsNullOrWhiteSpace(lastName))
            throw new ArgumentException("Last name cannot be empty.", nameof(lastName));

        firstName = NormalizeName(firstName);
        lastName = NormalizeName(lastName);

        if (firstName.Length > 100)
            throw new ArgumentException("First name cannot exceed 100 characters.", nameof(firstName));

        if (lastName.Length > 100)
            throw new ArgumentException("Last name cannot exceed 100 characters.", nameof(lastName));

        return new PersonName(firstName, lastName);
    }

    /// <summary>
    /// Creates a PersonName from a full name string (splits by first space).
    /// </summary>
    /// <param name="fullName">Full name string.</param>
    /// <returns>A valid PersonName value object.</returns>
    public static PersonName FromFullName(string fullName)
    {
        if (string.IsNullOrWhiteSpace(fullName))
            throw new ArgumentException("Full name cannot be empty.", nameof(fullName));

        var parts = fullName.Trim().Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
        
        if (parts.Length < 2)
            throw new ArgumentException("Full name must contain at least first and last name.", nameof(fullName));

        return Create(parts[0], parts[1]);
    }

    /// <summary>
    /// Tries to create a PersonName without throwing exceptions.
    /// </summary>
    public static bool TryCreate(string firstName, string lastName, out PersonName? result)
    {
        try
        {
            result = Create(firstName, lastName);
            return true;
        }
        catch
        {
            result = null;
            return false;
        }
    }

    private static string NormalizeName(string name)
    {
        name = name.Trim();
        
        if (string.IsNullOrEmpty(name))
            return name;

        // Capitalize first letter of each word
        return string.Join(' ', name.Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Select(word => char.ToUpperInvariant(word[0]) + word[1..].ToLowerInvariant()));
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return FirstName;
        yield return LastName;
    }

    public override string ToString() => FullName;

    public static implicit operator string(PersonName name) => name.FullName;
}
