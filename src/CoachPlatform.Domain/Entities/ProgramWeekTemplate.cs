using CoachPlatform.Domain.Common;

namespace CoachPlatform.Domain.Entities;

/// <summary>
/// ProgramWeekTemplate entity - represents a week within a training program template.
/// </summary>
public class ProgramWeekTemplate : AuditableEntity
{
    /// <summary>
    /// Reference to the parent program template.
    /// </summary>
    public Guid ProgramTemplateId { get; private set; }

    /// <summary>
    /// Week number within the program (1-based).
    /// </summary>
    public int WeekNumber { get; private set; }

    /// <summary>
    /// Display name for the week (e.g., "Week 1", "Deload Week").
    /// </summary>
    public string? Name { get; private set; }

    /// <summary>
    /// Optional notes or theme for the week (e.g., "Deload", "Intensity Week").
    /// </summary>
    public string? Notes { get; private set; }

    // Navigation properties
    public ProgramTemplate ProgramTemplate { get; private set; } = null!;
    private readonly List<ProgramDayTemplate> _days = [];
    public IReadOnlyCollection<ProgramDayTemplate> Days => _days.AsReadOnly();

    // EF Core constructor
    private ProgramWeekTemplate() { }

    private ProgramWeekTemplate(Guid programTemplateId, int weekNumber, string? name = null)
    {
        ProgramTemplateId = programTemplateId;
        WeekNumber = weekNumber;
        Name = name;
    }

    /// <summary>
    /// Creates a new program week template.
    /// </summary>
    internal static ProgramWeekTemplate Create(Guid programTemplateId, int weekNumber, string? name = null)
    {
        if (programTemplateId == Guid.Empty)
            throw new ArgumentException("Program template ID is required.", nameof(programTemplateId));

        if (weekNumber < 1)
            throw new ArgumentException("Week number must be at least 1.", nameof(weekNumber));

        return new ProgramWeekTemplate(programTemplateId, weekNumber, name?.Trim());
    }

    /// <summary>
    /// Updates the week name and notes.
    /// </summary>
    public void Update(string? name, string? notes)
    {
        Name = name?.Trim();
        Notes = notes?.Trim();
    }

    /// <summary>
    /// Sets notes for the week.
    /// </summary>
    public void SetNotes(string? notes)
    {
        Notes = notes?.Trim();
    }

    /// <summary>
    /// Adds a training day to the week.
    /// </summary>
    public ProgramDayTemplate AddDay(int dayNumber, Enums.DayFocus? focus = null, string? name = null, string? notes = null)
    {
        if (dayNumber < 1 || dayNumber > 7)
            throw new ArgumentException("Day number must be between 1 and 7.", nameof(dayNumber));

        if (_days.Any(d => d.DayNumber == dayNumber))
            throw new InvalidOperationException($"Day {dayNumber} already exists in this week.");

        var day = ProgramDayTemplate.Create(Id, dayNumber, focus, name, notes);
        _days.Add(day);
        return day;
    }

    /// <summary>
    /// Removes a training day from the week.
    /// </summary>
    public void RemoveDay(int dayNumber)
    {
        var day = _days.FirstOrDefault(d => d.DayNumber == dayNumber);
        if (day == null)
            throw new InvalidOperationException($"Day {dayNumber} does not exist in this week.");

        _days.Remove(day);
    }

    /// <summary>
    /// Removes a training day by its ID.
    /// </summary>
    public void RemoveDayById(Guid dayId)
    {
        var day = _days.FirstOrDefault(d => d.Id == dayId);
        if (day == null)
            throw new InvalidOperationException("Day does not exist in this week.");

        _days.Remove(day);
    }

    /// <summary>
    /// Clears all days from this week.
    /// </summary>
    public void ClearDays()
    {
        _days.Clear();
    }
}
