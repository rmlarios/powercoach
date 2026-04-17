using FluentValidation;

namespace CoachPlatform.Application.Features.WorkoutLogs.Commands.SaveSet;

/// <summary>
/// Validator for SaveSetCommand.
/// </summary>
public class SaveSetCommandValidator : AbstractValidator<SaveSetCommand>
{
    public SaveSetCommandValidator()
    {
        RuleFor(x => x.AthleteId)
            .NotEmpty().WithMessage("Athlete ID is required.");

        RuleFor(x => x.WorkoutId)
            .NotEmpty().WithMessage("Workout ID is required.");

        RuleFor(x => x.ExerciseId)
            .NotEmpty().WithMessage("Exercise ID is required.");

        RuleFor(x => x.SetNumber)
            .InclusiveBetween(1, 50).WithMessage("Set number must be between 1 and 50.");

        RuleFor(x => x.Reps)
            .GreaterThanOrEqualTo(0).WithMessage("Reps must be 0 or greater.")
            .LessThanOrEqualTo(100).WithMessage("Reps must not exceed 100.");

        RuleFor(x => x.Weight)
            .GreaterThanOrEqualTo(0).WithMessage("Weight must be 0 or greater.")
            .LessThanOrEqualTo(1000).WithMessage("Weight must not exceed 1000 kg.");

        RuleFor(x => x.Rpe)
            .InclusiveBetween(1, 10).WithMessage("RPE must be between 1 and 10.")
            .When(x => x.Rpe.HasValue);

        RuleFor(x => x.Notes)
            .MaximumLength(500).WithMessage("Notes must not exceed 500 characters.")
            .When(x => !string.IsNullOrEmpty(x.Notes));
    }
}
