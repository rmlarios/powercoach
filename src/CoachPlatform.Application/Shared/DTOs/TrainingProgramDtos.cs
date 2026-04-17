using CoachPlatform.Domain.Enums;

namespace CoachPlatform.Application.Shared.DTOs;

// ========================
// Program Template DTOs
// ========================

/// <summary>
/// DTO for program template list view.
/// </summary>
public record ProgramTemplateListItemDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = null!;
    public string? Description { get; init; }
    public int DurationWeeks { get; init; }
    public bool IsActive { get; init; }
    public int WeekCount { get; init; }
    public DateTime CreatedAt { get; init; }
}

/// <summary>
/// DTO for program template detail view.
/// </summary>
public record ProgramTemplateDetailDto
{
    public Guid Id { get; init; }
    public Guid CoachId { get; init; }
    public string Name { get; init; } = null!;
    public string? Description { get; init; }
    public int DurationWeeks { get; init; }
    public bool IsActive { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
    public IReadOnlyList<ProgramWeekTemplateDto> Weeks { get; init; } = [];
}

/// <summary>
/// DTO for creating a new program template.
/// </summary>
public record CreateProgramTemplateDto
{
    public Guid CoachId { get; init; }
    public string Name { get; init; } = null!;
    public string? Description { get; init; }
    public int DurationWeeks { get; init; }
}

/// <summary>
/// DTO for updating a program template.
/// </summary>
public record UpdateProgramTemplateDto
{
    public string Name { get; init; } = null!;
    public string? Description { get; init; }
    public int DurationWeeks { get; init; }
}

// ========================
// Program Week Template DTOs
// ========================

/// <summary>
/// DTO for program week template.
/// </summary>
public record ProgramWeekTemplateDto
{
    public Guid Id { get; init; }
    public int WeekNumber { get; init; }
    public string? Name { get; init; }
    public string? Notes { get; init; }
    public IReadOnlyList<ProgramDayTemplateDto> Days { get; init; } = [];
}

/// <summary>
/// DTO for adding a week to a program.
/// </summary>
public record AddProgramWeekDto
{
    public int WeekNumber { get; init; }
    public string? Name { get; init; }
    public string? Notes { get; init; }
}

// ========================
// Program Day Template DTOs
// ========================

/// <summary>
/// DTO for program day template.
/// </summary>
public record ProgramDayTemplateDto
{
    public Guid Id { get; init; }
    public int DayNumber { get; init; }
    public string? Name { get; init; }
    public DayFocus Focus { get; init; }
    public string FocusName => Focus.ToString();
    public string? Notes { get; init; }
    public IReadOnlyList<ProgramExerciseTemplateDto> Exercises { get; init; } = [];
}

/// <summary>
/// DTO for adding a day to a week.
/// </summary>
public record AddProgramDayDto
{
    public int DayNumber { get; init; }
    public string? Name { get; init; }
    public DayFocus Focus { get; init; }
    public string? Notes { get; init; }
}

// ========================
// Program Exercise Template DTOs
// ========================

/// <summary>
/// DTO for program exercise template.
/// </summary>
public record ProgramExerciseTemplateDto
{
    public Guid Id { get; init; }
    public Guid ExerciseId { get; init; }
    public string ExerciseName { get; init; } = null!;
    public int Sets { get; init; }
    public string Reps { get; init; } = null!;
    public decimal? TargetRpe { get; init; }
    public int? RestSeconds { get; init; }
    public string? Notes { get; init; }
    public int Order { get; init; }
    public string ExerciseType { get; init; } = "Standard";
    public decimal? PercentageRM { get; init; }
    public string? RawNotation { get; init; }
    public decimal? Weight { get; init; }
    public string? EmomConfigJson { get; init; }
    public string? TempoConfigJson { get; init; }
    public string? SupersetConfigJson { get; init; }
}

/// <summary>
/// DTO for adding an exercise to a day.
/// </summary>
public record AddProgramExerciseDto
{
    public Guid ExerciseId { get; init; }
    public int Sets { get; init; }
    public string Reps { get; init; } = null!;
    public decimal? TargetRpe { get; init; }
    public int? RestSeconds { get; init; }
    public string? Notes { get; init; }
    public string? ExerciseType { get; init; }
    public decimal? PercentageRM { get; init; }
    public string? RawNotation { get; init; }
    public decimal? Weight { get; init; }
    public string? EmomConfigJson { get; init; }
    public string? TempoConfigJson { get; init; }
    public string? SupersetConfigJson { get; init; }
}

// ========================
// Bulk Save DTOs
// ========================

/// <summary>
/// DTO for bulk saving an entire program template (all weeks/days/exercises at once).
/// </summary>
public record SaveProgramTemplateDto
{
    public string Name { get; init; } = null!;
    public string? Description { get; init; }
    public int DurationWeeks { get; init; }
    public IReadOnlyList<SaveProgramWeekDto> Weeks { get; init; } = [];
}

public record SaveProgramWeekDto
{
    public int WeekNumber { get; init; }
    public string? Name { get; init; }
    public string? Notes { get; init; }
    public IReadOnlyList<SaveProgramDayDto> Days { get; init; } = [];
}

public record SaveProgramDayDto
{
    public int DayNumber { get; init; }
    public string? Name { get; init; }
    public string Focus { get; init; } = "FullBody";
    public string? Notes { get; init; }
    public IReadOnlyList<SaveProgramExerciseDto> Exercises { get; init; } = [];
}

public record SaveProgramExerciseDto
{
    public Guid ExerciseId { get; init; }
    public int Sets { get; init; }
    public string Reps { get; init; } = null!;
    public decimal? TargetRpe { get; init; }
    public int? RestSeconds { get; init; }
    public string? Notes { get; init; }
    public int Order { get; init; }
    public string? ExerciseType { get; init; }
    public decimal? PercentageRM { get; init; }
    public string? RawNotation { get; init; }
    public decimal? Weight { get; init; }
    public string? EmomConfigJson { get; init; }
    public string? TempoConfigJson { get; init; }
    public string? SupersetConfigJson { get; init; }
}

// ========================
// Athlete Program DTOs
// ========================

/// <summary>
/// DTO for athlete program list view.
/// </summary>
public record AthleteProgramListItemDto
{
    public Guid Id { get; init; }
    public Guid AthleteId { get; init; }
    public string AthleteName { get; init; } = null!;
    public string ProgramName { get; init; } = null!;
    public DateTime StartDate { get; init; }
    public DateTime? EndDate { get; init; }
    public int CurrentWeek { get; init; }
    public int TotalWeeks { get; init; }
    public ProgramStatus Status { get; init; }
    public string StatusName => Status.ToString();
}

/// <summary>
/// DTO for athlete program detail view.
/// </summary>
public record AthleteProgramDetailDto
{
    public Guid Id { get; init; }
    public Guid AthleteId { get; init; }
    public string AthleteName { get; init; } = null!;
    public Guid ProgramTemplateId { get; init; }
    public string ProgramName { get; init; } = null!;
    public string? ProgramDescription { get; init; }
    public DateTime StartDate { get; init; }
    public DateTime? EndDate { get; init; }
    public int CurrentWeek { get; init; }
    public int TotalWeeks { get; init; }
    public ProgramStatus Status { get; init; }
    public string StatusName => Status.ToString();
    public string? Notes { get; init; }
    public DateTime CreatedAt { get; init; }
    public IReadOnlyList<AthleteWorkoutDto> Workouts { get; init; } = [];
}

/// <summary>
/// DTO for assigning a program to an athlete.
/// </summary>
public record AssignProgramToAthleteDto
{
    public Guid AthleteId { get; init; }
    public Guid ProgramTemplateId { get; init; }
    public DateTime StartDate { get; init; }
    public string? Notes { get; init; }
}

// ========================
// Athlete Workout DTOs
// ========================

/// <summary>
/// DTO for athlete workout.
/// </summary>
public record AthleteWorkoutDto
{
    public Guid Id { get; init; }
    public int WeekNumber { get; init; }
    public int DayNumber { get; init; }
    public DayFocus Focus { get; init; }
    public string FocusName => Focus.ToString();
    public DateTime ScheduledDate { get; init; }
    public DateTime? CompletedDate { get; init; }
    public bool IsCompleted { get; init; }
    public int? DurationMinutes { get; init; }
    public int? FatigueRating { get; init; }
    public string? Notes { get; init; }
    public IReadOnlyList<AthleteExerciseLogDto> ExerciseLogs { get; init; } = [];
    public IReadOnlyList<ProgramExerciseTemplateDto> PrescribedExercises { get; init; } = [];
}

/// <summary>
/// DTO for completing a workout.
/// </summary>
public record CompleteWorkoutDto
{
    public int? DurationMinutes { get; init; }
    public int? FatigueRating { get; init; }
    public string? Notes { get; init; }
}

// ========================
// Athlete Exercise Log DTOs
// ========================

/// <summary>
/// DTO for athlete exercise log.
/// </summary>
public record AthleteExerciseLogDto
{
    public Guid Id { get; init; }
    public Guid ExerciseId { get; init; }
    public string ExerciseName { get; init; } = null!;
    public int SetNumber { get; init; }
    public int Reps { get; init; }
    public decimal Weight { get; init; }
    public decimal? Rpe { get; init; }
    public string? Notes { get; init; }
    public DateTime CreatedAt { get; init; }
}

/// <summary>
/// DTO for logging an exercise set.
/// </summary>
public record LogExerciseSetDto
{
    public Guid ExerciseId { get; init; }
    public int SetNumber { get; init; }
    public int Reps { get; init; }
    public decimal Weight { get; init; }
    public decimal? Rpe { get; init; }
    public string? Notes { get; init; }
}

/// <summary>
/// DTO for logging a complete workout with all sets.
/// </summary>
public record LogWorkoutDto
{
    public int? DurationMinutes { get; init; }
    public int? FatigueRating { get; init; }
    public string? Notes { get; init; }
    public IReadOnlyList<LogExerciseSetDto> ExerciseSets { get; init; } = [];
}

// ========================
// Athlete Current Program View DTO
// ========================

/// <summary>
/// DTO for getting the athlete's current program view.
/// </summary>
public record AthleteCurrentProgramDto
{
    public Guid AthleteProgramId { get; init; }
    public string ProgramName { get; init; } = null!;
    public string? ProgramDescription { get; init; }
    public int CurrentWeek { get; init; }
    public int TotalWeeks { get; init; }
    public ProgramStatus Status { get; init; }
    public IReadOnlyList<AthleteWeekScheduleDto> WeekSchedule { get; init; } = [];
}

/// <summary>
/// DTO for a week in the athlete's schedule.
/// </summary>
public record AthleteWeekScheduleDto
{
    public int WeekNumber { get; init; }
    public bool IsCurrentWeek { get; init; }
    public IReadOnlyList<AthleteDayScheduleDto> Days { get; init; } = [];
}

/// <summary>
/// DTO for a day in the athlete's schedule.
/// </summary>
public record AthleteDayScheduleDto
{
    public int DayNumber { get; init; }
    public DayFocus Focus { get; init; }
    public string FocusName => Focus.ToString();
    public string? Notes { get; init; }
    public DateTime ScheduledDate { get; init; }
    public bool IsCompleted { get; init; }
    public Guid? WorkoutId { get; init; }
    public IReadOnlyList<ProgramExerciseTemplateDto> Exercises { get; init; } = [];
}
