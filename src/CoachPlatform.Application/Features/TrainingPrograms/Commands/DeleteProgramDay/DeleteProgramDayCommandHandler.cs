using CoachPlatform.Application.Shared.Exceptions;
using CoachPlatform.Application.Shared.Interfaces;
using CoachPlatform.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CoachPlatform.Application.Features.TrainingPrograms.Commands.DeleteProgramDay;

/// <summary>
/// Handler for DeleteProgramDayCommand.
/// </summary>
public class DeleteProgramDayCommandHandler : IRequestHandler<DeleteProgramDayCommand, Unit>
{
    private readonly IApplicationDbContext _context;

    public DeleteProgramDayCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Unit> Handle(DeleteProgramDayCommand request, CancellationToken cancellationToken)
    {
        var day = await _context.ProgramDayTemplates
            .Include(d => d.Exercises)
            .Include(d => d.WeekTemplate)
                .ThenInclude(w => w.ProgramTemplate)
            .FirstOrDefaultAsync(d => d.Id == request.DayId, cancellationToken);

        if (day == null)
        {
            throw new NotFoundException(nameof(ProgramDayTemplate), request.DayId);
        }

        // Verify coach ownership
        if (day.WeekTemplate.ProgramTemplate.CoachId != request.CoachId)
        {
            throw new ForbiddenAccessException();
        }

        // Remove exercises then the day
        _context.ProgramExerciseTemplates.RemoveRange(day.Exercises);
        _context.ProgramDayTemplates.Remove(day);

        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
