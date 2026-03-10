using FluentValidation;

namespace CoachPlatform.Application.Features.Athletes.Commands.UpdateAthlete;

/// <summary>
/// Validator for UpdateAthleteCommand using FluentValidation.
/// </summary>
public class UpdateAthleteCommandValidator : AbstractValidator<UpdateAthleteCommand>
{
    public UpdateAthleteCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Athlete ID is required.");

        RuleFor(x => x.Country)
            .MaximumLength(100).WithMessage("Country must not exceed 100 characters.")
            .When(x => !string.IsNullOrEmpty(x.Country));

        RuleFor(x => x.Height)
            .GreaterThan(0).WithMessage("Height must be greater than 0.")
            .LessThanOrEqualTo(300).WithMessage("Height must not exceed 300 cm.")
            .When(x => x.Height.HasValue);

        RuleFor(x => x.Weight)
            .GreaterThan(0).WithMessage("Weight must be greater than 0.")
            .LessThanOrEqualTo(500).WithMessage("Weight must not exceed 500 kg.")
            .When(x => x.Weight.HasValue);

        RuleFor(x => x.ExperienceLevel)
            .MaximumLength(50).WithMessage("Experience level must not exceed 50 characters.")
            .When(x => !string.IsNullOrEmpty(x.ExperienceLevel));
    }
}
