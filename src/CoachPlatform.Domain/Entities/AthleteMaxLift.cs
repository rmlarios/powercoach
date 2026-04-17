using CoachPlatform.Domain.Common;

namespace CoachPlatform.Domain.Entities;

/// <summary>
/// Represents an athlete's maximum lift (1RM) for a specific exercise.
/// Can be an actual tested max or an estimated max from training data.
/// </summary>
public class AthleteMaxLift : AuditableEntity
{
    /// <summary>
    /// Reference to the athlete.
    /// </summary>
    public Guid AthleteId { get; private set; }

    /// <summary>
    /// Reference to the exercise.
    /// </summary>
    public Guid ExerciseId { get; private set; }

    /// <summary>
    /// The 1RM weight in kilograms.
    /// </summary>
    public decimal Weight { get; private set; }

    /// <summary>
    /// Whether this is a tested (actual) max or estimated from training data.
    /// </summary>
    public bool IsTested { get; private set; }

    /// <summary>
    /// Date when the max was achieved or estimated.
    /// </summary>
    public DateTime RecordedAt { get; private set; }

    /// <summary>
    /// Optional notes about the max lift (e.g., "Competition PR", "Training max").
    /// </summary>
    public string? Notes { get; private set; }

    /// <summary>
    /// If estimated, this contains the calculation details (e.g., "225kg x 5 reps = 253kg e1RM").
    /// </summary>
    public string? EstimationDetails { get; private set; }

    // Navigation properties
    public Athlete Athlete { get; private set; } = null!;
    public Exercise Exercise { get; private set; } = null!;

    // EF Core constructor
    private AthleteMaxLift() { }

    private AthleteMaxLift(
        Guid athleteId,
        Guid exerciseId,
        decimal weight,
        bool isTested,
        DateTime recordedAt,
        string? notes,
        string? estimationDetails)
    {
        AthleteId = athleteId;
        ExerciseId = exerciseId;
        Weight = weight;
        IsTested = isTested;
        RecordedAt = recordedAt;
        Notes = notes;
        EstimationDetails = estimationDetails;
    }

    /// <summary>
    /// Creates a new tested (actual) max lift record.
    /// </summary>
    public static AthleteMaxLift CreateTested(
        Guid athleteId,
        Guid exerciseId,
        decimal weight,
        DateTime? recordedAt = null,
        string? notes = null)
    {
        return new AthleteMaxLift(
            athleteId,
            exerciseId,
            weight,
            isTested: true,
            recordedAt ?? DateTime.UtcNow,
            notes,
            estimationDetails: null);
    }

    /// <summary>
    /// Creates a new estimated max lift record.
    /// </summary>
    public static AthleteMaxLift CreateEstimated(
        Guid athleteId,
        Guid exerciseId,
        decimal weight,
        string estimationDetails,
        DateTime? recordedAt = null,
        string? notes = null)
    {
        return new AthleteMaxLift(
            athleteId,
            exerciseId,
            weight,
            isTested: false,
            recordedAt ?? DateTime.UtcNow,
            notes,
            estimationDetails);
    }

    /// <summary>
    /// Updates the max lift weight.
    /// </summary>
    public void UpdateWeight(decimal newWeight, bool isTested, string? notes = null, string? estimationDetails = null)
    {
        Weight = newWeight;
        IsTested = isTested;
        Notes = notes;
        EstimationDetails = estimationDetails;
        RecordedAt = DateTime.UtcNow;
    }
}

/// <summary>
/// Common 1RM estimation formulas.
/// </summary>
public static class OneRepMaxFormulas
{
    /// <summary>
    /// Epley formula: weight × (1 + reps/30)
    /// Best for reps ≤ 10
    /// </summary>
    public static decimal Epley(decimal weight, int reps)
    {
        if (reps <= 0) return weight;
        if (reps == 1) return weight;
        return weight * (1 + (decimal)reps / 30);
    }

    /// <summary>
    /// Brzycki formula: weight × 36 / (37 - reps)
    /// Most accurate for 1-10 reps
    /// </summary>
    public static decimal Brzycki(decimal weight, int reps)
    {
        if (reps <= 0) return weight;
        if (reps == 1) return weight;
        if (reps >= 37) return weight * 3; // Prevent division by zero/negative
        return weight * 36 / (37 - reps);
    }

    /// <summary>
    /// Calculate estimated 1RM using the average of Epley and Brzycki.
    /// </summary>
    public static decimal Average(decimal weight, int reps)
    {
        return (Epley(weight, reps) + Brzycki(weight, reps)) / 2;
    }

    /// <summary>
    /// Calculate weight for a target percentage of 1RM.
    /// </summary>
    /// <param name="oneRepMax">The 1RM weight</param>
    /// <param name="percentage">Target percentage (e.g., 75 for 75%)</param>
    /// <param name="roundTo">Round to nearest increment (e.g., 2.5 or 5)</param>
    public static decimal CalculateWorkingWeight(decimal oneRepMax, decimal percentage, decimal roundTo = 2.5m)
    {
        var targetWeight = oneRepMax * (percentage / 100);
        if (roundTo <= 0) return targetWeight;
        return Math.Round(targetWeight / roundTo) * roundTo;
    }
}
