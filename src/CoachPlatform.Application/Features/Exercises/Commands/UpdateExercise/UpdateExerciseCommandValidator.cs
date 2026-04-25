using FluentValidation;

namespace CoachPlatform.Application.Features.Exercises.Commands.UpdateExercise;

public class UpdateExerciseCommandValidator : AbstractValidator<UpdateExerciseCommand>
{
    public UpdateExerciseCommandValidator()
    {
        RuleFor(x => x.ExerciseId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(2000);
        RuleFor(x => x.VideoUrl).MaximumLength(500);
        RuleFor(x => x.Equipment).MaximumLength(200);
    }
}
