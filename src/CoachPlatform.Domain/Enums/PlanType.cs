namespace CoachPlatform.Domain.Enums;

/// <summary>
/// Type of membership plan based on duration.
/// </summary>
public enum PlanType
{
    /// <summary>
    /// Monthly plan (30 days).
    /// </summary>
    Monthly = 0,

    /// <summary>
    /// Quarterly plan (90 days).
    /// </summary>
    Quarterly = 1,

    /// <summary>
    /// Semi-annual plan (180 days).
    /// </summary>
    SemiAnnual = 2,

    /// <summary>
    /// Annual plan (365 days).
    /// </summary>
    Annual = 3,

    /// <summary>
    /// Custom duration plan.
    /// </summary>
    Custom = 4
}
