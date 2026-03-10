using FluentValidation;

namespace CoachPlatform.Application.Features.TrainingCycles.Commands.CreateTrainingCycle;

/// <summary>
/// Validator for CreateTrainingCycleCommand using FluentValidation.
/// </summary>
public class CreateTrainingCycleCommandValidator : AbstractValidator<CreateTrainingCycleCommand>
{
    public CreateTrainingCycleCommandValidator()
    {
        RuleFor(x => x.AthleteId)
            .NotEmpty().WithMessage("Athlete ID is required.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(200).WithMessage("Name must not exceed 200 characters.");

        RuleFor(x => x.DurationWeeks)
            .GreaterThan(0).WithMessage("Duration must be greater than 0.")
            .LessThanOrEqualTo(52).WithMessage("Duration must not exceed 52 weeks.");

        RuleFor(x => x.StartDate)
            .NotEmpty().WithMessage("Start date is required.");
    }
}
