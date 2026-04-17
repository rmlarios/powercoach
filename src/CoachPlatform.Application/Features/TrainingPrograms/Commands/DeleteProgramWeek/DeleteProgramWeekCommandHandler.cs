using CoachPlatform.Application.Shared.Exceptions;
using CoachPlatform.Application.Shared.Interfaces;
using CoachPlatform.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CoachPlatform.Application.Features.TrainingPrograms.Commands.DeleteProgramWeek;

/// <summary>
/// Handler for DeleteProgramWeekCommand.
/// </summary>
public class DeleteProgramWeekCommandHandler : IRequestHandler<DeleteProgramWeekCommand, Unit>
{
    private readonly IApplicationDbContext _context;

    public DeleteProgramWeekCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Unit> Handle(DeleteProgramWeekCommand request, CancellationToken cancellationToken)
    {
        var program = await _context.ProgramTemplates
            .Include(p => p.Weeks)
                .ThenInclude(w => w.Days)
                    .ThenInclude(d => d.Exercises)
            .FirstOrDefaultAsync(p => p.Id == request.ProgramTemplateId && p.CoachId == request.CoachId, cancellationToken);

        if (program == null)
        {
            throw new NotFoundException(nameof(ProgramTemplate), request.ProgramTemplateId);
        }

        var week = program.Weeks.FirstOrDefault(w => w.Id == request.WeekId);
        if (week == null)
        {
            throw new NotFoundException(nameof(ProgramWeekTemplate), request.WeekId);
        }

        // Remove all exercises and days first (EF cascade may handle this, but be explicit)
        foreach (var day in week.Days.ToList())
        {
            _context.ProgramExerciseTemplates.RemoveRange(day.Exercises);
        }
        _context.ProgramDayTemplates.RemoveRange(week.Days);
        _context.ProgramWeekTemplates.Remove(week);

        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
