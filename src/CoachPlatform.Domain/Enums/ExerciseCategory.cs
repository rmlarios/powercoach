namespace CoachPlatform.Domain.Enums;

/// <summary>
/// Categories of exercises based on movement patterns.
/// </summary>
public enum ExerciseCategory
{
    /// <summary>
    /// Squat variations (Back Squat, Front Squat, etc.)
    /// </summary>
    Squat = 1,

    /// <summary>
    /// Bench press variations (Flat, Incline, Close Grip, etc.)
    /// </summary>
    Bench = 2,

    /// <summary>
    /// Deadlift variations (Conventional, Sumo, RDL, etc.)
    /// </summary>
    Deadlift = 3,

    /// <summary>
    /// Overhead pressing movements (OHP, Push Press, etc.)
    /// </summary>
    OverheadPress = 4,

    /// <summary>
    /// Rowing movements (Barbell Row, Cable Row, etc.)
    /// </summary>
    Row = 5,

    /// <summary>
    /// Pull-up and lat pulldown variations
    /// </summary>
    Pull = 6,

    /// <summary>
    /// Accessory exercises for specific muscle groups
    /// </summary>
    Accessory = 7,

    /// <summary>
    /// Cardiovascular exercises
    /// </summary>
    Cardio = 8,

    /// <summary>
    /// Mobility and flexibility work
    /// </summary>
    Mobility = 9,

    /// <summary>
    /// Core and abdominal exercises
    /// </summary>
    Core = 10
}
