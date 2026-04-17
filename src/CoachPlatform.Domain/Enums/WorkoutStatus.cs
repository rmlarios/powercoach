namespace CoachPlatform.Domain.Enums;

/// <summary>
/// Represents the status of an athlete's workout session.
/// </summary>
public enum WorkoutStatus
{
    /// <summary>
    /// Workout has not been started yet.
    /// </summary>
    NotStarted = 0,

    /// <summary>
    /// Workout is currently in progress.
    /// </summary>
    InProgress = 1,

    /// <summary>
    /// Workout has been completed successfully.
    /// </summary>
    Completed = 2,

    /// <summary>
    /// Workout was skipped entirely.
    /// </summary>
    Skipped = 3,

    /// <summary>
    /// Workout was started but not all exercises were completed.
    /// </summary>
    PartiallyCompleted = 4
}
