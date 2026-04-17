using CoachPlatform.Domain.Common;

namespace CoachPlatform.Domain.Entities;

/// <summary>
/// ProgramTemplate entity - represents a training program template created by a coach.
/// Aggregate Root - the coach's reusable training program blueprint.
/// </summary>
public class ProgramTemplate : AuditableEntity, IAggregateRoot
{
    /// <summary>
    /// Reference to the coach who owns this program template.
    /// Required for multi-tenant support.
    /// </summary>
    public Guid CoachId { get; private set; }

    /// <summary>
    /// Name of the program template.
    /// </summary>
    public string Name { get; private set; } = null!;

    /// <summary>
    /// Description of the program, its goals and methodology.
    /// </summary>
    public string? Description { get; private set; }

    /// <summary>
    /// Total duration of the program in weeks.
    /// </summary>
    public int DurationWeeks { get; private set; }

    /// <summary>
    /// Whether this program template is currently active and available for assignment.
    /// </summary>
    public bool IsActive { get; private set; }

    // Navigation properties
    public Coach Coach { get; private set; } = null!;
    private readonly List<ProgramWeekTemplate> _weeks = [];
    public IReadOnlyCollection<ProgramWeekTemplate> Weeks => _weeks.AsReadOnly();

    // EF Core constructor
    private ProgramTemplate() { }

    private ProgramTemplate(
        Guid coachId,
        string name,
        string? description,
        int durationWeeks)
    {
        CoachId = coachId;
        Name = name;
        Description = description;
        DurationWeeks = durationWeeks;
        IsActive = true;
    }

    /// <summary>
    /// Creates a new program template.
    /// </summary>
    public static ProgramTemplate Create(
        Guid coachId,
        string name,
        string? description,
        int durationWeeks)
    {
        if (coachId == Guid.Empty)
            throw new ArgumentException("Coach ID is required.", nameof(coachId));

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name is required.", nameof(name));

        if (durationWeeks < 1 || durationWeeks > 52)
            throw new ArgumentException("Duration must be between 1 and 52 weeks.", nameof(durationWeeks));

        return new ProgramTemplate(coachId, name.Trim(), description?.Trim(), durationWeeks);
    }

    /// <summary>
    /// Updates the program template details.
    /// </summary>
    public void Update(string name, string? description, int durationWeeks)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name is required.", nameof(name));

        if (durationWeeks < 1 || durationWeeks > 52)
            throw new ArgumentException("Duration must be between 1 and 52 weeks.", nameof(durationWeeks));

        Name = name.Trim();
        Description = description?.Trim();
        DurationWeeks = durationWeeks;
    }

    /// <summary>
    /// Adds a week to the program template.
    /// </summary>
    public ProgramWeekTemplate AddWeek(int weekNumber, string? name = null)
    {
        if (weekNumber < 1 || weekNumber > DurationWeeks)
            throw new ArgumentException($"Week number must be between 1 and {DurationWeeks}.", nameof(weekNumber));

        if (_weeks.Any(w => w.WeekNumber == weekNumber))
            throw new InvalidOperationException($"Week {weekNumber} already exists in this program.");

        var week = ProgramWeekTemplate.Create(Id, weekNumber, name);
        _weeks.Add(week);
        return week;
    }

    /// <summary>
    /// Removes a week from the program template.
    /// </summary>
    public void RemoveWeek(int weekNumber)
    {
        var week = _weeks.FirstOrDefault(w => w.WeekNumber == weekNumber);
        if (week == null)
            throw new InvalidOperationException($"Week {weekNumber} does not exist in this program.");

        _weeks.Remove(week);
    }

    /// <summary>
    /// Removes a week by its ID.
    /// </summary>
    public void RemoveWeekById(Guid weekId)
    {
        var week = _weeks.FirstOrDefault(w => w.Id == weekId);
        if (week == null)
            throw new InvalidOperationException("Week does not exist in this program.");

        _weeks.Remove(week);
    }

    /// <summary>
    /// Clears all weeks from the program.
    /// </summary>
    public void ClearWeeks()
    {
        _weeks.Clear();
    }

    /// <summary>
    /// Activates the program template.
    /// </summary>
    public void Activate()
    {
        IsActive = true;
    }

    /// <summary>
    /// Deactivates the program template.
    /// </summary>
    public void Deactivate()
    {
        IsActive = false;
    }
}
