using CoachPlatform.Application.Shared.Exceptions;
using CoachPlatform.Application.Shared.Interfaces;
using CoachPlatform.Domain.Entities;
using CoachPlatform.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CoachPlatform.Application.Features.TrainingPrograms.Commands.BulkAssignProgramToAthletes;

/// <summary>
/// Handler for BulkAssignProgramToAthletesCommand.
/// Assigns the same program template to multiple athletes, skipping any that already
/// have an active or pending program (returns only the IDs of successful assignments).
/// </summary>
public class BulkAssignProgramToAthletesCommandHandler
    : IRequestHandler<BulkAssignProgramToAthletesCommand, List<Guid>>
{
    private readonly IApplicationDbContext _context;

    public BulkAssignProgramToAthletesCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<Guid>> Handle(
        BulkAssignProgramToAthletesCommand request,
        CancellationToken cancellationToken)
    {
        // Verify program template exists and belongs to the coach
        var programTemplate = await _context.ProgramTemplates
            .Include(p => p.Weeks)
                .ThenInclude(w => w.Days)
                    .ThenInclude(d => d.Exercises)
            .FirstOrDefaultAsync(
                p => p.Id == request.ProgramTemplateId && p.CoachId == request.CoachId,
                cancellationToken);

        if (programTemplate is null)
            throw new NotFoundException(nameof(ProgramTemplate), request.ProgramTemplateId);

        if (!programTemplate.IsActive)
            throw new InvalidOperationException("Cannot assign an inactive program template.");

        // Load all requested athletes that belong to this coach in one query
        var athletes = await _context.Athletes
            .Where(a => request.AthleteIds.Contains(a.Id) && a.CoachId == request.CoachId)
            .ToListAsync(cancellationToken);

        // Load athletes that already have an active/pending program
        var busyAthleteIds = await _context.AthletePrograms
            .Where(ap =>
                request.AthleteIds.Contains(ap.AthleteId) &&
                (ap.Status == ProgramStatus.Active || ap.Status == ProgramStatus.NotStarted))
            .Select(ap => ap.AthleteId)
            .ToListAsync(cancellationToken);

        var createdIds = new List<Guid>();

        foreach (var athlete in athletes)
        {
            // Skip athletes that already have an active program
            if (busyAthleteIds.Contains(athlete.Id))
                continue;

            var athleteProgram = AthleteProgram.Create(
                athleteId: athlete.Id,
                programTemplateId: request.ProgramTemplateId,
                startDate: request.StartDate,
                notes: request.Notes);

            // Pre-create workouts based on the template structure
            foreach (var week in programTemplate.Weeks.OrderBy(w => w.WeekNumber))
            {
                foreach (var day in week.Days.OrderBy(d => d.DayNumber))
                {
                    var daysToAdd = ((week.WeekNumber - 1) * 7) + (day.DayNumber - 1);
                    var scheduledDate = request.StartDate.AddDays(daysToAdd);
                    athleteProgram.AddWorkout(week.WeekNumber, day.DayNumber, scheduledDate);
                }
            }

            _context.AthletePrograms.Add(athleteProgram);
            createdIds.Add(athleteProgram.Id);
        }

        await _context.SaveChangesAsync(cancellationToken);

        return createdIds;
    }
}
