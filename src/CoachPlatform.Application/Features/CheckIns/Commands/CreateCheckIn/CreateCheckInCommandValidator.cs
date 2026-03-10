using FluentValidation;

namespace CoachPlatform.Application.Features.CheckIns.Commands.CreateCheckIn;

/// <summary>
/// Validator for CreateCheckInCommand using FluentValidation.
/// </summary>
public class CreateCheckInCommandValidator : AbstractValidator<CreateCheckInCommand>
{
    public CreateCheckInCommandValidator()
    {
        RuleFor(x => x.AthleteId)
            .NotEmpty().WithMessage("Athlete ID is required.");

        RuleFor(x => x.Weight)
            .GreaterThan(0).WithMessage("Weight must be greater than 0.")
            .LessThanOrEqualTo(500).WithMessage("Weight must not exceed 500 kg.")
            .When(x => x.Weight.HasValue);

        RuleFor(x => x.FatigueLevel)
            .InclusiveBetween(1, 10).WithMessage("Fatigue level must be between 1 and 10.")
            .When(x => x.FatigueLevel.HasValue);

        RuleFor(x => x.SleepQuality)
            .InclusiveBetween(1, 10).WithMessage("Sleep quality must be between 1 and 10.")
            .When(x => x.SleepQuality.HasValue);

        RuleFor(x => x.MotivationLevel)
            .InclusiveBetween(1, 10).WithMessage("Motivation level must be between 1 and 10.")
            .When(x => x.MotivationLevel.HasValue);

        RuleFor(x => x.Notes)
            .MaximumLength(2000).WithMessage("Notes must not exceed 2000 characters.")
            .When(x => !string.IsNullOrEmpty(x.Notes));
    }
}
