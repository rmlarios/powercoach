using FluentValidation;

namespace CoachPlatform.Application.Features.WorkoutLogs.Commands.CompleteWorkout;

/// <summary>
/// Validator for CompleteWorkoutCommand.
/// </summary>
public class CompleteWorkoutCommandValidator : AbstractValidator<CompleteWorkoutCommand>
{
    public CompleteWorkoutCommandValidator()
    {
        RuleFor(x => x.AthleteId)
            .NotEmpty().WithMessage("Athlete ID is required.");

        RuleFor(x => x.WorkoutId)
            .NotEmpty().WithMessage("Workout ID is required.");

        RuleFor(x => x.DurationMinutes)
            .GreaterThan(0).WithMessage("Duration must be greater than 0.")
            .LessThanOrEqualTo(600).WithMessage("Duration must not exceed 600 minutes.")
            .When(x => x.DurationMinutes.HasValue);

        RuleFor(x => x.FatigueRating)
            .InclusiveBetween(1, 10).WithMessage("Fatigue rating must be between 1 and 10.")
            .When(x => x.FatigueRating.HasValue);

        RuleFor(x => x.Notes)
            .MaximumLength(2000).WithMessage("Notes must not exceed 2000 characters.")
            .When(x => !string.IsNullOrEmpty(x.Notes));
    }
}
