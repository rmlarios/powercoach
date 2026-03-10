using FluentValidation;

namespace CoachPlatform.Application.Features.Plans.Commands.CreatePlan;

/// <summary>
/// Validator for CreatePlanCommand using FluentValidation.
/// </summary>
public class CreatePlanCommandValidator : AbstractValidator<CreatePlanCommand>
{
    private static readonly string[] ValidCurrencies = ["USD", "EUR", "GBP", "MXN", "CAD", "AUD"];

    public CreatePlanCommandValidator()
    {
        RuleFor(x => x.CoachId)
            .NotEmpty().WithMessage("Coach ID is required.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Plan name is required.")
            .MaximumLength(100).WithMessage("Plan name must not exceed 100 characters.");

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Description must not exceed 1000 characters.")
            .When(x => !string.IsNullOrEmpty(x.Description));

        RuleFor(x => x.Price)
            .GreaterThan(0).WithMessage("Price must be greater than 0.");

        RuleFor(x => x.Currency)
            .NotEmpty().WithMessage("Currency is required.")
            .Must(c => ValidCurrencies.Contains(c.ToUpperInvariant()))
            .WithMessage($"Currency must be one of: {string.Join(", ", ValidCurrencies)}");

        RuleFor(x => x.DurationInMonths)
            .GreaterThan(0).WithMessage("Duration must be greater than 0 months.")
            .LessThanOrEqualTo(24).WithMessage("Duration must not exceed 24 months.");
    }
}
