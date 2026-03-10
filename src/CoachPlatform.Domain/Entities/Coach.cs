using CoachPlatform.Domain.Common;
using CoachPlatform.Domain.ValueObjects;

namespace CoachPlatform.Domain.Entities;

/// <summary>
/// Coach entity - Aggregate Root.
/// Represents a coach who manages athletes, plans, and applications.
/// </summary>
public class Coach : AuditableEntity, IAggregateRoot
{
    /// <summary>
    /// Coach's full name.
    /// </summary>
    public PersonName Name { get; private set; } = null!;

    /// <summary>
    /// Coach's email address.
    /// </summary>
    public Email Email { get; private set; } = null!;

    /// <summary>
    /// Coach's biography or description.
    /// </summary>
    public string? Bio { get; private set; }

    /// <summary>
    /// Coach's phone number.
    /// </summary>
    public string? Phone { get; private set; }

    /// <summary>
    /// URL to coach's profile picture.
    /// </summary>
    public string? ProfilePictureUrl { get; private set; }

    /// <summary>
    /// Whether the coach is currently active.
    /// </summary>
    public bool IsActive { get; private set; }

    // Navigation properties
    private readonly List<Athlete> _athletes = [];
    public IReadOnlyCollection<Athlete> Athletes => _athletes.AsReadOnly();

    private readonly List<Plan> _plans = [];
    public IReadOnlyCollection<Plan> Plans => _plans.AsReadOnly();

    private readonly List<Application> _applications = [];
    public IReadOnlyCollection<Application> Applications => _applications.AsReadOnly();

    private readonly List<Exercise> _exercises = [];
    public IReadOnlyCollection<Exercise> Exercises => _exercises.AsReadOnly();

    // EF Core constructor
    private Coach() { }

    private Coach(PersonName name, Email email, string? bio = null)
    {
        Name = name;
        Email = email;
        Bio = bio;
        IsActive = true;
    }

    /// <summary>
    /// Creates a new Coach.
    /// </summary>
    public static Coach Create(string firstName, string lastName, string email, string? bio = null)
    {
        var name = PersonName.Create(firstName, lastName);
        var emailVo = Email.Create(email);
        
        return new Coach(name, emailVo, bio);
    }

    /// <summary>
    /// Updates the coach's profile information.
    /// </summary>
    public void UpdateProfile(string firstName, string lastName, string? bio, string? phone)
    {
        Name = PersonName.Create(firstName, lastName);
        Bio = bio;
        Phone = phone;
        SetUpdatedAt();
    }

    /// <summary>
    /// Updates the coach's email.
    /// </summary>
    public void UpdateEmail(string email)
    {
        Email = Email.Create(email);
        SetUpdatedAt();
    }

    /// <summary>
    /// Sets the profile picture URL.
    /// </summary>
    public void SetProfilePicture(string? url)
    {
        ProfilePictureUrl = url;
        SetUpdatedAt();
    }

    /// <summary>
    /// Activates the coach.
    /// </summary>
    public void Activate()
    {
        IsActive = true;
        SetUpdatedAt();
    }

    /// <summary>
    /// Deactivates the coach.
    /// </summary>
    public void Deactivate()
    {
        IsActive = false;
        SetUpdatedAt();
    }
}
