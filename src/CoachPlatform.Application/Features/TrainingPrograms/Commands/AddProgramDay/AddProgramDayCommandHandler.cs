using CoachPlatform.Application.Shared.Exceptions;
using CoachPlatform.Application.Shared.Interfaces;
using CoachPlatform.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CoachPlatform.Application.Features.TrainingPrograms.Commands.AddProgramDay;

/// <summary>
/// Handler for AddProgramDayCommand.
/// </summary>
public class AddProgramDayCommandHandler : IRequestHandler<AddProgramDayCommand, Guid>
{
    private readonly IApplicationDbContext _context;

    public AddProgramDayCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(AddProgramDayCommand request, CancellationToken cancellationToken)
    {
        // Get the week template with days
        var weekTemplate = await _context.ProgramWeekTemplates
            .Include(w => w.Days)
            .Include(w => w.ProgramTemplate)
            .FirstOrDefaultAsync(w => w.Id == request.WeekTemplateId, cancellationToken);

        if (weekTemplate == null)
        {
            throw new NotFoundException(nameof(ProgramWeekTemplate), request.WeekTemplateId);
        }

        // Verify coach ownership
        if (weekTemplate.ProgramTemplate.CoachId != request.CoachId)
        {
            throw new ForbiddenAccessException();
        }

        // Add the day
        var day = weekTemplate.AddDay(request.DayNumber, request.Focus, request.Name, request.Notes);

        // Explicitly track the new entity as Added
        _context.ProgramDayTemplates.Add(day);

        await _context.SaveChangesAsync(cancellationToken);

        return day.Id;
    }
}
