using FluentValidation;

namespace CoachPlatform.Application.Features.Applications.Commands.CreateApplication;

/// <summary>
/// Validator for CreateApplicationCommand.
/// </summary>
public class CreateApplicationCommandValidator : AbstractValidator<CreateApplicationCommand>
{
    public CreateApplicationCommandValidator()
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

        RuleFor(x => x.Age)
            .InclusiveBetween(13, 100).WithMessage("Age must be between 13 and 100 years.")
            .When(x => x.Age.HasValue);

        RuleFor(x => x.Gender)
            .MaximumLength(20).WithMessage("Gender must not exceed 20 characters.")
            .When(x => !string.IsNullOrEmpty(x.Gender));

        RuleFor(x => x.Country)
            .MaximumLength(100).WithMessage("Country must not exceed 100 characters.")
            .When(x => !string.IsNullOrEmpty(x.Country));

        RuleFor(x => x.TrainingExperience)
            .MaximumLength(2000).WithMessage("Training experience must not exceed 2000 characters.")
            .When(x => !string.IsNullOrEmpty(x.TrainingExperience));

        RuleFor(x => x.CurrentSquat)
            .GreaterThanOrEqualTo(0).WithMessage("Squat must be a positive number.")
            .LessThanOrEqualTo(1000).WithMessage("Squat seems unrealistic. Maximum 1000kg.")
            .When(x => x.CurrentSquat.HasValue);

        RuleFor(x => x.CurrentBench)
            .GreaterThanOrEqualTo(0).WithMessage("Bench must be a positive number.")
            .LessThanOrEqualTo(500).WithMessage("Bench seems unrealistic. Maximum 500kg.")
            .When(x => x.CurrentBench.HasValue);

        RuleFor(x => x.CurrentDeadlift)
            .GreaterThanOrEqualTo(0).WithMessage("Deadlift must be a positive number.")
            .LessThanOrEqualTo(1000).WithMessage("Deadlift seems unrealistic. Maximum 1000kg.")
            .When(x => x.CurrentDeadlift.HasValue);

        RuleFor(x => x.Motivation)
            .MaximumLength(2000).WithMessage("Motivation must not exceed 2000 characters.")
            .When(x => !string.IsNullOrEmpty(x.Motivation));

        RuleFor(x => x.Goals)
            .MaximumLength(2000).WithMessage("Goals must not exceed 2000 characters.")
            .When(x => !string.IsNullOrEmpty(x.Goals));

        RuleFor(x => x.Message)
            .MaximumLength(2000).WithMessage("Message must not exceed 2000 characters.")
            .When(x => !string.IsNullOrEmpty(x.Message));

        RuleFor(x => x.ReferralSource)
            .MaximumLength(500).WithMessage("Referral source must not exceed 500 characters.")
            .When(x => !string.IsNullOrEmpty(x.ReferralSource));
    }
}
