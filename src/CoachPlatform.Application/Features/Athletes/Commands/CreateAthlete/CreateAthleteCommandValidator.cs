using FluentValidation;

namespace CoachPlatform.Application.Features.Athletes.Commands.CreateAthlete;

/// <summary>
/// Validator for CreateAthleteCommand using FluentValidation.
/// </summary>
public class CreateAthleteCommandValidator : AbstractValidator<CreateAthleteCommand>
{
    public CreateAthleteCommandValidator()
    {
        RuleFor(x => x.CoachId)
            .NotEmpty().WithMessage("Coach ID is required.");

        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required.")
            .MaximumLength(100).WithMessage("First name must not exceed 100 characters.");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last name is required.")
            .MaximumLength(100).WithMessage("Last name must not exceed 100 characters.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .MaximumLength(256).WithMessage("Email must not exceed 256 characters.")
            .EmailAddress().WithMessage("A valid email address is required.");

        RuleFor(x => x.Phone)
            .MaximumLength(20).WithMessage("Phone number must not exceed 20 characters.")
            .When(x => !string.IsNullOrEmpty(x.Phone));

        RuleFor(x => x.Goals)
            .MaximumLength(2000).WithMessage("Goals must not exceed 2000 characters.")
            .When(x => !string.IsNullOrEmpty(x.Goals));

        RuleFor(x => x.Country)
            .MaximumLength(100).WithMessage("Country must not exceed 100 characters.")
            .When(x => !string.IsNullOrEmpty(x.Country));

        RuleFor(x => x.Gender)
            .MaximumLength(20).WithMessage("Gender must not exceed 20 characters.")
            .When(x => !string.IsNullOrEmpty(x.Gender));

        RuleFor(x => x.DateOfBirth)
            .LessThan(DateTime.UtcNow).WithMessage("Date of birth must be in the past.")
            .GreaterThan(DateTime.UtcNow.AddYears(-120)).WithMessage("Date of birth is invalid.")
            .When(x => x.DateOfBirth.HasValue);

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
