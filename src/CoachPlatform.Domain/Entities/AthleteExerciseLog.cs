using CoachPlatform.Domain.Common;

namespace CoachPlatform.Domain.Entities;

/// <summary>
/// AthleteExerciseLog entity - represents the actual performance of an exercise by an athlete.
/// </summary>
public class AthleteExerciseLog : AuditableEntity
{
    /// <summary>
    /// Reference to the parent workout.
    /// </summary>
    public Guid WorkoutId { get; private set; }

    /// <summary>
    /// Reference to the exercise performed.
    /// </summary>
    public Guid ExerciseId { get; private set; }

    /// <summary>
    /// Set number (1-based).
    /// </summary>
    public int SetNumber { get; private set; }

    /// <summary>
    /// Target number of reps for this set (from program template).
    /// </summary>
    public int? TargetReps { get; private set; }

    /// <summary>
    /// Target weight for this set (from program template).
    /// </summary>
    public decimal? TargetWeight { get; private set; }

    /// <summary>
    /// Number of reps performed.
    /// </summary>
    public int Reps { get; private set; }

    /// <summary>
    /// Weight used in kilograms.
    /// </summary>
    public decimal Weight { get; private set; }

    /// <summary>
    /// Rate of Perceived Exertion (1-10 scale).
    /// </summary>
    public decimal? Rpe { get; private set; }

    /// <summary>
    /// Whether this set has been completed.
    /// </summary>
    public bool IsCompleted { get; private set; }

    /// <summary>
    /// Reason for skipping this set (if skipped).
    /// </summary>
    public string? SkippedReason { get; private set; }

    /// <summary>
    /// Additional notes about the set.
    /// </summary>
    public string? Notes { get; private set; }

    // Navigation properties
    public AthleteWorkout Workout { get; private set; } = null!;
    public Exercise Exercise { get; private set; } = null!;

    // EF Core constructor
    private AthleteExerciseLog() { }

    private AthleteExerciseLog(
        Guid workoutId,
        Guid exerciseId,
        int setNumber,
        int? targetReps,
        decimal? targetWeight,
        int reps,
        decimal weight,
        decimal? rpe,
        string? notes)
    {
        WorkoutId = workoutId;
        ExerciseId = exerciseId;
        SetNumber = setNumber;
        TargetReps = targetReps;
        TargetWeight = targetWeight;
        Reps = reps;
        Weight = weight;
        Rpe = rpe;
        Notes = notes;
        IsCompleted = false;
    }

    /// <summary>
    /// Creates a new exercise log entry.
    /// </summary>
    internal static AthleteExerciseLog Create(
        Guid workoutId,
        Guid exerciseId,
        int setNumber,
        int reps,
        decimal weight,
        decimal? rpe = null,
        string? notes = null,
        int? targetReps = null,
        decimal? targetWeight = null)
    {
        if (workoutId == Guid.Empty)
            throw new ArgumentException("Workout ID is required.", nameof(workoutId));

        if (exerciseId == Guid.Empty)
            throw new ArgumentException("Exercise ID is required.", nameof(exerciseId));

        if (setNumber < 1 || setNumber > 50)
            throw new ArgumentException("Set number must be between 1 and 50.", nameof(setNumber));

        if (reps < 0 || reps > 100)
            throw new ArgumentException("Reps must be between 0 and 100.", nameof(reps));

        if (weight < 0 || weight > 1000)
            throw new ArgumentException("Weight must be between 0 and 1000 kg.", nameof(weight));

        if (rpe.HasValue && (rpe < 1 || rpe > 10))
            throw new ArgumentException("RPE must be between 1 and 10.", nameof(rpe));

        return new AthleteExerciseLog(workoutId, exerciseId, setNumber, targetReps, targetWeight, reps, weight, rpe, notes?.Trim());
    }

    /// <summary>
    /// Updates the exercise log entry.
    /// </summary>
    public void Update(int reps, decimal weight, decimal? rpe, string? notes)
    {
        if (reps < 0 || reps > 100)
            throw new ArgumentException("Reps must be between 0 and 100.", nameof(reps));

        if (weight < 0 || weight > 1000)
            throw new ArgumentException("Weight must be between 0 and 1000 kg.", nameof(weight));

        if (rpe.HasValue && (rpe < 1 || rpe > 10))
            throw new ArgumentException("RPE must be between 1 and 10.", nameof(rpe));

        Reps = reps;
        Weight = weight;
        Rpe = rpe;
        Notes = notes?.Trim();
    }

    /// <summary>
    /// Marks this set as completed.
    /// </summary>
    public void Complete()
    {
        if (Reps <= 0)
            throw new InvalidOperationException("Cannot complete a set with 0 reps.");

        IsCompleted = true;
        SkippedReason = null;
    }

    /// <summary>
    /// Skips this set with an optional reason.
    /// </summary>
    public void Skip(string? reason = null)
    {
        IsCompleted = false;
        SkippedReason = reason?.Trim();
    }

    /// <summary>
    /// Sets the target values from the program template.
    /// </summary>
    public void SetTargets(int? targetReps, decimal? targetWeight)
    {
        TargetReps = targetReps;
        TargetWeight = targetWeight;
    }
}
