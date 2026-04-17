using FluentValidation;

namespace CoachPlatform.Application.Features.TrainingPrograms.Commands.AssignProgramToAthlete;

/// <summary>
/// Validator for AssignProgramToAthleteCommand.
/// </summary>
public class AssignProgramToAthleteCommandValidator : AbstractValidator<AssignProgramToAthleteCommand>
{
    public AssignProgramToAthleteCommandValidator()
    {
        RuleFor(x => x.CoachId)
            .NotEmpty().WithMessage("Coach ID is required.");

        RuleFor(x => x.AthleteId)
            .NotEmpty().WithMessage("Athlete ID is required.");

        RuleFor(x => x.ProgramTemplateId)
            .NotEmpty().WithMessage("Program template ID is required.");

        RuleFor(x => x.StartDate)
            .NotEmpty().WithMessage("Start date is required.")
            .GreaterThanOrEqualTo(DateTime.UtcNow.Date.AddDays(-7))
            .WithMessage("Start date cannot be more than 7 days in the past.");

        RuleFor(x => x.Notes)
            .MaximumLength(2000).WithMessage("Notes must not exceed 2000 characters.")
            .When(x => !string.IsNullOrEmpty(x.Notes));
    }
}
