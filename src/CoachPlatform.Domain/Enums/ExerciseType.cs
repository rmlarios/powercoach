namespace CoachPlatform.Domain.Enums;

/// <summary>
/// Type of exercise execution within a program prescription.
/// </summary>
public enum ExerciseType
{
    /// <summary>
    /// Standard straight-set execution.
    /// </summary>
    Standard = 0,

    /// <summary>
    /// Every Minute On the Minute — timed interval work.
    /// </summary>
    Emom = 1,

    /// <summary>
    /// Tempo-controlled execution with specified eccentric/concentric phases.
    /// </summary>
    Tempo = 2,

    /// <summary>
    /// Superset — paired exercises performed back-to-back.
    /// </summary>
    Superset = 3,

    /// <summary>
    /// Circuit — multiple exercises performed in sequence with minimal rest.
    /// </summary>
    Circuit = 4,

    /// <summary>
    /// Drop set — reducing weight each set without rest.
    /// </summary>
    Dropset = 5
}
