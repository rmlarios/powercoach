using CoachPlatform.Application.Shared.Exceptions;
using CoachPlatform.Application.Shared.Interfaces;
using CoachPlatform.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CoachPlatform.Application.Features.TrainingPrograms.Commands.AddProgramWeek;

/// <summary>
/// Handler for AddProgramWeekCommand.
/// </summary>
public class AddProgramWeekCommandHandler : IRequestHandler<AddProgramWeekCommand, Guid>
{
    private readonly IApplicationDbContext _context;

    public AddProgramWeekCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(AddProgramWeekCommand request, CancellationToken cancellationToken)
    {
        // Get the program template with weeks
        var programTemplate = await _context.ProgramTemplates
            .Include(p => p.Weeks)
            .FirstOrDefaultAsync(p => p.Id == request.ProgramTemplateId && p.CoachId == request.CoachId, cancellationToken);

        if (programTemplate == null)
        {
            throw new NotFoundException(nameof(ProgramTemplate), request.ProgramTemplateId);
        }

        // Add the week
        var week = programTemplate.AddWeek(request.WeekNumber, request.Name);
        
        if (!string.IsNullOrWhiteSpace(request.Notes))
        {
            week.SetNotes(request.Notes);
        }

        // Explicitly track the new entity as Added to avoid EF detecting it as Modified
        _context.ProgramWeekTemplates.Add(week);

        await _context.SaveChangesAsync(cancellationToken);

        return week.Id;
    }
}
