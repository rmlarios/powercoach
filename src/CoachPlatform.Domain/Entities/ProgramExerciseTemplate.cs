using CoachPlatform.Domain.Common;
using CoachPlatform.Domain.Enums;

namespace CoachPlatform.Domain.Entities;

/// <summary>
/// ProgramExerciseTemplate entity - represents an exercise prescription within a program day.
/// </summary>
public class ProgramExerciseTemplate : AuditableEntity
{
    /// <summary>
    /// Reference to the parent day template.
    /// </summary>
    public Guid DayTemplateId { get; private set; }

    /// <summary>
    /// Reference to the exercise in the catalog.
    /// </summary>
    public Guid ExerciseId { get; private set; }

    /// <summary>
    /// Number of sets to perform.
    /// </summary>
    public int Sets { get; private set; }

    /// <summary>
    /// Rep scheme (can be a range like "6-8" or specific like "5" or "AMRAP").
    /// </summary>
    public string Reps { get; private set; } = null!;

    /// <summary>
    /// Target RPE (Rate of Perceived Exertion) on a 1-10 scale.
    /// </summary>
    public decimal? TargetRpe { get; private set; }

    /// <summary>
    /// Rest time between sets in seconds.
    /// </summary>
    public int? RestSeconds { get; private set; }

    /// <summary>
    /// Additional notes or instructions for the exercise.
    /// </summary>
    public string? Notes { get; private set; }

    /// <summary>
    /// Display order within the day.
    /// </summary>
    public int Order { get; private set; }

    /// <summary>
    /// Type of exercise execution (standard, emom, tempo, superset, etc.).
    /// </summary>
    public ExerciseType ExerciseType { get; private set; }

    /// <summary>
    /// Target percentage of 1RM for weight calculation.
    /// </summary>
    public decimal? PercentageRM { get; private set; }

    /// <summary>
    /// Raw compound notation when sets vary (e.g. "1x1 3x4", "Single @9").
    /// </summary>
    public string? RawNotation { get; private set; }

    /// <summary>
    /// Explicit weight in kg (manual override, not calculated from %RM).
    /// </summary>
    public decimal? Weight { get; private set; }

    /// <summary>
    /// EMOM configuration as JSON (totalMinutes, workSeconds, restSeconds).
    /// Null when ExerciseType != Emom.
    /// </summary>
    public string? EmomConfigJson { get; private set; }

    /// <summary>
    /// Tempo configuration as JSON (eccentric, pauseBottom, concentric, pauseTop).
    /// Null when ExerciseType != Tempo.
    /// </summary>
    public string? TempoConfigJson { get; private set; }

    /// <summary>
    /// Superset configuration as JSON (groupId, position).
    /// Null when ExerciseType != Superset.
    /// </summary>
    public string? SupersetConfigJson { get; private set; }

    // Navigation properties
    public ProgramDayTemplate DayTemplate { get; private set; } = null!;
    public Exercise Exercise { get; private set; } = null!;

    // EF Core constructor
    private ProgramExerciseTemplate() { }

    private ProgramExerciseTemplate(
        Guid dayTemplateId,
        Guid exerciseId,
        int sets,
        string reps,
        decimal? targetRpe,
        int? restSeconds,
        string? notes,
        int order,
        ExerciseType exerciseType = ExerciseType.Standard,
        decimal? percentageRM = null,
        string? rawNotation = null,
        decimal? weight = null,
        string? emomConfigJson = null,
        string? tempoConfigJson = null,
        string? supersetConfigJson = null)
    {
        DayTemplateId = dayTemplateId;
        ExerciseId = exerciseId;
        Sets = sets;
        Reps = reps;
        TargetRpe = targetRpe;
        RestSeconds = restSeconds;
        Notes = notes;
        Order = order;
        ExerciseType = exerciseType;
        PercentageRM = percentageRM;
        RawNotation = rawNotation;
        Weight = weight;
        EmomConfigJson = emomConfigJson;
        TempoConfigJson = tempoConfigJson;
        SupersetConfigJson = supersetConfigJson;
    }

    /// <summary>
    /// Creates a new program exercise template.
    /// </summary>
    internal static ProgramExerciseTemplate Create(
        Guid dayTemplateId,
        Guid exerciseId,
        int sets,
        string reps,
        decimal? targetRpe = null,
        int? restSeconds = null,
        string? notes = null,
        int order = 1,
        ExerciseType exerciseType = ExerciseType.Standard,
        decimal? percentageRM = null,
        string? rawNotation = null,
        decimal? weight = null,
        string? emomConfigJson = null,
        string? tempoConfigJson = null,
        string? supersetConfigJson = null)
    {
        if (dayTemplateId == Guid.Empty)
            throw new ArgumentException("Day template ID is required.", nameof(dayTemplateId));

        if (exerciseId == Guid.Empty)
            throw new ArgumentException("Exercise ID is required.", nameof(exerciseId));

        if (sets < 1 || sets > 50)
            throw new ArgumentException("Sets must be between 1 and 50.", nameof(sets));

        if (string.IsNullOrWhiteSpace(reps))
            throw new ArgumentException("Reps prescription is required.", nameof(reps));

        if (targetRpe.HasValue && (targetRpe < 1 || targetRpe > 10))
            throw new ArgumentException("Target RPE must be between 1 and 10.", nameof(targetRpe));

        if (restSeconds.HasValue && (restSeconds < 0 || restSeconds > 600))
            throw new ArgumentException("Rest seconds must be between 0 and 600.", nameof(restSeconds));

        if (percentageRM.HasValue && (percentageRM < 1 || percentageRM > 120))
            throw new ArgumentException("Percentage of 1RM must be between 1 and 120.", nameof(percentageRM));

        if (weight.HasValue && (weight < 0 || weight > 1000))
            throw new ArgumentException("Weight must be between 0 and 1000 kg.", nameof(weight));

        return new ProgramExerciseTemplate(
            dayTemplateId, exerciseId, sets, reps.Trim(), targetRpe, restSeconds, notes?.Trim(), order,
            exerciseType, percentageRM, rawNotation?.Trim(), weight,
            emomConfigJson, tempoConfigJson, supersetConfigJson);
    }

    /// <summary>
    /// Updates the exercise prescription.
    /// </summary>
    public void Update(
        int sets,
        string reps,
        decimal? targetRpe,
        int? restSeconds,
        string? notes,
        ExerciseType exerciseType = ExerciseType.Standard,
        decimal? percentageRM = null,
        string? rawNotation = null,
        decimal? weight = null,
        string? emomConfigJson = null,
        string? tempoConfigJson = null,
        string? supersetConfigJson = null)
    {
        if (sets < 1 || sets > 50)
            throw new ArgumentException("Sets must be between 1 and 50.", nameof(sets));

        if (string.IsNullOrWhiteSpace(reps))
            throw new ArgumentException("Reps prescription is required.", nameof(reps));

        if (targetRpe.HasValue && (targetRpe < 1 || targetRpe > 10))
            throw new ArgumentException("Target RPE must be between 1 and 10.", nameof(targetRpe));

        if (restSeconds.HasValue && (restSeconds < 0 || restSeconds > 600))
            throw new ArgumentException("Rest seconds must be between 0 and 600.", nameof(restSeconds));

        Sets = sets;
        Reps = reps.Trim();
        TargetRpe = targetRpe;
        RestSeconds = restSeconds;
        Notes = notes?.Trim();
        ExerciseType = exerciseType;
        PercentageRM = percentageRM;
        RawNotation = rawNotation?.Trim();
        Weight = weight;
        EmomConfigJson = emomConfigJson;
        TempoConfigJson = tempoConfigJson;
        SupersetConfigJson = supersetConfigJson;
    }

    /// <summary>
    /// Sets the display order.
    /// </summary>
    internal void SetOrder(int order)
    {
        if (order < 1)
            throw new ArgumentException("Order must be at least 1.", nameof(order));

        Order = order;
    }
}
