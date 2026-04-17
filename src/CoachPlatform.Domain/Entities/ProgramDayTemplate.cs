using CoachPlatform.Domain.Common;
using CoachPlatform.Domain.Enums;

namespace CoachPlatform.Domain.Entities;

/// <summary>
/// ProgramDayTemplate entity - represents a training day within a program week template.
/// </summary>
public class ProgramDayTemplate : AuditableEntity
{
    /// <summary>
    /// Reference to the parent week template.
    /// </summary>
    public Guid WeekTemplateId { get; private set; }

    /// <summary>
    /// Day number within the week (1-7).
    /// </summary>
    public int DayNumber { get; private set; }

    /// <summary>
    /// Display name for the day (e.g., "Push", "Back & Biceps").
    /// </summary>
    public string? Name { get; private set; }

    /// <summary>
    /// Primary focus of the training day.
    /// </summary>
    public DayFocus Focus { get; private set; }

    /// <summary>
    /// Optional notes or instructions for the day.
    /// </summary>
    public string? Notes { get; private set; }

    // Navigation properties
    public ProgramWeekTemplate WeekTemplate { get; private set; } = null!;
    private readonly List<ProgramExerciseTemplate> _exercises = [];
    public IReadOnlyCollection<ProgramExerciseTemplate> Exercises => _exercises.AsReadOnly();

    // EF Core constructor
    private ProgramDayTemplate() { }

    private ProgramDayTemplate(
        Guid weekTemplateId,
        int dayNumber,
        DayFocus focus,
        string? name,
        string? notes)
    {
        WeekTemplateId = weekTemplateId;
        DayNumber = dayNumber;
        Focus = focus;
        Name = name;
        Notes = notes;
    }

    /// <summary>
    /// Creates a new program day template.
    /// </summary>
    internal static ProgramDayTemplate Create(
        Guid weekTemplateId,
        int dayNumber,
        DayFocus focus,
        string? name = null,
        string? notes = null)
    {
        if (weekTemplateId == Guid.Empty)
            throw new ArgumentException("Week template ID is required.", nameof(weekTemplateId));

        if (dayNumber < 1 || dayNumber > 7)
            throw new ArgumentException("Day number must be between 1 and 7.", nameof(dayNumber));

        return new ProgramDayTemplate(weekTemplateId, dayNumber, focus, name?.Trim(), notes?.Trim());
    }

    /// <summary>
    /// Updates the day template.
    /// </summary>
    public void Update(DayFocus focus, string? name, string? notes)
    {
        Focus = focus;
        Name = name?.Trim();
        Notes = notes?.Trim();
    }

    /// <summary>
    /// Adds an exercise to the day.
    /// </summary>
    public ProgramExerciseTemplate AddExercise(
        Guid exerciseId,
        int sets,
        string reps,
        decimal? targetRpe = null,
        int? restSeconds = null,
        string? notes = null,
        ExerciseType exerciseType = ExerciseType.Standard,
        decimal? percentageRM = null,
        string? rawNotation = null,
        decimal? weight = null,
        string? emomConfigJson = null,
        string? tempoConfigJson = null,
        string? supersetConfigJson = null)
    {
        var order = _exercises.Count + 1;
        var exercise = ProgramExerciseTemplate.Create(
            Id, exerciseId, sets, reps, targetRpe, restSeconds, notes, order,
            exerciseType, percentageRM, rawNotation, weight,
            emomConfigJson, tempoConfigJson, supersetConfigJson);
        _exercises.Add(exercise);
        return exercise;
    }

    /// <summary>
    /// Removes an exercise from the day.
    /// </summary>
    public void RemoveExercise(Guid exerciseTemplateId)
    {
        var exercise = _exercises.FirstOrDefault(e => e.Id == exerciseTemplateId);
        if (exercise == null)
            throw new InvalidOperationException("Exercise not found in this day.");

        _exercises.Remove(exercise);
        
        // Reorder remaining exercises
        var orderedExercises = _exercises.OrderBy(e => e.Order).ToList();
        for (int i = 0; i < orderedExercises.Count; i++)
        {
            orderedExercises[i].SetOrder(i + 1);
        }
    }

    /// <summary>
    /// Clears all exercises from this day.
    /// </summary>
    public void ClearExercises()
    {
        _exercises.Clear();
    }

    /// <summary>
    /// Reorders an exercise within the day.
    /// </summary>
    public void ReorderExercise(Guid exerciseTemplateId, int newOrder)
    {
        var exercise = _exercises.FirstOrDefault(e => e.Id == exerciseTemplateId);
        if (exercise == null)
            throw new InvalidOperationException("Exercise not found in this day.");

        if (newOrder < 1 || newOrder > _exercises.Count)
            throw new ArgumentException($"Order must be between 1 and {_exercises.Count}.", nameof(newOrder));

        var currentOrder = exercise.Order;
        if (currentOrder == newOrder) return;

        foreach (var ex in _exercises)
        {
            if (ex.Id == exerciseTemplateId)
            {
                ex.SetOrder(newOrder);
            }
            else if (currentOrder < newOrder && ex.Order > currentOrder && ex.Order <= newOrder)
            {
                ex.SetOrder(ex.Order - 1);
            }
            else if (currentOrder > newOrder && ex.Order >= newOrder && ex.Order < currentOrder)
            {
                ex.SetOrder(ex.Order + 1);
            }
        }
    }
}
