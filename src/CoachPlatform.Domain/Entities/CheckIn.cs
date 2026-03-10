using CoachPlatform.Domain.Common;

namespace CoachPlatform.Domain.Entities;

/// <summary>
/// CheckIn entity - represents a periodic check-in/progress report from an athlete.
/// </summary>
public class CheckIn : AuditableEntity
{
    /// <summary>
    /// Reference to the athlete who submitted this check-in.
    /// </summary>
    public Guid AthleteId { get; private set; }

    /// <summary>
    /// Date of the check-in.
    /// </summary>
    public DateTime CheckInDate { get; private set; }

    /// <summary>
    /// Athlete's weight at check-in (in kg).
    /// </summary>
    public decimal? Weight { get; private set; }

    /// <summary>
    /// General notes/comments from the athlete.
    /// </summary>
    public string? Notes { get; private set; }

    /// <summary>
    /// How the athlete is feeling (1-10 scale).
    /// </summary>
    public int? EnergyLevel { get; private set; }

    /// <summary>
    /// Sleep quality (1-10 scale).
    /// </summary>
    public int? SleepQuality { get; private set; }

    /// <summary>
    /// Average hours of sleep.
    /// </summary>
    public decimal? SleepHours { get; private set; }

    /// <summary>
    /// Stress level (1-10 scale).
    /// </summary>
    public int? StressLevel { get; private set; }

    /// <summary>
    /// Adherence to nutrition plan (percentage 0-100).
    /// </summary>
    public int? NutritionAdherence { get; private set; }

    /// <summary>
    /// Adherence to training plan (percentage 0-100).
    /// </summary>
    public int? TrainingAdherence { get; private set; }

    /// <summary>
    /// URLs to progress photos.
    /// </summary>
    public List<string> PhotoUrls { get; private set; } = [];

    /// <summary>
    /// Feedback from the coach on this check-in.
    /// </summary>
    public string? CoachFeedback { get; private set; }

    /// <summary>
    /// Date when the coach provided feedback.
    /// </summary>
    public DateTime? FeedbackDate { get; private set; }

    /// <summary>
    /// Whether the coach has reviewed this check-in.
    /// </summary>
    public bool IsReviewed { get; private set; }

    // Navigation property
    public Athlete Athlete { get; private set; } = null!;

    // EF Core constructor
    private CheckIn() { }

    private CheckIn(Guid athleteId, DateTime checkInDate)
    {
        AthleteId = athleteId;
        CheckInDate = checkInDate;
        IsReviewed = false;
    }

    /// <summary>
    /// Creates a new CheckIn.
    /// </summary>
    public static CheckIn Create(Guid athleteId, DateTime? checkInDate = null)
    {
        if (athleteId == Guid.Empty)
            throw new ArgumentException("Athlete ID is required.", nameof(athleteId));

        return new CheckIn(athleteId, checkInDate ?? DateTime.UtcNow);
    }

    /// <summary>
    /// Sets the weight measurement.
    /// </summary>
    public void SetWeight(decimal weight)
    {
        if (weight <= 0)
            throw new ArgumentException("Weight must be greater than 0.", nameof(weight));

        Weight = Math.Round(weight, 2);
        SetUpdatedAt();
    }

    /// <summary>
    /// Sets the energy level (1-10).
    /// </summary>
    public void SetEnergyLevel(int level)
    {
        ValidateScale(level, nameof(level));
        EnergyLevel = level;
        SetUpdatedAt();
    }

    /// <summary>
    /// Sets sleep information.
    /// </summary>
    public void SetSleepInfo(int quality, decimal hours)
    {
        ValidateScale(quality, nameof(quality));
        
        if (hours < 0 || hours > 24)
            throw new ArgumentException("Sleep hours must be between 0 and 24.", nameof(hours));

        SleepQuality = quality;
        SleepHours = Math.Round(hours, 1);
        SetUpdatedAt();
    }

    /// <summary>
    /// Sets the stress level (1-10).
    /// </summary>
    public void SetStressLevel(int level)
    {
        ValidateScale(level, nameof(level));
        StressLevel = level;
        SetUpdatedAt();
    }

    /// <summary>
    /// Sets nutrition plan adherence (0-100%).
    /// </summary>
    public void SetNutritionAdherence(int percentage)
    {
        ValidatePercentage(percentage, nameof(percentage));
        NutritionAdherence = percentage;
        SetUpdatedAt();
    }

    /// <summary>
    /// Sets training plan adherence (0-100%).
    /// </summary>
    public void SetTrainingAdherence(int percentage)
    {
        ValidatePercentage(percentage, nameof(percentage));
        TrainingAdherence = percentage;
        SetUpdatedAt();
    }

    /// <summary>
    /// Sets the athlete's notes.
    /// </summary>
    public void SetNotes(string? notes)
    {
        Notes = notes;
        SetUpdatedAt();
    }

    /// <summary>
    /// Sets the progress photos.
    /// </summary>
    public void SetPhotos(IEnumerable<string> photoUrls)
    {
        PhotoUrls = photoUrls.Where(url => !string.IsNullOrWhiteSpace(url)).ToList();
        SetUpdatedAt();
    }

    /// <summary>
    /// Adds a progress photo.
    /// </summary>
    public void AddPhoto(string photoUrl)
    {
        if (!string.IsNullOrWhiteSpace(photoUrl) && !PhotoUrls.Contains(photoUrl))
        {
            PhotoUrls.Add(photoUrl);
            SetUpdatedAt();
        }
    }

    /// <summary>
    /// Removes a progress photo.
    /// </summary>
    public void RemovePhoto(string photoUrl)
    {
        if (PhotoUrls.Remove(photoUrl))
        {
            SetUpdatedAt();
        }
    }

    /// <summary>
    /// Coach adds feedback to the check-in.
    /// </summary>
    public void AddCoachFeedback(string feedback)
    {
        if (string.IsNullOrWhiteSpace(feedback))
            throw new ArgumentException("Feedback cannot be empty.", nameof(feedback));

        CoachFeedback = feedback;
        FeedbackDate = DateTime.UtcNow;
        IsReviewed = true;
        SetUpdatedAt();
    }

    /// <summary>
    /// Marks the check-in as reviewed without adding feedback.
    /// </summary>
    public void MarkAsReviewed()
    {
        IsReviewed = true;
        SetUpdatedAt();
    }

    private static void ValidateScale(int value, string paramName)
    {
        if (value < 1 || value > 10)
            throw new ArgumentException($"{paramName} must be between 1 and 10.", paramName);
    }

    private static void ValidatePercentage(int value, string paramName)
    {
        if (value < 0 || value > 100)
            throw new ArgumentException($"{paramName} must be between 0 and 100.", paramName);
    }
}
