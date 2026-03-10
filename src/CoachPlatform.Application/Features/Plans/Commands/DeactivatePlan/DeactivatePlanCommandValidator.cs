using FluentValidation;

namespace CoachPlatform.Application.Features.Plans.Commands.DeactivatePlan;

/// <summary>
/// Validator for DeactivatePlanCommand using FluentValidation.
/// </summary>
public class DeactivatePlanCommandValidator : AbstractValidator<DeactivatePlanCommand>
{
    public DeactivatePlanCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Plan ID is required.");
    }
}
