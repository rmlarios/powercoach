using CoachPlatform.Domain.Common;
using CoachPlatform.Domain.Enums;
using CoachPlatform.Domain.ValueObjects;

namespace CoachPlatform.Domain.Entities;

/// <summary>
/// Payment entity - represents a payment made for a subscription.
/// </summary>
public class Payment : AuditableEntity
{
    /// <summary>
    /// Reference to the subscription this payment is for.
    /// </summary>
    public Guid SubscriptionId { get; private set; }

    /// <summary>
    /// Amount of the payment.
    /// </summary>
    public Money Amount { get; private set; } = null!;

    /// <summary>
    /// Date when the payment was made.
    /// </summary>
    public DateTime PaymentDate { get; private set; }

    /// <summary>
    /// Current status of the payment.
    /// </summary>
    public PaymentStatus Status { get; private set; }

    /// <summary>
    /// External payment reference (from payment gateway).
    /// </summary>
    public string? ExternalReference { get; private set; }

    /// <summary>
    /// Payment method used (e.g., "credit_card", "bank_transfer", "paypal").
    /// </summary>
    public string? PaymentMethod { get; private set; }

    /// <summary>
    /// Transaction ID from the payment gateway.
    /// </summary>
    public string? TransactionId { get; private set; }

    /// <summary>
    /// Notes about the payment.
    /// </summary>
    public string? Notes { get; private set; }

    /// <summary>
    /// Date when the payment was confirmed.
    /// </summary>
    public DateTime? ConfirmedAt { get; private set; }

    /// <summary>
    /// Date when the payment was refunded (if applicable).
    /// </summary>
    public DateTime? RefundedAt { get; private set; }

    /// <summary>
    /// Reason for refund (if applicable).
    /// </summary>
    public string? RefundReason { get; private set; }

    /// <summary>
    /// Failure reason (if payment failed).
    /// </summary>
    public string? FailureReason { get; private set; }

    // Navigation property
    public Subscription Subscription { get; private set; } = null!;

    // EF Core constructor
    private Payment() { }

    private Payment(
        Guid subscriptionId,
        Money amount,
        string? paymentMethod,
        string? externalReference)
    {
        SubscriptionId = subscriptionId;
        Amount = amount;
        PaymentDate = DateTime.UtcNow;
        PaymentMethod = paymentMethod;
        ExternalReference = externalReference;
        Status = PaymentStatus.Pending;
    }

    /// <summary>
    /// Creates a new Payment.
    /// </summary>
    public static Payment Create(
        Guid subscriptionId,
        decimal amount,
        string currency,
        string? paymentMethod = null,
        string? externalReference = null)
    {
        if (subscriptionId == Guid.Empty)
            throw new ArgumentException("Subscription ID is required.", nameof(subscriptionId));

        var amountVo = Money.Create(amount, currency);

        return new Payment(subscriptionId, amountVo, paymentMethod, externalReference);
    }

    /// <summary>
    /// Creates a Payment from a Subscription.
    /// </summary>
    public static Payment CreateFromSubscription(
        Subscription subscription,
        string? paymentMethod = null,
        string? externalReference = null)
    {
        if (subscription is null)
            throw new ArgumentNullException(nameof(subscription));

        return new Payment(subscription.Id, subscription.Price, paymentMethod, externalReference);
    }

    /// <summary>
    /// Marks the payment as completed.
    /// </summary>
    public void Complete(string? transactionId = null)
    {
        if (Status != PaymentStatus.Pending)
            throw new InvalidOperationException($"Cannot complete payment with status {Status}.");

        Status = PaymentStatus.Completed;
        TransactionId = transactionId;
        ConfirmedAt = DateTime.UtcNow;
        SetUpdatedAt();
    }

    /// <summary>
    /// Marks the payment as failed.
    /// </summary>
    public void Fail(string? reason = null)
    {
        if (Status != PaymentStatus.Pending)
            throw new InvalidOperationException($"Cannot fail payment with status {Status}.");

        Status = PaymentStatus.Failed;
        FailureReason = reason;
        SetUpdatedAt();
    }

    /// <summary>
    /// Refunds the payment.
    /// </summary>
    public void Refund(string? reason = null)
    {
        if (Status != PaymentStatus.Completed)
            throw new InvalidOperationException("Can only refund completed payments.");

        Status = PaymentStatus.Refunded;
        RefundedAt = DateTime.UtcNow;
        RefundReason = reason;
        SetUpdatedAt();
    }

    /// <summary>
    /// Cancels the payment.
    /// </summary>
    public void Cancel()
    {
        if (Status != PaymentStatus.Pending)
            throw new InvalidOperationException("Can only cancel pending payments.");

        Status = PaymentStatus.Cancelled;
        SetUpdatedAt();
    }

    /// <summary>
    /// Sets the transaction ID.
    /// </summary>
    public void SetTransactionId(string transactionId)
    {
        TransactionId = transactionId;
        SetUpdatedAt();
    }

    /// <summary>
    /// Updates notes for the payment.
    /// </summary>
    public void UpdateNotes(string? notes)
    {
        Notes = notes;
        SetUpdatedAt();
    }
}
