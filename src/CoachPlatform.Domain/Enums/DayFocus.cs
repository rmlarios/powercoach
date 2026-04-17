namespace CoachPlatform.Domain.Enums;

/// <summary>
/// Training day focus/emphasis for powerlifting and strength training programs.
/// </summary>
public enum DayFocus
{
    /// <summary>
    /// Squat-focused training day
    /// </summary>
    Squat = 1,

    /// <summary>
    /// Bench press-focused training day
    /// </summary>
    Bench = 2,

    /// <summary>
    /// Deadlift-focused training day
    /// </summary>
    Deadlift = 3,

    /// <summary>
    /// Hypertrophy/bodybuilding focused day
    /// </summary>
    Hypertrophy = 4,

    /// <summary>
    /// Upper body focused training day
    /// </summary>
    Upper = 5,

    /// <summary>
    /// Lower body focused training day
    /// </summary>
    Lower = 6,

    /// <summary>
    /// Full body training day
    /// </summary>
    FullBody = 7,

    /// <summary>
    /// Accessory work and weak point training
    /// </summary>
    Accessories = 8,

    /// <summary>
    /// Active recovery or deload day
    /// </summary>
    Recovery = 9,

    /// <summary>
    /// Competition simulation or peaking day
    /// </summary>
    Competition = 10,

    /// <summary>
    /// Push-focused training day (chest, shoulders, triceps)
    /// </summary>
    Push = 11,

    /// <summary>
    /// Pull-focused training day (back, biceps)
    /// </summary>
    Pull = 12,

    /// <summary>
    /// Legs-focused training day (quads, hamstrings, glutes, calves)
    /// </summary>
    Legs = 13,

    /// <summary>
    /// Rest day
    /// </summary>
    Rest = 14,

    /// <summary>
    /// Cardio-focused training day
    /// </summary>
    Cardio = 15,

    /// <summary>
    /// Custom focus defined by coach
    /// </summary>
    Custom = 99
}
