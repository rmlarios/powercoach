using CoachPlatform.Domain.Common;
using CoachPlatform.Domain.Enums;
using CoachPlatform.Domain.ValueObjects;

namespace CoachPlatform.Domain.Entities;

/// <summary>
/// Application entity - represents a person applying to the coach's program.
/// </summary>
public class Application : AuditableEntity, IAggregateRoot
{
    /// <summary>
    /// Reference to the coach this application is for.
    /// Required for multi-tenant support.
    /// </summary>
    public Guid CoachId { get; private set; }

    /// <summary>
    /// Applicant's full name.
    /// </summary>
    public PersonName ApplicantName { get; private set; } = null!;

    /// <summary>
    /// Applicant's email address.
    /// </summary>
    public Email Email { get; private set; } = null!;

    /// <summary>
    /// Applicant's phone number.
    /// </summary>
    public string? Phone { get; private set; }

    /// <summary>
    /// Applicant's age.
    /// </summary>
    public int? Age { get; private set; }

    /// <summary>
    /// Applicant's gender.
    /// </summary>
    public string? Gender { get; private set; }

    /// <summary>
    /// Applicant's country of residence.
    /// </summary>
    public string? Country { get; private set; }

    /// <summary>
    /// Description of applicant's training experience.
    /// </summary>
    public string? TrainingExperience { get; private set; }

    /// <summary>
    /// Current squat max (in kg).
    /// </summary>
    public decimal? CurrentSquat { get; private set; }

    /// <summary>
    /// Current bench press max (in kg).
    /// </summary>
    public decimal? CurrentBench { get; private set; }

    /// <summary>
    /// Current deadlift max (in kg).
    /// </summary>
    public decimal? CurrentDeadlift { get; private set; }

    /// <summary>
    /// Applicant's motivation for joining the program.
    /// </summary>
    public string? Motivation { get; private set; }

    /// <summary>
    /// Applicant's goals and objectives.
    /// </summary>
    public string? Goals { get; private set; }

    /// <summary>
    /// Additional message from the applicant.
    /// </summary>
    public string? Message { get; private set; }

    /// <summary>
    /// How the applicant heard about the coach.
    /// </summary>
    public string? ReferralSource { get; private set; }

    /// <summary>
    /// Current status of the application.
    /// </summary>
    public ApplicationStatus Status { get; private set; }

    /// <summary>
    /// Internal notes from the coach about this application.
    /// </summary>
    public string? CoachNotes { get; private set; }

    /// <summary>
    /// Date when the application was reviewed.
    /// </summary>
    public DateTime? ReviewedAt { get; private set; }

    /// <summary>
    /// Reason for rejection (if rejected).
    /// </summary>
    public string? RejectionReason { get; private set; }

    // Navigation properties
    public Coach Coach { get; private set; } = null!;

    // EF Core constructor
    private Application() { }

    private Application(
        Guid coachId,
        PersonName applicantName,
        Email email,
        string? phone,
        int? age,
        string? gender,
        string? country,
        string? trainingExperience,
        decimal? currentSquat,
        decimal? currentBench,
        decimal? currentDeadlift,
        string? motivation,
        string? goals,
        string? message,
        string? referralSource)
    {
        CoachId = coachId;
        ApplicantName = applicantName;
        Email = email;
        Phone = phone;
        Age = age;
        Gender = gender;
        Country = country;
        TrainingExperience = trainingExperience;
        CurrentSquat = currentSquat;
        CurrentBench = currentBench;
        CurrentDeadlift = currentDeadlift;
        Motivation = motivation;
        Goals = goals;
        Message = message;
        ReferralSource = referralSource;
        Status = ApplicationStatus.Pending;
    }

    /// <summary>
    /// Creates a new Application.
    /// </summary>
    public static Application Create(
        Guid coachId,
        string firstName,
        string lastName,
        string email,
        string? phone = null,
        int? age = null,
        string? gender = null,
        string? country = null,
        string? trainingExperience = null,
        decimal? currentSquat = null,
        decimal? currentBench = null,
        decimal? currentDeadlift = null,
        string? motivation = null,
        string? goals = null,
        string? message = null,
        string? referralSource = null)
    {
        if (coachId == Guid.Empty)
            throw new ArgumentException("Coach ID is required.", nameof(coachId));

        var name = PersonName.Create(firstName, lastName);
        var emailVo = Email.Create(email);

        return new Application(
            coachId, name, emailVo, phone, age, gender, country,
            trainingExperience, currentSquat, currentBench, currentDeadlift,
            motivation, goals, message, referralSource);
    }

    /// <summary>
    /// Marks the application as under review.
    /// </summary>
    public void StartReview()
    {
        if (Status != ApplicationStatus.Pending)
            throw new InvalidOperationException("Can only start review on pending applications.");

        Status = ApplicationStatus.UnderReview;
        SetUpdatedAt();
    }

    /// <summary>
    /// Accepts the application.
    /// </summary>
    public void Accept(string? notes = null)
    {
        if (Status != ApplicationStatus.Pending && Status != ApplicationStatus.UnderReview)
            throw new InvalidOperationException("Can only accept pending or under review applications.");

        Status = ApplicationStatus.Accepted;
        CoachNotes = notes;
        ReviewedAt = DateTime.UtcNow;
        SetUpdatedAt();
    }

    /// <summary>
    /// Rejects the application.
    /// </summary>
    public void Reject(string? reason = null, string? notes = null)
    {
        if (Status != ApplicationStatus.Pending && Status != ApplicationStatus.UnderReview)
            throw new InvalidOperationException("Can only reject pending or under review applications.");

        Status = ApplicationStatus.Rejected;
        RejectionReason = reason;
        CoachNotes = notes;
        ReviewedAt = DateTime.UtcNow;
        SetUpdatedAt();
    }

    /// <summary>
    /// Allows the applicant to withdraw their application.
    /// </summary>
    public void Withdraw()
    {
        if (Status == ApplicationStatus.Accepted || Status == ApplicationStatus.Rejected)
            throw new InvalidOperationException("Cannot withdraw an already processed application.");

        Status = ApplicationStatus.Withdrawn;
        SetUpdatedAt();
    }

    /// <summary>
    /// Updates coach notes for the application.
    /// </summary>
    public void UpdateCoachNotes(string? notes)
    {
        CoachNotes = notes;
        SetUpdatedAt();
    }
}
