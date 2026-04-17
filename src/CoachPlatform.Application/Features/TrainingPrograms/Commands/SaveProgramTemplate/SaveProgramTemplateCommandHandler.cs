using CoachPlatform.Application.Shared.Exceptions;
using CoachPlatform.Application.Shared.Interfaces;
using CoachPlatform.Domain.Entities;
using CoachPlatform.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CoachPlatform.Application.Features.TrainingPrograms.Commands.SaveProgramTemplate;

/// <summary>
/// Handler for SaveProgramTemplateCommand.
/// Performs a full replace of the program structure (weeks → days → exercises).
/// </summary>
public class SaveProgramTemplateCommandHandler : IRequestHandler<SaveProgramTemplateCommand, Guid>
{
    private readonly IApplicationDbContext _context;

    public SaveProgramTemplateCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(SaveProgramTemplateCommand request, CancellationToken cancellationToken)
    {
        // Load the full program graph
        var program = await _context.ProgramTemplates
            .Include(p => p.Weeks)
                .ThenInclude(w => w.Days)
                    .ThenInclude(d => d.Exercises)
            .FirstOrDefaultAsync(p => p.Id == request.ProgramTemplateId && p.CoachId == request.CoachId, cancellationToken);

        if (program == null)
        {
            throw new NotFoundException(nameof(ProgramTemplate), request.ProgramTemplateId);
        }

        // Remove all existing exercises, days, and weeks (EF tracks removals)
        foreach (var week in program.Weeks.ToList())
        {
            foreach (var day in week.Days.ToList())
            {
                _context.ProgramExerciseTemplates.RemoveRange(day.Exercises);
            }
            _context.ProgramDayTemplates.RemoveRange(week.Days);
        }
        _context.ProgramWeekTemplates.RemoveRange(program.Weeks);

        // Update program metadata
        program.Update(
            request.Data.Name,
            request.Data.Description,
            request.Data.DurationWeeks);

        // Rebuild the structure from the DTO
        foreach (var weekDto in request.Data.Weeks)
        {
            var week = program.AddWeek(weekDto.WeekNumber, weekDto.Name);
            _context.ProgramWeekTemplates.Add(week);

            if (!string.IsNullOrWhiteSpace(weekDto.Notes))
            {
                week.SetNotes(weekDto.Notes);
            }

            foreach (var dayDto in weekDto.Days)
            {
                // Parse day focus
                var focus = DayFocus.FullBody;
                if (!string.IsNullOrWhiteSpace(dayDto.Focus))
                {
                    Enum.TryParse<DayFocus>(dayDto.Focus, ignoreCase: true, out focus);
                }

                var day = week.AddDay(dayDto.DayNumber, focus, dayDto.Name, dayDto.Notes);
                _context.ProgramDayTemplates.Add(day);

                foreach (var exDto in dayDto.Exercises)
                {
                    // Parse exercise type
                    var exerciseType = ExerciseType.Standard;
                    if (!string.IsNullOrWhiteSpace(exDto.ExerciseType))
                    {
                        Enum.TryParse<ExerciseType>(exDto.ExerciseType, ignoreCase: true, out exerciseType);
                    }

                    var exercise = day.AddExercise(
                        exerciseId: exDto.ExerciseId,
                        sets: exDto.Sets,
                        reps: exDto.Reps,
                        targetRpe: exDto.TargetRpe,
                        restSeconds: exDto.RestSeconds,
                        notes: exDto.Notes,
                        exerciseType: exerciseType,
                        percentageRM: exDto.PercentageRM,
                        rawNotation: exDto.RawNotation,
                        weight: exDto.Weight,
                        emomConfigJson: exDto.EmomConfigJson,
                        tempoConfigJson: exDto.TempoConfigJson,
                        supersetConfigJson: exDto.SupersetConfigJson);
                    _context.ProgramExerciseTemplates.Add(exercise);
                }
            }
        }

        await _context.SaveChangesAsync(cancellationToken);

        return program.Id;
    }
}
