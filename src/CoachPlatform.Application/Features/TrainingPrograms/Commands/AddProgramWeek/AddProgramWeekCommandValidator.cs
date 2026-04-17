using FluentValidation;

namespace CoachPlatform.Application.Features.TrainingPrograms.Commands.AddProgramWeek;

/// <summary>
/// Validator for AddProgramWeekCommand.
/// </summary>
public class AddProgramWeekCommandValidator : AbstractValidator<AddProgramWeekCommand>
{
    public AddProgramWeekCommandValidator()
    {
        RuleFor(x => x.CoachId)
            .NotEmpty().WithMessage("Coach ID is required.");

        RuleFor(x => x.ProgramTemplateId)
            .NotEmpty().WithMessage("Program template ID is required.");

        RuleFor(x => x.WeekNumber)
            .GreaterThanOrEqualTo(1).WithMessage("Week number must be at least 1.")
            .LessThanOrEqualTo(52).WithMessage("Week number must not exceed 52.");

        RuleFor(x => x.Notes)
            .MaximumLength(1000).WithMessage("Notes must not exceed 1000 characters.")
            .When(x => !string.IsNullOrEmpty(x.Notes));
    }
}
