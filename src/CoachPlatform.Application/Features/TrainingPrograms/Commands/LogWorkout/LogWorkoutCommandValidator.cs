using FluentValidation;

namespace CoachPlatform.Application.Features.TrainingPrograms.Commands.LogWorkout;

/// <summary>
/// Validator for LogWorkoutCommand.
/// </summary>
public class LogWorkoutCommandValidator : AbstractValidator<LogWorkoutCommand>
{
    public LogWorkoutCommandValidator()
    {
        RuleFor(x => x.WorkoutId)
            .NotEmpty().WithMessage("Workout ID is required.");

        RuleFor(x => x.DurationMinutes)
            .InclusiveBetween(1, 600).WithMessage("Duration must be between 1 and 600 minutes.")
            .When(x => x.DurationMinutes.HasValue);

        RuleFor(x => x.FatigueRating)
            .InclusiveBetween(1, 10).WithMessage("Fatigue rating must be between 1 and 10.")
            .When(x => x.FatigueRating.HasValue);

        RuleFor(x => x.Notes)
            .MaximumLength(2000).WithMessage("Notes must not exceed 2000 characters.")
            .When(x => !string.IsNullOrEmpty(x.Notes));

        RuleFor(x => x.ExerciseSets)
            .NotEmpty().WithMessage("At least one exercise set is required.");

        RuleForEach(x => x.ExerciseSets).ChildRules(set =>
        {
            set.RuleFor(s => s.ExerciseId)
                .NotEmpty().WithMessage("Exercise ID is required.");

            set.RuleFor(s => s.SetNumber)
                .InclusiveBetween(1, 50).WithMessage("Set number must be between 1 and 50.");

            set.RuleFor(s => s.Reps)
                .InclusiveBetween(0, 100).WithMessage("Reps must be between 0 and 100.");

            set.RuleFor(s => s.Weight)
                .InclusiveBetween(0, 1000).WithMessage("Weight must be between 0 and 1000 kg.");

            set.RuleFor(s => s.Rpe)
                .InclusiveBetween(1, 10).WithMessage("RPE must be between 1 and 10.")
                .When(s => s.Rpe.HasValue);

            set.RuleFor(s => s.Notes)
                .MaximumLength(500).WithMessage("Notes must not exceed 500 characters.")
                .When(s => !string.IsNullOrEmpty(s.Notes));
        });
    }
}
