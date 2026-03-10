using FluentValidation;

namespace CoachPlatform.Application.Features.WorkoutLogs.Commands.LogWorkout;

/// <summary>
/// Validator for LogWorkoutCommand using FluentValidation.
/// </summary>
public class LogWorkoutCommandValidator : AbstractValidator<LogWorkoutCommand>
{
    public LogWorkoutCommandValidator()
    {
        RuleFor(x => x.AthleteId)
            .NotEmpty().WithMessage("Athlete ID is required.");

        RuleFor(x => x.ExerciseId)
            .NotEmpty().WithMessage("Exercise ID is required.");

        RuleFor(x => x.Sets)
            .GreaterThan(0).WithMessage("Sets must be greater than 0.")
            .LessThanOrEqualTo(100).WithMessage("Sets must not exceed 100.");

        RuleFor(x => x.Reps)
            .GreaterThan(0).WithMessage("Reps must be greater than 0.")
            .LessThanOrEqualTo(1000).WithMessage("Reps must not exceed 1000.");

        RuleFor(x => x.Weight)
            .GreaterThan(0).WithMessage("Weight must be greater than 0.")
            .LessThanOrEqualTo(1000).WithMessage("Weight must not exceed 1000 kg.")
            .When(x => x.Weight.HasValue);

        RuleFor(x => x.RPE)
            .InclusiveBetween(1, 10).WithMessage("RPE must be between 1 and 10.")
            .When(x => x.RPE.HasValue);

        RuleFor(x => x.Notes)
            .MaximumLength(2000).WithMessage("Notes must not exceed 2000 characters.")
            .When(x => !string.IsNullOrEmpty(x.Notes));

        RuleFor(x => x.WorkoutDate)
            .NotEmpty().WithMessage("Workout date is required.");
    }
}
