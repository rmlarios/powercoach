using FluentValidation;

namespace CoachPlatform.Application.Features.Exercises.Commands.CreateExercise;

/// <summary>
/// Validator for CreateExerciseCommand.
/// </summary>
public class CreateExerciseCommandValidator : AbstractValidator<CreateExerciseCommand>
{
    public CreateExerciseCommandValidator()
    {
        RuleFor(x => x.CoachId)
            .NotEmpty().WithMessage("Coach ID is required.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Exercise name is required.")
            .MaximumLength(200).WithMessage("Exercise name must not exceed 200 characters.");

        RuleFor(x => x.Description)
            .MaximumLength(2000).WithMessage("Description must not exceed 2000 characters.")
            .When(x => !string.IsNullOrEmpty(x.Description));

        RuleFor(x => x.Category)
            .IsInEnum().WithMessage("Invalid exercise category.");

        RuleFor(x => x.PrimaryMuscleGroup)
            .IsInEnum().WithMessage("Invalid muscle group.");

        RuleFor(x => x.VideoUrl)
            .MaximumLength(500).WithMessage("Video URL must not exceed 500 characters.")
            .When(x => !string.IsNullOrEmpty(x.VideoUrl));

        RuleFor(x => x.Equipment)
            .MaximumLength(200).WithMessage("Equipment must not exceed 200 characters.")
            .When(x => !string.IsNullOrEmpty(x.Equipment));
    }
}
