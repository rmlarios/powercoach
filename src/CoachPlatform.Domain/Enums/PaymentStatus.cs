namespace CoachPlatform.Domain.Enums;

/// <summary>
/// Status of a payment.
/// </summary>
public enum PaymentStatus
{
    /// <summary>
    /// Payment initiated, awaiting completion.
    /// </summary>
    Pending = 0,

    /// <summary>
    /// Payment completed successfully.
    /// </summary>
    Completed = 1,

    /// <summary>
    /// Payment failed.
    /// </summary>
    Failed = 2,

    /// <summary>
    /// Payment was refunded.
    /// </summary>
    Refunded = 3,

    /// <summary>
    /// Payment was cancelled.
    /// </summary>
    Cancelled = 4
}
