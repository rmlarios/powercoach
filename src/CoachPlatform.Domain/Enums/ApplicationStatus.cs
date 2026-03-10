namespace CoachPlatform.Domain.Enums;

/// <summary>
/// Status of an application (postulación) to the coach's program.
/// </summary>
public enum ApplicationStatus
{
    /// <summary>
    /// Application received, pending review.
    /// </summary>
    Pending = 0,

    /// <summary>
    /// Application is being reviewed by the coach.
    /// </summary>
    UnderReview = 1,

    /// <summary>
    /// Application accepted - applicant becomes an athlete.
    /// </summary>
    Accepted = 2,

    /// <summary>
    /// Application rejected.
    /// </summary>
    Rejected = 3,

    /// <summary>
    /// Application withdrawn by the applicant.
    /// </summary>
    Withdrawn = 4
}
