using CoachPlatform.Domain.Common;

namespace CoachPlatform.Domain.Entities;

/// <summary>
/// WorkoutLog entity - represents a workout session logged by an athlete.
/// </summary>
public class WorkoutLog : AuditableEntity
{
    /// <summary>
    /// Reference to the athlete who performed this workout.
    /// </summary>
    public Guid AthleteId { get; private set; }

    /// <summary>
    /// Reference to the exercise performed.
    /// </summary>
    public Guid ExerciseId { get; private set; }

    /// <summary>
    /// Denormalized exercise name for quick reporting (synced from Exercise entity).
    /// </summary>
    public string ExerciseName { get; private set; } = null!;

    /// <summary>
    /// Number of sets performed.
    /// </summary>
    public int Sets { get; private set; }

    /// <summary>
    /// Number of repetitions per set.
    /// </summary>
    public int Reps { get; private set; }

    /// <summary>
    /// Weight used (in kg).
    /// </summary>
    public decimal? Weight { get; private set; }

    /// <summary>
    /// Rate of Perceived Exertion (1-10 scale).
    /// </summary>
    public int? RPE { get; private set; }

    /// <summary>
    /// Additional notes about the workout.
    /// </summary>
    public string? Notes { get; private set; }

    /// <summary>
    /// Date when the workout was performed.
    /// </summary>
    public DateTime WorkoutDate { get; private set; }

    /// <summary>
    /// Navigation property to the athlete.
    /// </summary>
    public virtual Athlete Athlete { get; private set; } = null!;

    /// <summary>
    /// Navigation property to the exercise.
    /// </summary>
    public virtual Exercise Exercise { get; private set; } = null!;

    // Private constructor for EF Core
    private WorkoutLog() { }

    /// <summary>
    /// Factory method to create a new WorkoutLog.
    /// </summary>
    public static WorkoutLog Create(
        Guid athleteId,
        Exercise exercise,
        int sets,
        int reps,
        DateTime workoutDate,
        decimal? weight = null,
        int? rpe = null,
        string? notes = null)
    {
        if (athleteId == Guid.Empty)
            throw new ArgumentException("Athlete ID cannot be empty.", nameof(athleteId));

        if (exercise is null)
            throw new ArgumentNullException(nameof(exercise));

        if (sets <= 0)
            throw new ArgumentException("Sets must be greater than 0.", nameof(sets));

        if (reps <= 0)
            throw new ArgumentException("Reps must be greater than 0.", nameof(reps));

        if (rpe.HasValue && (rpe < 1 || rpe > 10))
            throw new ArgumentException("RPE must be between 1 and 10.", nameof(rpe));

        var workoutLog = new WorkoutLog
        {
            AthleteId = athleteId,
            ExerciseId = exercise.Id,
            ExerciseName = exercise.Name,
            Sets = sets,
            Reps = reps,
            Weight = weight,
            RPE = rpe,
            Notes = notes?.Trim(),
            WorkoutDate = workoutDate.Date
        };

        return workoutLog;
    }

    /// <summary>
    /// Factory method to create a WorkoutLog using IDs (for migrations/legacy support).
    /// </summary>
    public static WorkoutLog CreateWithIds(
        Guid athleteId,
        Guid exerciseId,
        string exerciseName,
        int sets,
        int reps,
        DateTime workoutDate,
        decimal? weight = null,
        int? rpe = null,
        string? notes = null)
    {
        if (athleteId == Guid.Empty)
            throw new ArgumentException("Athlete ID cannot be empty.", nameof(athleteId));

        if (exerciseId == Guid.Empty)
            throw new ArgumentException("Exercise ID cannot be empty.", nameof(exerciseId));

        if (string.IsNullOrWhiteSpace(exerciseName))
            throw new ArgumentException("Exercise name cannot be empty.", nameof(exerciseName));

        if (sets <= 0)
            throw new ArgumentException("Sets must be greater than 0.", nameof(sets));

        if (reps <= 0)
            throw new ArgumentException("Reps must be greater than 0.", nameof(reps));

        if (rpe.HasValue && (rpe < 1 || rpe > 10))
            throw new ArgumentException("RPE must be between 1 and 10.", nameof(rpe));

        var workoutLog = new WorkoutLog
        {
            AthleteId = athleteId,
            ExerciseId = exerciseId,
            ExerciseName = exerciseName.Trim(),
            Sets = sets,
            Reps = reps,
            Weight = weight,
            RPE = rpe,
            Notes = notes?.Trim(),
            WorkoutDate = workoutDate.Date
        };

        return workoutLog;
    }

    /// <summary>
    /// Updates the exercise for this log.
    /// </summary>
    public void UpdateExercise(Exercise exercise, int sets, int reps)
    {
        if (exercise is null)
            throw new ArgumentNullException(nameof(exercise));

        if (sets <= 0)
            throw new ArgumentException("Sets must be greater than 0.", nameof(sets));

        if (reps <= 0)
            throw new ArgumentException("Reps must be greater than 0.", nameof(reps));

        ExerciseId = exercise.Id;
        ExerciseName = exercise.Name;
        Sets = sets;
        Reps = reps;
    }

    /// <summary>
    /// Sets the weight used.
    /// </summary>
    public void SetWeight(decimal? weight)
    {
        Weight = weight;
    }

    /// <summary>
    /// Sets the RPE (Rate of Perceived Exertion).
    /// </summary>
    public void SetRPE(int? rpe)
    {
        if (rpe.HasValue && (rpe < 1 || rpe > 10))
            throw new ArgumentException("RPE must be between 1 and 10.", nameof(rpe));

        RPE = rpe;
    }

    /// <summary>
    /// Sets the notes for this workout.
    /// </summary>
    public void SetNotes(string? notes)
    {
        Notes = notes?.Trim();
    }

    /// <summary>
    /// Calculates the total volume (sets x reps x weight).
    /// </summary>
    public decimal? GetTotalVolume()
    {
        if (!Weight.HasValue)
            return null;

        return Sets * Reps * Weight.Value;
    }
}
