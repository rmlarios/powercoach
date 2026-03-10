using CoachPlatform.Domain.Common;

namespace CoachPlatform.Domain.Entities;

/// <summary>
/// TrainingCycle entity - represents a training program assigned by a coach to an athlete.
/// </summary>
public class TrainingCycle : AuditableEntity
{
    /// <summary>
    /// Reference to the athlete assigned to this training cycle.
    /// </summary>
    public Guid AthleteId { get; private set; }

    /// <summary>
    /// Name/title of the training cycle.
    /// </summary>
    public string Name { get; private set; } = null!;

    /// <summary>
    /// Duration of the training cycle in weeks.
    /// </summary>
    public int DurationWeeks { get; private set; }

    /// <summary>
    /// Start date of the training cycle.
    /// </summary>
    public DateTime StartDate { get; private set; }

    /// <summary>
    /// Calculated end date of the training cycle.
    /// </summary>
    public DateTime EndDate { get; private set; }

    /// <summary>
    /// Navigation property to the athlete.
    /// </summary>
    public virtual Athlete Athlete { get; private set; } = null!;

    // Private constructor for EF Core
    private TrainingCycle() { }

    /// <summary>
    /// Factory method to create a new TrainingCycle.
    /// </summary>
    /// <param name="athleteId">The athlete's unique identifier.</param>
    /// <param name="name">Name of the training cycle.</param>
    /// <param name="durationWeeks">Duration in weeks.</param>
    /// <param name="startDate">Start date of the cycle.</param>
    /// <returns>A new TrainingCycle instance.</returns>
    public static TrainingCycle Create(Guid athleteId, string name, int durationWeeks, DateTime startDate)
    {
        if (athleteId == Guid.Empty)
            throw new ArgumentException("Athlete ID cannot be empty.", nameof(athleteId));

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be empty.", nameof(name));

        if (durationWeeks <= 0)
            throw new ArgumentException("Duration must be greater than 0.", nameof(durationWeeks));

        var trainingCycle = new TrainingCycle
        {
            AthleteId = athleteId,
            Name = name.Trim(),
            DurationWeeks = durationWeeks,
            StartDate = startDate.Date,
            EndDate = startDate.Date.AddDays(durationWeeks * 7)
        };

        return trainingCycle;
    }

    /// <summary>
    /// Updates the training cycle name.
    /// </summary>
    /// <param name="name">New name for the training cycle.</param>
    public void UpdateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be empty.", nameof(name));

        Name = name.Trim();
    }

    /// <summary>
    /// Updates the duration and recalculates the end date.
    /// </summary>
    /// <param name="durationWeeks">New duration in weeks.</param>
    public void UpdateDuration(int durationWeeks)
    {
        if (durationWeeks <= 0)
            throw new ArgumentException("Duration must be greater than 0.", nameof(durationWeeks));

        DurationWeeks = durationWeeks;
        EndDate = StartDate.AddDays(durationWeeks * 7);
    }

    /// <summary>
    /// Updates the start date and recalculates the end date.
    /// </summary>
    /// <param name="startDate">New start date.</param>
    public void UpdateStartDate(DateTime startDate)
    {
        StartDate = startDate.Date;
        EndDate = StartDate.AddDays(DurationWeeks * 7);
    }

    /// <summary>
    /// Checks if the training cycle is currently active.
    /// </summary>
    /// <returns>True if the cycle is active, false otherwise.</returns>
    public bool IsActive()
    {
        var today = DateTime.UtcNow.Date;
        return today >= StartDate && today <= EndDate;
    }

    /// <summary>
    /// Gets the current week number within the training cycle.
    /// </summary>
    /// <returns>Current week number (1-based), or 0 if not started or completed.</returns>
    public int GetCurrentWeek()
    {
        var today = DateTime.UtcNow.Date;
        
        if (today < StartDate)
            return 0;
        
        if (today > EndDate)
            return 0;

        var daysSinceStart = (today - StartDate).Days;
        return (daysSinceStart / 7) + 1;
    }
}
