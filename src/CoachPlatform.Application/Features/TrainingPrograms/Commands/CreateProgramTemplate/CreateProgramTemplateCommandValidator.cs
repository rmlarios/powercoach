using FluentValidation;

namespace CoachPlatform.Application.Features.TrainingPrograms.Commands.CreateProgramTemplate;

/// <summary>
/// Validator for CreateProgramTemplateCommand.
/// </summary>
public class CreateProgramTemplateCommandValidator : AbstractValidator<CreateProgramTemplateCommand>
{
    public CreateProgramTemplateCommandValidator()
    {
        RuleFor(x => x.CoachId)
            .NotEmpty().WithMessage("Coach ID is required.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Program name is required.")
            .MaximumLength(200).WithMessage("Program name must not exceed 200 characters.");

        RuleFor(x => x.Description)
            .MaximumLength(2000).WithMessage("Description must not exceed 2000 characters.")
            .When(x => !string.IsNullOrEmpty(x.Description));

        RuleFor(x => x.DurationWeeks)
            .GreaterThanOrEqualTo(1).WithMessage("Duration must be at least 1 week.")
            .LessThanOrEqualTo(52).WithMessage("Duration must not exceed 52 weeks.");

        When(x => x.DaysPerWeek.HasValue, () =>
        {
            RuleFor(x => x.DaysPerWeek!.Value)
                .InclusiveBetween(1, 7).WithMessage("Days per week must be between 1 and 7.");
        });
    }
}
