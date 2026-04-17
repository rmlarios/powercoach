using FluentValidation;

namespace CoachPlatform.Application.Features.WorkoutLogs.Commands.StartWorkout;

/// <summary>
/// Validator for StartWorkoutCommand.
/// </summary>
public class StartWorkoutCommandValidator : AbstractValidator<StartWorkoutCommand>
{
    public StartWorkoutCommandValidator()
    {
        RuleFor(x => x.AthleteId)
            .NotEmpty().WithMessage("Athlete ID is required.");

        RuleFor(x => x.WorkoutId)
            .NotEmpty().WithMessage("Workout ID is required.");
    }
}
