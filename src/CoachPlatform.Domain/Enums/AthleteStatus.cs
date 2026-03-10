namespace CoachPlatform.Domain.Enums;

/// <summary>
/// Status of an athlete within the coach's program.
/// </summary>
public enum AthleteStatus
{
    /// <summary>
    /// Athlete is active in the program.
    /// </summary>
    Active = 0,

    /// <summary>
    /// Athlete is on hold/paused.
    /// </summary>
    OnHold = 1,

    /// <summary>
    /// Athlete has left the program.
    /// </summary>
    Inactive = 2,

    /// <summary>
    /// Athlete has been graduated/completed the program.
    /// </summary>
    Graduated = 3,

    /// <summary>
    /// Athlete has been suspended from the program.
    /// </summary>
    Suspended = 4
}
