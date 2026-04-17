using FluentValidation;

namespace CoachPlatform.Application.Features.TrainingPrograms.Commands.SaveProgramTemplate;

/// <summary>
/// Validator for SaveProgramTemplateCommand.
/// </summary>
public class SaveProgramTemplateCommandValidator : AbstractValidator<SaveProgramTemplateCommand>
{
    public SaveProgramTemplateCommandValidator()
    {
        RuleFor(x => x.CoachId)
            .NotEmpty().WithMessage("Coach ID is required.");

        RuleFor(x => x.ProgramTemplateId)
            .NotEmpty().WithMessage("Program template ID is required.");

        RuleFor(x => x.Data)
            .NotNull().WithMessage("Program data is required.");

        RuleFor(x => x.Data.Name)
            .NotEmpty().WithMessage("Program name is required.")
            .MaximumLength(200).WithMessage("Program name must not exceed 200 characters.")
            .When(x => x.Data != null);

        RuleFor(x => x.Data.DurationWeeks)
            .InclusiveBetween(1, 52).WithMessage("Duration must be between 1 and 52 weeks.")
            .When(x => x.Data != null);
    }
}
