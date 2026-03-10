using CoachPlatform.Domain.Enums;

namespace CoachPlatform.Application.Shared.DTOs;

/// <summary>
/// Data Transfer Object for Payment entity.
/// </summary>
public record PaymentDto
{
    public Guid Id { get; init; }
    public Guid AthleteId { get; init; }
    public string AthleteName { get; init; } = null!;
    public Guid? SubscriptionId { get; init; }
    public decimal Amount { get; init; }
    public string Currency { get; init; } = null!;
    public PaymentStatus Status { get; init; }
    public DateTime PaymentDate { get; init; }
    public DateTime? ProcessedAt { get; init; }
    public string? PaymentMethod { get; init; }
    public string? TransactionReference { get; init; }
    public string? FailureReason { get; init; }
    public string? Notes { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}

/// <summary>
/// DTO for recording a new payment.
/// </summary>
public record CreatePaymentDto
{
    public Guid AthleteId { get; init; }
    public Guid? SubscriptionId { get; init; }
    public decimal Amount { get; init; }
    public string Currency { get; init; } = null!;
    public string? PaymentMethod { get; init; }
    public string? TransactionReference { get; init; }
    public string? Notes { get; init; }
}

/// <summary>
/// DTO for confirming a payment.
/// </summary>
public record ConfirmPaymentDto
{
    public string? TransactionReference { get; init; }
    public string? PaymentMethod { get; init; }
}

/// <summary>
/// DTO for cancelling/failing a payment.
/// </summary>
public record FailPaymentDto
{
    public string Reason { get; init; } = null!;
}

/// <summary>
/// Lightweight DTO for listing payments.
/// </summary>
public record PaymentListItemDto
{
    public Guid Id { get; init; }
    public string AthleteName { get; init; } = null!;
    public decimal Amount { get; init; }
    public string Currency { get; init; } = null!;
    public PaymentStatus Status { get; init; }
    public DateTime PaymentDate { get; init; }
    public string? PaymentMethod { get; init; }
}

/// <summary>
/// DTO for payment summary/analytics.
/// </summary>
public record PaymentSummaryDto
{
    public decimal TotalReceived { get; init; }
    public decimal TotalPending { get; init; }
    public int CompletedCount { get; init; }
    public int PendingCount { get; init; }
    public int FailedCount { get; init; }
    public string Currency { get; init; } = null!;
}
