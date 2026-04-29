using CoachPlatform.Domain.Enums;

namespace CoachPlatform.Application.Shared.DTOs;

// ========================
// Week Navigation DTOs
// ========================

/// <summary>
/// DTO for the week strip — all days in a specific week with their status.
/// </summary>
public record WeekWorkoutsDto
{
    public string ProgramName { get; init; } = null!;
    public int WeekNumber { get; init; }
    public int TotalWeeks { get; init; }
    public IReadOnlyList<WeekDayDto> Days { get; init; } = [];
}

/// <summary>
/// Lightweight summary of a single day within the week strip.
/// </summary>
public record WeekDayDto
{
    public Guid WorkoutId { get; init; }
    public int DayNumber { get; init; }
    public string? DayName { get; init; }
    public string? Focus { get; init; }
    public DateTime ScheduledDate { get; init; }
    public WorkoutStatus Status { get; init; }
    public string StatusName => Status.ToString();
    public bool IsToday { get; init; }
    public int? DurationMinutes { get; init; }
    public int? FatigueRating { get; init; }
    public int ExerciseCount { get; init; }
    public int CompletedSets { get; init; }
    public int TotalSets { get; init; }
}

// ========================
// Exercise Lift History DTOs
// ========================

/// <summary>
/// DTO for an exercise's complete lift history for an athlete.
/// Includes exercise details, lift entries, and personal records.
/// </summary>
public record ExerciseLiftHistoryDto
{
    public Guid ExerciseId { get; init; }
    public string ExerciseName { get; init; } = null!;
    public string? Category { get; init; }
    public string? PrimaryMuscleGroup { get; init; }
    public string? Equipment { get; init; }
    public string? VideoUrl { get; init; }
    public string? ImageUrl { get; init; }
    public string? Description { get; init; }
    public bool IsCompound { get; init; }
    public List<string> Instructions { get; init; } = [];
    public List<string> CoachingCues { get; init; } = [];
    public decimal? CurrentEstimated1RM { get; init; }
    public ExerciseLiftPRDto? PersonalRecords { get; init; }
    public IReadOnlyList<LiftEntryDto> Entries { get; init; } = [];
}

/// <summary>
/// Personal records for an exercise.
/// </summary>
public record ExerciseLiftPRDto
{
    public decimal MaxWeight { get; init; }
    public DateTime MaxWeightDate { get; init; }
    public int MaxReps { get; init; }
    public DateTime MaxRepsDate { get; init; }
    public decimal MaxEstimated1RM { get; init; }
    public DateTime MaxEstimated1RMDate { get; init; }
    public decimal MaxVolume { get; init; }
    public DateTime MaxVolumeDate { get; init; }
}

/// <summary>
/// A single session's best performance for the lift history chart.
/// </summary>
public record LiftEntryDto
{
    public DateTime Date { get; init; }
    public int WeekNumber { get; init; }
    public int DayNumber { get; init; }
    public decimal MaxWeight { get; init; }
    public int BestReps { get; init; }
    public decimal? Rpe { get; init; }
    public decimal? Estimated1RM { get; init; }
    public decimal TotalVolume { get; init; }
    public int TotalSets { get; init; }
}

// ========================
// Workout Tracking DTOs (F-015)
// ========================

/// <summary>
/// DTO for today's workout view - includes exercises grouped with their sets.
/// </summary>
public record TodayWorkoutDto
{
    public Guid WorkoutId { get; init; }
    public Guid AthleteProgramId { get; init; }
    public string ProgramName { get; init; } = null!;
    public int WeekNumber { get; init; }
    public int DayNumber { get; init; }
    public string? DayName { get; init; }
    public string? Focus { get; init; }
    public DateTime ScheduledDate { get; init; }
    public WorkoutStatus Status { get; init; }
    public string StatusName => Status.ToString();
    public DateTime? StartedAt { get; init; }
    public DateTime? CompletedDate { get; init; }
    public int? DurationMinutes { get; init; }
    public int? FatigueRating { get; init; }
    public string? Notes { get; init; }
    /// <summary>Coach notes for the week, visible to the athlete.</summary>
    public string? WeekNotes { get; init; }
    public IReadOnlyList<WorkoutExerciseGroupDto> Exercises { get; init; } = [];
}

/// <summary>
/// DTO for an exercise within a workout, grouped with all its sets.
/// </summary>
public record WorkoutExerciseGroupDto
{
    public Guid ExerciseId { get; init; }
    public string ExerciseName { get; init; } = null!;
    public int Order { get; init; }
    public int PrescribedSets { get; init; }
    public string? PrescribedReps { get; init; }
    public decimal? PrescribedRpe { get; init; }
    public int? RestSeconds { get; init; }
    public string? ExerciseNotes { get; init; }

    // ── Exercise type & scheme fields (from ProgramExerciseTemplate) ──

    /// <summary>Exercise execution type (standard, emom, tempo, superset, circuit, dropset).</summary>
    public string ExerciseType { get; init; } = "Standard";

    /// <summary>Target percentage of 1RM (e.g. 84 for 84%).</summary>
    public decimal? PercentageRM { get; init; }

    /// <summary>Raw compound notation from the builder (e.g. "1x1 3x4", "Single @9").</summary>
    public string? RawNotation { get; init; }

    /// <summary>Prescribed weight in kg (manual override, not from %RM).</summary>
    public decimal? PrescribedWeight { get; init; }

    /// <summary>EMOM config: totalMinutes, workSeconds, restSeconds.</summary>
    public EmomConfigDto? EmomConfig { get; init; }

    /// <summary>Tempo config: eccentric:pauseBottom:concentric:pauseTop.</summary>
    public TempoConfigDto? TempoConfig { get; init; }

    /// <summary>Superset group identifier (exercises sharing this ID are performed back-to-back).</summary>
    public string? SupersetGroupId { get; init; }

    /// <summary>Position within a superset (1 = A1, 2 = A2, etc.).</summary>
    public int? SupersetPosition { get; init; }

    public IReadOnlyList<WorkoutSetDto> Sets { get; init; } = [];
    public PreviousPerformanceDto? PreviousPerformance { get; init; }
}

/// <summary>EMOM timing configuration.</summary>
public record EmomConfigDto
{
    public int TotalMinutes { get; init; }
    public int? WorkSeconds { get; init; }
    public int? RestSeconds { get; init; }
}

/// <summary>Tempo timing configuration (eccentric:pause:concentric:pause).</summary>
public record TempoConfigDto
{
    public int Eccentric { get; init; }
    public int PauseBottom { get; init; }
    public int Concentric { get; init; }
    public int? PauseTop { get; init; }
}

/// <summary>
/// DTO for an individual set within an exercise.
/// </summary>
public record WorkoutSetDto
{
    public Guid Id { get; init; }
    public int SetNumber { get; init; }
    public int? TargetReps { get; init; }
    public decimal? TargetWeight { get; init; }

    /// <summary>Target RPE for this specific set (may vary per set in compound notation).</summary>
    public decimal? TargetRpe { get; init; }

    /// <summary>Target %1RM for this specific set (may vary per set in compound notation).</summary>
    public decimal? TargetPercentageRM { get; init; }

    /// <summary>
    /// Suggested weight calculated from estimated 1RM × target %RM.
    /// Rounded to nearest 2.5 kg. Null when no %RM or no 1RM history.
    /// </summary>
    public decimal? SuggestedWeight { get; init; }

    /// <summary>Label for compound sets (e.g. "Top Set", "Backoff 1").</summary>
    public string? SetLabel { get; init; }

    public int ActualReps { get; init; }
    public decimal ActualWeight { get; init; }
    public decimal? ActualRpe { get; init; }
    public bool IsCompleted { get; init; }
    public string? SkippedReason { get; init; }
    public string? Notes { get; init; }
}

/// <summary>
/// DTO for previous performance of an exercise (last time athlete did it).
/// </summary>
public record PreviousPerformanceDto
{
    public DateTime Date { get; init; }
    public decimal MaxWeight { get; init; }
    public int MaxReps { get; init; }
    public decimal? BestRpe { get; init; }
    public decimal? EstimatedOneRM { get; init; }
    public int TotalSets { get; init; }
}

/// <summary>
/// DTO for workout history list items.
/// </summary>
public record WorkoutHistoryItemDto
{
    public Guid Id { get; init; }
    public int WeekNumber { get; init; }
    public int DayNumber { get; init; }
    public string? DayName { get; init; }
    public string? Focus { get; init; }
    public DateTime ScheduledDate { get; init; }
    public WorkoutStatus Status { get; init; }
    public string StatusName => Status.ToString();
    public DateTime? CompletedDate { get; init; }
    public int? DurationMinutes { get; init; }
    public int? FatigueRating { get; init; }
    public int ExerciseCount { get; init; }
    public int CompletedSets { get; init; }
    public int TotalSets { get; init; }
}
