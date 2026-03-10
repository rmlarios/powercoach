using FluentValidation;

namespace CoachPlatform.Application.Features.Payments.Commands.RegisterPayment;

/// <summary>
/// Validator for RegisterPaymentCommand using FluentValidation.
/// </summary>
public class RegisterPaymentCommandValidator : AbstractValidator<RegisterPaymentCommand>
{
    private static readonly string[] ValidCurrencies = ["USD", "EUR", "GBP", "MXN", "CAD", "AUD"];
    private static readonly string[] ValidPaymentMethods = ["cash", "credit_card", "debit_card", "bank_transfer", "paypal", "stripe", "other"];

    public RegisterPaymentCommandValidator()
    {
        RuleFor(x => x.AthleteId)
            .NotEmpty().WithMessage("Athlete ID is required.");

        RuleFor(x => x.SubscriptionId)
            .NotEmpty().WithMessage("Subscription ID is required.");

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Amount must be greater than 0.");

        RuleFor(x => x.Currency)
            .NotEmpty().WithMessage("Currency is required.")
            .Must(c => ValidCurrencies.Contains(c.ToUpperInvariant()))
            .WithMessage($"Currency must be one of: {string.Join(", ", ValidCurrencies)}");

        RuleFor(x => x.PaymentMethod)
            .Must(m => string.IsNullOrEmpty(m) || ValidPaymentMethods.Contains(m.ToLowerInvariant()))
            .WithMessage($"Payment method must be one of: {string.Join(", ", ValidPaymentMethods)}")
            .When(x => !string.IsNullOrEmpty(x.PaymentMethod));

        RuleFor(x => x.PaymentDate)
            .NotEmpty().WithMessage("Payment date is required.")
            .LessThanOrEqualTo(DateTime.UtcNow.AddDays(1))
            .WithMessage("Payment date cannot be in the future.");

        RuleFor(x => x.Notes)
            .MaximumLength(500).WithMessage("Notes must not exceed 500 characters.")
            .When(x => !string.IsNullOrEmpty(x.Notes));
    }
}
