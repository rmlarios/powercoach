using CoachPlatform.Application.Shared.Exceptions;
using CoachPlatform.Application.Shared.Interfaces;
using CoachPlatform.Domain.Entities;
using CoachPlatform.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CoachPlatform.Application.Features.TrainingPrograms.Commands.AssignProgramToAthlete;

/// <summary>
/// Handler for AssignProgramToAthleteCommand.
/// </summary>
public class AssignProgramToAthleteCommandHandler : IRequestHandler<AssignProgramToAthleteCommand, Guid>
{
    private readonly IApplicationDbContext _context;

    public AssignProgramToAthleteCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(AssignProgramToAthleteCommand request, CancellationToken cancellationToken)
    {
        // Verify athlete exists and belongs to the coach
        var athlete = await _context.Athletes
            .FirstOrDefaultAsync(a => a.Id == request.AthleteId && a.CoachId == request.CoachId, cancellationToken);

        if (athlete == null)
        {
            throw new NotFoundException(nameof(Athlete), request.AthleteId);
        }

        // Verify program template exists and belongs to the coach
        var programTemplate = await _context.ProgramTemplates
            .Include(p => p.Weeks)
                .ThenInclude(w => w.Days)
                    .ThenInclude(d => d.Exercises)
            .FirstOrDefaultAsync(p => p.Id == request.ProgramTemplateId && p.CoachId == request.CoachId, cancellationToken);

        if (programTemplate == null)
        {
            throw new NotFoundException(nameof(ProgramTemplate), request.ProgramTemplateId);
        }

        if (!programTemplate.IsActive)
        {
            throw new InvalidOperationException("Cannot assign an inactive program template.");
        }

        // Check if athlete already has an active program
        var hasActiveProgram = await _context.AthletePrograms
            .AnyAsync(ap => ap.AthleteId == request.AthleteId && 
                          (ap.Status == ProgramStatus.Active || ap.Status == ProgramStatus.NotStarted), 
                     cancellationToken);

        if (hasActiveProgram)
        {
            throw new ConflictException(nameof(AthleteProgram), "AthleteId", 
                $"Athlete {request.AthleteId} already has an active or pending program.");
        }

        // Create the athlete program
        var athleteProgram = AthleteProgram.Create(
            athleteId: request.AthleteId,
            programTemplateId: request.ProgramTemplateId,
            startDate: request.StartDate,
            notes: request.Notes);

        // Pre-create workouts based on the template structure
        var currentDate = request.StartDate;
        foreach (var week in programTemplate.Weeks.OrderBy(w => w.WeekNumber))
        {
            foreach (var day in week.Days.OrderBy(d => d.DayNumber))
            {
                // Calculate scheduled date based on week and day number
                var daysToAdd = ((week.WeekNumber - 1) * 7) + (day.DayNumber - 1);
                var scheduledDate = request.StartDate.AddDays(daysToAdd);

                athleteProgram.AddWorkout(week.WeekNumber, day.DayNumber, scheduledDate);
            }
        }

        _context.AthletePrograms.Add(athleteProgram);
        await _context.SaveChangesAsync(cancellationToken);

        return athleteProgram.Id;
    }
}
