using CoachPlatform.Domain.Common;
using CoachPlatform.Domain.Enums;
using CoachPlatform.Domain.ValueObjects;

namespace CoachPlatform.Domain.Entities;

/// <summary>
/// Athlete entity - represents an accepted athlete in the coach's program.
/// </summary>
public class Athlete : AuditableEntity, IAggregateRoot
{
    /// <summary>
    /// Reference to the coach this athlete belongs to.
    /// Required for multi-tenant support.
    /// </summary>
    public Guid CoachId { get; private set; }

    /// <summary>
    /// Athlete's full name.
    /// </summary>
    public PersonName Name { get; private set; } = null!;

    /// <summary>
    /// Athlete's email address.
    /// </summary>
    public Email Email { get; private set; } = null!;

    /// <summary>
    /// Athlete's phone number.
    /// </summary>
    public string? Phone { get; private set; }

    /// <summary>
    /// Athlete's goals and objectives.
    /// </summary>
    public string? Goals { get; private set; }

    /// <summary>
    /// Additional notes about the athlete.
    /// </summary>
    public string? Notes { get; private set; }

    /// <summary>
    /// Athlete's country.
    /// </summary>
    public string? Country { get; private set; }

    /// <summary>
    /// Athlete's gender.
    /// </summary>
    public string? Gender { get; private set; }

    /// <summary>
    /// Athlete's date of birth.
    /// </summary>
    public DateTime? DateOfBirth { get; private set; }

    /// <summary>
    /// Athlete's height in centimeters.
    /// </summary>
    public decimal? Height { get; private set; }

    /// <summary>
    /// Athlete's weight in kilograms.
    /// </summary>
    public decimal? Weight { get; private set; }

    /// <summary>
    /// Athlete's experience level in training.
    /// </summary>
    public string? ExperienceLevel { get; private set; }

    /// <summary>
    /// Date when the athlete started with the coach.
    /// </summary>
    public DateTime StartDate { get; private set; }

    /// <summary>
    /// Date when the athlete left the program (if applicable).
    /// </summary>
    public DateTime? EndDate { get; private set; }

    /// <summary>
    /// Current status of the athlete.
    /// </summary>
    public AthleteStatus Status { get; private set; }

    /// <summary>
    /// URL to athlete's profile picture.
    /// </summary>
    public string? ProfilePictureUrl { get; private set; }

    /// <summary>
    /// Reference to the original application (if created from an application).
    /// </summary>
    public Guid? ApplicationId { get; private set; }

    // Navigation properties
    public Coach Coach { get; private set; } = null!;
    public Application? Application { get; private set; }

    private readonly List<Subscription> _subscriptions = [];
    public IReadOnlyCollection<Subscription> Subscriptions => _subscriptions.AsReadOnly();

    private readonly List<CheckIn> _checkIns = [];
    public IReadOnlyCollection<CheckIn> CheckIns => _checkIns.AsReadOnly();

    private readonly List<TrainingCycle> _trainingCycles = [];
    public IReadOnlyCollection<TrainingCycle> TrainingCycles => _trainingCycles.AsReadOnly();

    private readonly List<WorkoutLog> _workoutLogs = [];
    public IReadOnlyCollection<WorkoutLog> WorkoutLogs => _workoutLogs.AsReadOnly();

    // EF Core constructor
    private Athlete() { }

    private Athlete(
        Guid coachId,
        PersonName name,
        Email email,
        string? phone,
        string? goals,
        string? country,
        string? gender,
        DateTime? dateOfBirth,
        decimal? height,
        decimal? weight,
        string? experienceLevel,
        Guid? applicationId = null)
    {
        CoachId = coachId;
        Name = name;
        Email = email;
        Phone = phone;
        Goals = goals;
        Country = country;
        Gender = gender;
        DateOfBirth = dateOfBirth;
        Height = height;
        Weight = weight;
        ExperienceLevel = experienceLevel;
        StartDate = DateTime.UtcNow;
        Status = AthleteStatus.Active;
        ApplicationId = applicationId;
    }

    /// <summary>
    /// Creates a new Athlete.
    /// </summary>
    public static Athlete Create(
        Guid coachId,
        string firstName,
        string lastName,
        string email,
        string? phone = null,
        string? goals = null,
        string? country = null,
        string? gender = null,
        DateTime? dateOfBirth = null,
        decimal? height = null,
        decimal? weight = null,
        string? experienceLevel = null,
        Guid? applicationId = null)
    {
        if (coachId == Guid.Empty)
            throw new ArgumentException("Coach ID is required.", nameof(coachId));

        var name = PersonName.Create(firstName, lastName);
        var emailVo = Email.Create(email);

        return new Athlete(coachId, name, emailVo, phone, goals, country, gender, dateOfBirth, height, weight, experienceLevel, applicationId);
    }

    /// <summary>
    /// Creates an Athlete from an accepted Application.
    /// </summary>
    public static Athlete CreateFromApplication(Application application)
    {
        if (application.Status != ApplicationStatus.Accepted)
            throw new InvalidOperationException("Can only create athlete from accepted applications.");

        return Create(
            application.CoachId,
            application.ApplicantName.FirstName,
            application.ApplicantName.LastName,
            application.Email.Value,
            application.Phone,
            application.Goals,
            application.Country,
            application.Gender,
            dateOfBirth: null,
            height: null,
            weight: null,
            application.TrainingExperience,
            application.Id);
    }

    /// <summary>
    /// Updates the athlete's profile information.
    /// </summary>
    public void UpdateProfile(string firstName, string lastName, string? phone, string? goals, string? notes)
    {
        Name = PersonName.Create(firstName, lastName);
        Phone = phone;
        Goals = goals;
        Notes = notes;
        SetUpdatedAt();
    }

    /// <summary>
    /// Updates the athlete's physical and training data.
    /// </summary>
    public void UpdatePhysicalData(string? country, decimal? height, decimal? weight, string? experienceLevel)
    {
        Country = country;
        Height = height;
        Weight = weight;
        ExperienceLevel = experienceLevel;
        SetUpdatedAt();
    }

    /// <summary>
    /// Updates the athlete's email.
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
    /// Puts the athlete on hold.
    /// </summary>
    public void PutOnHold()
    {
        Status = AthleteStatus.OnHold;
        SetUpdatedAt();
    }

    /// <summary>
    /// Reactivates the athlete.
    /// </summary>
    public void Activate()
    {
        Status = AthleteStatus.Active;
        SetUpdatedAt();
    }

    /// <summary>
    /// Marks the athlete as inactive (left the program).
    /// </summary>
    public void Deactivate()
    {
        Status = AthleteStatus.Inactive;
        EndDate = DateTime.UtcNow;
        SetUpdatedAt();
    }

    /// <summary>
    /// Suspends the athlete from the program.
    /// </summary>
    public void Suspend()
    {
        Status = AthleteStatus.Suspended;
        SetUpdatedAt();
    }

    /// <summary>
    /// Marks the athlete as graduated.
    /// </summary>
    public void Graduate()
    {
        Status = AthleteStatus.Graduated;
        EndDate = DateTime.UtcNow;
        SetUpdatedAt();
    }
}
