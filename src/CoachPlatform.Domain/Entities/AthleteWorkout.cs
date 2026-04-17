using CoachPlatform.Domain.Common;
using CoachPlatform.Domain.Enums;

namespace CoachPlatform.Domain.Entities;

/// <summary>
/// AthleteWorkout entity - represents a specific training session for an athlete.
/// </summary>
public class AthleteWorkout : AuditableEntity
{
    /// <summary>
    /// Reference to the parent athlete program.
    /// </summary>
    public Guid AthleteProgramId { get; private set; }

    /// <summary>
    /// Week number within the program.
    /// </summary>
    public int WeekNumber { get; private set; }

    /// <summary>
    /// Day number within the week.
    /// </summary>
    public int DayNumber { get; private set; }

    /// <summary>
    /// Scheduled date for the workout.
    /// </summary>
    public DateTime ScheduledDate { get; private set; }

    /// <summary>
    /// Current status of the workout.
    /// </summary>
    public WorkoutStatus Status { get; private set; }

    /// <summary>
    /// Date and time when the workout was started.
    /// </summary>
    public DateTime? StartedAt { get; private set; }

    /// <summary>
    /// Actual date when the workout was completed.
    /// </summary>
    public DateTime? CompletedDate { get; private set; }

    /// <summary>
    /// Whether the workout has been completed.
    /// </summary>
    public bool IsCompleted { get; private set; }

    /// <summary>
    /// Duration of the workout in minutes.
    /// </summary>
    public int? DurationMinutes { get; private set; }

    /// <summary>
    /// Overall feeling/fatigue rating (1-10).
    /// </summary>
    public int? FatigueRating { get; private set; }

    /// <summary>
    /// Reason for skipping the workout (if skipped).
    /// </summary>
    public string? SkippedReason { get; private set; }

    /// <summary>
    /// Athlete's notes about the workout.
    /// </summary>
    public string? Notes { get; private set; }

    // Navigation properties
    public AthleteProgram AthleteProgram { get; private set; } = null!;
    private readonly List<AthleteExerciseLog> _exerciseLogs = [];
    public IReadOnlyCollection<AthleteExerciseLog> ExerciseLogs => _exerciseLogs.AsReadOnly();

    // EF Core constructor
    private AthleteWorkout() { }

    private AthleteWorkout(
        Guid athleteProgramId,
        int weekNumber,
        int dayNumber,
        DateTime scheduledDate)
    {
        AthleteProgramId = athleteProgramId;
        WeekNumber = weekNumber;
        DayNumber = dayNumber;
        ScheduledDate = scheduledDate;
        Status = WorkoutStatus.NotStarted;
        IsCompleted = false;
    }

    /// <summary>
    /// Creates a new athlete workout.
    /// </summary>
    internal static AthleteWorkout Create(
        Guid athleteProgramId,
        int weekNumber,
        int dayNumber,
        DateTime scheduledDate)
    {
        if (athleteProgramId == Guid.Empty)
            throw new ArgumentException("Athlete program ID is required.", nameof(athleteProgramId));

        if (weekNumber < 1)
            throw new ArgumentException("Week number must be at least 1.", nameof(weekNumber));

        if (dayNumber < 1 || dayNumber > 7)
            throw new ArgumentException("Day number must be between 1 and 7.", nameof(dayNumber));

        return new AthleteWorkout(athleteProgramId, weekNumber, dayNumber, scheduledDate);
    }

    /// <summary>
    /// Reschedules the workout.
    /// </summary>
    public void Reschedule(DateTime newDate)
    {
        if (IsCompleted)
            throw new InvalidOperationException("Cannot reschedule a completed workout.");

        ScheduledDate = newDate;
    }

    /// <summary>
    /// Starts the workout session.
    /// </summary>
    public void Start()
    {
        if (IsCompleted)
            throw new InvalidOperationException("Cannot start a completed workout.");

        if (Status == WorkoutStatus.InProgress)
            throw new InvalidOperationException("Workout is already in progress.");

        if (Status == WorkoutStatus.Skipped)
            throw new InvalidOperationException("Cannot start a skipped workout.");

        Status = WorkoutStatus.InProgress;
        StartedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Skips the workout with an optional reason.
    /// </summary>
    public void Skip(string? reason = null)
    {
        if (IsCompleted)
            throw new InvalidOperationException("Cannot skip a completed workout.");

        if (Status == WorkoutStatus.InProgress)
            throw new InvalidOperationException("Cannot skip a workout that is in progress. Complete it as partial instead.");

        Status = WorkoutStatus.Skipped;
        SkippedReason = reason?.Trim();
    }

    /// <summary>
    /// Marks the workout as completed.
    /// </summary>
    public void Complete(
        int? durationMinutes = null,
        int? fatigueRating = null,
        string? notes = null)
    {
        if (IsCompleted)
            throw new InvalidOperationException("Workout is already completed.");

        if (fatigueRating.HasValue && (fatigueRating < 1 || fatigueRating > 10))
            throw new ArgumentException("Fatigue rating must be between 1 and 10.", nameof(fatigueRating));

        // Determine if all exercises were completed
        var allSetsCompleted = _exerciseLogs.Count > 0 && _exerciseLogs.All(l => l.IsCompleted);
        Status = allSetsCompleted ? WorkoutStatus.Completed : WorkoutStatus.PartiallyCompleted;

        IsCompleted = true;
        CompletedDate = DateTime.UtcNow;
        DurationMinutes = durationMinutes;
        FatigueRating = fatigueRating;
        Notes = notes?.Trim();
    }

    /// <summary>
    /// Adds an exercise log to the workout.
    /// </summary>
    public AthleteExerciseLog LogExercise(
        Guid exerciseId,
        int setNumber,
        int reps,
        decimal weight,
        decimal? rpe = null,
        string? notes = null,
        int? targetReps = null,
        decimal? targetWeight = null)
    {
        var log = AthleteExerciseLog.Create(Id, exerciseId, setNumber, reps, weight, rpe, notes, targetReps, targetWeight);
        _exerciseLogs.Add(log);
        return log;
    }

    /// <summary>
    /// Gets exercise logs for a specific exercise.
    /// </summary>
    public IEnumerable<AthleteExerciseLog> GetLogsForExercise(Guid exerciseId)
    {
        return _exerciseLogs.Where(l => l.ExerciseId == exerciseId).OrderBy(l => l.SetNumber);
    }
}
