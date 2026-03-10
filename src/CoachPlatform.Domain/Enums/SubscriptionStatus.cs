namespace CoachPlatform.Domain.Enums;

/// <summary>
/// Status of an athlete's subscription.
/// </summary>
public enum SubscriptionStatus
{
    /// <summary>
    /// Subscription is active and in good standing.
    /// </summary>
    Active = 0,

    /// <summary>
    /// Subscription is temporarily paused.
    /// </summary>
    Paused = 1,

    /// <summary>
    /// Subscription was cancelled by the athlete or coach.
    /// </summary>
    Cancelled = 2,

    /// <summary>
    /// Subscription has expired (end date passed).
    /// </summary>
    Expired = 3,

    /// <summary>
    /// Subscription is pending payment confirmation.
    /// </summary>
    PendingPayment = 4
}
