using FluentValidation;

namespace CoachPlatform.Application.Features.TrainingPrograms.Commands.BulkAssignProgramToAthletes;

public class BulkAssignProgramToAthletesCommandValidator : AbstractValidator<BulkAssignProgramToAthletesCommand>
{
    public BulkAssignProgramToAthletesCommandValidator()
    {
        RuleFor(x => x.CoachId).NotEmpty();
        RuleFor(x => x.ProgramTemplateId).NotEmpty();
        RuleFor(x => x.AthleteIds)
            .NotEmpty().WithMessage("At least one athlete must be selected.")
            .Must(ids => ids.Count <= 100).WithMessage("Cannot assign to more than 100 athletes at once.");
        RuleForEach(x => x.AthleteIds).NotEmpty();
        RuleFor(x => x.StartDate).NotEmpty();
    }
}
