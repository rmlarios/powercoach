using FluentValidation;

namespace CoachPlatform.Application.Features.Subscriptions.Commands.CreateSubscription;

/// <summary>
/// Validator for CreateSubscriptionCommand using FluentValidation.
/// </summary>
public class CreateSubscriptionCommandValidator : AbstractValidator<CreateSubscriptionCommand>
{
    public CreateSubscriptionCommandValidator()
    {
        RuleFor(x => x.AthleteId)
            .NotEmpty().WithMessage("Athlete ID is required.");

        RuleFor(x => x.PlanId)
            .NotEmpty().WithMessage("Plan ID is required.");

        RuleFor(x => x.StartDate)
            .NotEmpty().WithMessage("Start date is required.")
            .GreaterThanOrEqualTo(DateTime.UtcNow.Date)
            .WithMessage("Start date cannot be in the past.");
    }
}
