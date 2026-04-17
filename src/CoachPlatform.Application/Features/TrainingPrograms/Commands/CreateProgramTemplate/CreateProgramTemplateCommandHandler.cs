using CoachPlatform.Application.Shared.Exceptions;
using CoachPlatform.Application.Shared.Interfaces;
using CoachPlatform.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CoachPlatform.Application.Features.TrainingPrograms.Commands.CreateProgramTemplate;

/// <summary>
/// Handler for CreateProgramTemplateCommand.
/// </summary>
public class CreateProgramTemplateCommandHandler : IRequestHandler<CreateProgramTemplateCommand, Guid>
{
    private readonly IApplicationDbContext _context;

    public CreateProgramTemplateCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(CreateProgramTemplateCommand request, CancellationToken cancellationToken)
    {
        // Verify coach exists
        var coachExists = await _context.Coaches
            .AnyAsync(c => c.Id == request.CoachId, cancellationToken);

        if (!coachExists)
        {
            throw new NotFoundException(nameof(Coach), request.CoachId);
        }

        // Check for duplicate program name for this coach
        var nameExists = await _context.ProgramTemplates
            .AnyAsync(p => p.CoachId == request.CoachId && p.Name == request.Name.Trim(), cancellationToken);

        if (nameExists)
        {
            throw new ConflictException(nameof(ProgramTemplate), "Name", request.Name);
        }

        // Create the program template
        var programTemplate = ProgramTemplate.Create(
            coachId: request.CoachId,
            name: request.Name,
            description: request.Description,
            durationWeeks: request.DurationWeeks);

        _context.ProgramTemplates.Add(programTemplate);
        await _context.SaveChangesAsync(cancellationToken);

        return programTemplate.Id;
    }
}
