using FluentValidation;

namespace CoachPlatform.Application.Features.TrainingPrograms.Commands.UpdateProgramTemplate;

/// <summary>
/// Validator for UpdateProgramTemplateCommand.
/// </summary>
public class UpdateProgramTemplateCommandValidator : AbstractValidator<UpdateProgramTemplateCommand>
{
    public UpdateProgramTemplateCommandValidator()
    {
        RuleFor(x => x.CoachId)
            .NotEmpty().WithMessage("Coach ID is required.");

        RuleFor(x => x.ProgramTemplateId)
            .NotEmpty().WithMessage("Program template ID is required.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Program name is required.")
            .MaximumLength(200).WithMessage("Program name must not exceed 200 characters.");

        RuleFor(x => x.DurationWeeks)
            .InclusiveBetween(1, 52).WithMessage("Duration must be between 1 and 52 weeks.");
    }
}
