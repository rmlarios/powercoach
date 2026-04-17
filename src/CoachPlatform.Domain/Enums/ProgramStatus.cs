namespace CoachPlatform.Domain.Enums;

/// <summary>
/// Status of an athlete's assigned program.
/// </summary>
public enum ProgramStatus
{
    /// <summary>
    /// Program is scheduled but hasn't started yet
    /// </summary>
    NotStarted = 1,

    /// <summary>
    /// Program is currently active and in progress
    /// </summary>
    Active = 2,

    /// <summary>
    /// Program has been paused temporarily
    /// </summary>
    Paused = 3,

    /// <summary>
    /// Program has been completed successfully
    /// </summary>
    Completed = 4,

    /// <summary>
    /// Program was cancelled before completion
    /// </summary>
    Cancelled = 5
}
