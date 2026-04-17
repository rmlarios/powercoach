using FluentValidation;

namespace CoachPlatform.Application.Features.WorkoutLogs.Commands.CompleteSet;

/// <summary>
/// Validator for CompleteSetCommand.
/// </summary>
public class CompleteSetCommandValidator : AbstractValidator<CompleteSetCommand>
{
    public CompleteSetCommandValidator()
    {
        RuleFor(x => x.AthleteId)
            .NotEmpty().WithMessage("Athlete ID is required.");

        RuleFor(x => x.WorkoutId)
            .NotEmpty().WithMessage("Workout ID is required.");

        RuleFor(x => x.ExerciseLogId)
            .NotEmpty().WithMessage("Exercise Log ID is required.");
    }
}
