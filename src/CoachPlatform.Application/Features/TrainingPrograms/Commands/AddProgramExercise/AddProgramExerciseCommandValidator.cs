using FluentValidation;

namespace CoachPlatform.Application.Features.TrainingPrograms.Commands.AddProgramExercise;

/// <summary>
/// Validator for AddProgramExerciseCommand.
/// </summary>
public class AddProgramExerciseCommandValidator : AbstractValidator<AddProgramExerciseCommand>
{
    public AddProgramExerciseCommandValidator()
    {
        RuleFor(x => x.CoachId)
            .NotEmpty().WithMessage("Coach ID is required.");

        RuleFor(x => x.DayTemplateId)
            .NotEmpty().WithMessage("Day template ID is required.");

        RuleFor(x => x.ExerciseId)
            .NotEmpty().WithMessage("Exercise ID is required.");

        RuleFor(x => x.Sets)
            .InclusiveBetween(1, 50).WithMessage("Sets must be between 1 and 50.");

        RuleFor(x => x.Reps)
            .NotEmpty().WithMessage("Reps prescription is required.")
            .MaximumLength(50).WithMessage("Reps prescription must not exceed 50 characters.");

        RuleFor(x => x.TargetRpe)
            .InclusiveBetween(1, 10).WithMessage("Target RPE must be between 1 and 10.")
            .When(x => x.TargetRpe.HasValue);

        RuleFor(x => x.RestSeconds)
            .InclusiveBetween(0, 600).WithMessage("Rest seconds must be between 0 and 600.")
            .When(x => x.RestSeconds.HasValue);

        RuleFor(x => x.Notes)
            .MaximumLength(1000).WithMessage("Notes must not exceed 1000 characters.")
            .When(x => !string.IsNullOrEmpty(x.Notes));
    }
}
