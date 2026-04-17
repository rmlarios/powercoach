using CoachPlatform.Application.Shared.Exceptions;
using CoachPlatform.Application.Shared.Interfaces;
using CoachPlatform.Domain.Entities;
using CoachPlatform.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CoachPlatform.Application.Features.TrainingPrograms.Commands.AddProgramExercise;

/// <summary>
/// Handler for AddProgramExerciseCommand.
/// </summary>
public class AddProgramExerciseCommandHandler : IRequestHandler<AddProgramExerciseCommand, Guid>
{
    private readonly IApplicationDbContext _context;

    public AddProgramExerciseCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(AddProgramExerciseCommand request, CancellationToken cancellationToken)
    {
        // Get the day template with exercises
        var dayTemplate = await _context.ProgramDayTemplates
            .Include(d => d.Exercises)
            .Include(d => d.WeekTemplate)
                .ThenInclude(w => w.ProgramTemplate)
            .FirstOrDefaultAsync(d => d.Id == request.DayTemplateId, cancellationToken);

        if (dayTemplate == null)
        {
            throw new NotFoundException(nameof(ProgramDayTemplate), request.DayTemplateId);
        }

        // Verify coach ownership
        if (dayTemplate.WeekTemplate.ProgramTemplate.CoachId != request.CoachId)
        {
            throw new ForbiddenAccessException();
        }

        // Verify exercise exists and belongs to this coach
        var exercise = await _context.Exercises
            .FirstOrDefaultAsync(e => e.Id == request.ExerciseId && e.CoachId == request.CoachId, cancellationToken);

        if (exercise == null)
        {
            throw new NotFoundException(nameof(Exercise), request.ExerciseId);
        }

        // Parse exercise type
        var exerciseType = ExerciseType.Standard;
        if (!string.IsNullOrWhiteSpace(request.ExerciseType))
        {
            Enum.TryParse<ExerciseType>(request.ExerciseType, ignoreCase: true, out exerciseType);
        }

        // Add the exercise
        var exerciseTemplate = dayTemplate.AddExercise(
            exerciseId: request.ExerciseId,
            sets: request.Sets,
            reps: request.Reps,
            targetRpe: request.TargetRpe,
            restSeconds: request.RestSeconds,
            notes: request.Notes,
            exerciseType: exerciseType,
            percentageRM: request.PercentageRM,
            rawNotation: request.RawNotation,
            weight: request.Weight,
            emomConfigJson: request.EmomConfigJson,
            tempoConfigJson: request.TempoConfigJson,
            supersetConfigJson: request.SupersetConfigJson);

        // Explicitly track the new entity as Added
        _context.ProgramExerciseTemplates.Add(exerciseTemplate);

        await _context.SaveChangesAsync(cancellationToken);

        return exerciseTemplate.Id;
    }
}
