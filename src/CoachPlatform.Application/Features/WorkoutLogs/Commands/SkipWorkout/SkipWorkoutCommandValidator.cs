using FluentValidation;

namespace CoachPlatform.Application.Features.WorkoutLogs.Commands.SkipWorkout;

/// <summary>
/// Validator for SkipWorkoutCommand.
/// </summary>
public class SkipWorkoutCommandValidator : AbstractValidator<SkipWorkoutCommand>
{
    public SkipWorkoutCommandValidator()
    {
        RuleFor(x => x.AthleteId)
            .NotEmpty().WithMessage("Athlete ID is required.");

        RuleFor(x => x.WorkoutId)
            .NotEmpty().WithMessage("Workout ID is required.");

        RuleFor(x => x.Reason)
            .MaximumLength(500).WithMessage("Reason must not exceed 500 characters.")
            .When(x => !string.IsNullOrEmpty(x.Reason));
    }
}
