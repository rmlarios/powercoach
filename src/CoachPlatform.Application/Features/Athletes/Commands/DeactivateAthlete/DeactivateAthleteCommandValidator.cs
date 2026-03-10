using FluentValidation;

namespace CoachPlatform.Application.Features.Athletes.Commands.DeactivateAthlete;

/// <summary>
/// Validator for DeactivateAthleteCommand using FluentValidation.
/// </summary>
public class DeactivateAthleteCommandValidator : AbstractValidator<DeactivateAthleteCommand>
{
    public DeactivateAthleteCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Athlete ID is required.");
    }
}
