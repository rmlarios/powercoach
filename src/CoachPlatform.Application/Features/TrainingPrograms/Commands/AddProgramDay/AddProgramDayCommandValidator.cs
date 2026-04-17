using CoachPlatform.Domain.Enums;
using FluentValidation;

namespace CoachPlatform.Application.Features.TrainingPrograms.Commands.AddProgramDay;

/// <summary>
/// Validator for AddProgramDayCommand.
/// </summary>
public class AddProgramDayCommandValidator : AbstractValidator<AddProgramDayCommand>
{
    public AddProgramDayCommandValidator()
    {
        RuleFor(x => x.CoachId)
            .NotEmpty().WithMessage("Coach ID is required.");

        RuleFor(x => x.WeekTemplateId)
            .NotEmpty().WithMessage("Week template ID is required.");

        RuleFor(x => x.DayNumber)
            .InclusiveBetween(1, 7).WithMessage("Day number must be between 1 and 7.");

        RuleFor(x => x.Focus)
            .IsInEnum().WithMessage("Invalid day focus value.");

        RuleFor(x => x.Notes)
            .MaximumLength(2000).WithMessage("Notes must not exceed 2000 characters.")
            .When(x => !string.IsNullOrEmpty(x.Notes));
    }
}
