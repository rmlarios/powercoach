using FluentValidation;

namespace CoachPlatform.Application.Features.Subscriptions.Commands.CancelSubscription;

/// <summary>
/// Validator for CancelSubscriptionCommand using FluentValidation.
/// </summary>
public class CancelSubscriptionCommandValidator : AbstractValidator<CancelSubscriptionCommand>
{
    public CancelSubscriptionCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Subscription ID is required.");

        RuleFor(x => x.Reason)
            .MaximumLength(500).WithMessage("Cancellation reason must not exceed 500 characters.")
            .When(x => !string.IsNullOrEmpty(x.Reason));
    }
}
