using CoachPlatform.Domain.Common;
using CoachPlatform.Domain.Enums;
using CoachPlatform.Domain.ValueObjects;

namespace CoachPlatform.Domain.Entities;

/// <summary>
/// Subscription entity - represents the relationship between an athlete and a plan.
/// </summary>
public class Subscription : AuditableEntity
{
    /// <summary>
    /// Reference to the athlete.
    /// </summary>
    public Guid AthleteId { get; private set; }

    /// <summary>
    /// Reference to the plan.
    /// </summary>
    public Guid PlanId { get; private set; }

    /// <summary>
    /// Start date of the subscription.
    /// </summary>
    public DateTime StartDate { get; private set; }

    /// <summary>
    /// End date of the subscription.
    /// </summary>
    public DateTime EndDate { get; private set; }

    /// <summary>
    /// Current status of the subscription.
    /// </summary>
    public SubscriptionStatus Status { get; private set; }

    /// <summary>
    /// Price paid for this subscription (snapshot from plan at time of purchase).
    /// </summary>
    public Money Price { get; private set; } = null!;

    /// <summary>
    /// Whether the subscription auto-renews.
    /// </summary>
    public bool AutoRenew { get; private set; }

    /// <summary>
    /// Date when the subscription was cancelled (if applicable).
    /// </summary>
    public DateTime? CancelledAt { get; private set; }

    /// <summary>
    /// Reason for cancellation (if applicable).
    /// </summary>
    public string? CancellationReason { get; private set; }

    /// <summary>
    /// Notes about the subscription.
    /// </summary>
    public string? Notes { get; private set; }

    // Navigation properties
    public Athlete Athlete { get; private set; } = null!;
    public Plan Plan { get; private set; } = null!;

    private readonly List<Payment> _payments = [];
    public IReadOnlyCollection<Payment> Payments => _payments.AsReadOnly();

    // EF Core constructor
    private Subscription() { }

    private Subscription(
        Guid athleteId,
        Guid planId,
        DateTime startDate,
        DateTime endDate,
        Money price,
        bool autoRenew)
    {
        AthleteId = athleteId;
        PlanId = planId;
        StartDate = startDate;
        EndDate = endDate;
        Price = price;
        AutoRenew = autoRenew;
        Status = SubscriptionStatus.PendingPayment;
    }

    /// <summary>
    /// Creates a new Subscription.
    /// </summary>
    public static Subscription Create(
        Guid athleteId,
        Plan plan,
        DateTime? startDate = null,
        bool autoRenew = false)
    {
        if (athleteId == Guid.Empty)
            throw new ArgumentException("Athlete ID is required.", nameof(athleteId));

        if (plan is null)
            throw new ArgumentNullException(nameof(plan));

        if (!plan.IsActive)
            throw new InvalidOperationException("Cannot create subscription for inactive plan.");

        var start = startDate ?? DateTime.UtcNow;
        var end = start.AddDays(plan.DurationDays);

        return new Subscription(athleteId, plan.Id, start, end, plan.Price, autoRenew);
    }

    /// <summary>
    /// Activates the subscription after payment confirmation.
    /// </summary>
    public void Activate()
    {
        if (Status != SubscriptionStatus.PendingPayment && Status != SubscriptionStatus.Paused)
            throw new InvalidOperationException($"Cannot activate subscription with status {Status}.");

        Status = SubscriptionStatus.Active;
        SetUpdatedAt();
    }

    /// <summary>
    /// Pauses the subscription.
    /// </summary>
    public void Pause()
    {
        if (Status != SubscriptionStatus.Active)
            throw new InvalidOperationException("Can only pause active subscriptions.");

        Status = SubscriptionStatus.Paused;
        SetUpdatedAt();
    }

    /// <summary>
    /// Cancels the subscription.
    /// </summary>
    public void Cancel(string? reason = null)
    {
        if (Status == SubscriptionStatus.Cancelled || Status == SubscriptionStatus.Expired)
            throw new InvalidOperationException($"Cannot cancel subscription with status {Status}.");

        Status = SubscriptionStatus.Cancelled;
        CancelledAt = DateTime.UtcNow;
        CancellationReason = reason;
        AutoRenew = false;
        SetUpdatedAt();
    }

    /// <summary>
    /// Marks the subscription as expired.
    /// </summary>
    public void Expire()
    {
        if (Status == SubscriptionStatus.Cancelled)
            throw new InvalidOperationException("Cannot expire a cancelled subscription.");

        Status = SubscriptionStatus.Expired;
        SetUpdatedAt();
    }

    /// <summary>
    /// Extends the subscription end date.
    /// </summary>
    public void Extend(int days)
    {
        if (days <= 0)
            throw new ArgumentException("Days must be greater than 0.", nameof(days));

        EndDate = EndDate.AddDays(days);
        SetUpdatedAt();
    }

    /// <summary>
    /// Sets auto-renewal preference.
    /// </summary>
    public void SetAutoRenew(bool autoRenew)
    {
        AutoRenew = autoRenew;
        SetUpdatedAt();
    }

    /// <summary>
    /// Updates notes for the subscription.
    /// </summary>
    public void UpdateNotes(string? notes)
    {
        Notes = notes;
        SetUpdatedAt();
    }

    /// <summary>
    /// Checks if the subscription is currently active.
    /// </summary>
    public bool IsCurrentlyActive()
    {
        return Status == SubscriptionStatus.Active && 
               DateTime.UtcNow >= StartDate && 
               DateTime.UtcNow <= EndDate;
    }

    /// <summary>
    /// Gets the remaining days in the subscription.
    /// </summary>
    public int GetRemainingDays()
    {
        if (Status != SubscriptionStatus.Active)
            return 0;

        var remaining = (EndDate - DateTime.UtcNow).Days;
        return remaining > 0 ? remaining : 0;
    }
}
