using CoachPlatform.Application.Shared.Exceptions;
using CoachPlatform.Application.Shared.Interfaces;
using CoachPlatform.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CoachPlatform.Application.Features.TrainingPrograms.Commands.UpdateProgramTemplate;

/// <summary>
/// Handler for UpdateProgramTemplateCommand.
/// </summary>
public class UpdateProgramTemplateCommandHandler : IRequestHandler<UpdateProgramTemplateCommand, Unit>
{
    private readonly IApplicationDbContext _context;

    public UpdateProgramTemplateCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Unit> Handle(UpdateProgramTemplateCommand request, CancellationToken cancellationToken)
    {
        var program = await _context.ProgramTemplates
            .FirstOrDefaultAsync(p => p.Id == request.ProgramTemplateId && p.CoachId == request.CoachId, cancellationToken);

        if (program == null)
        {
            throw new NotFoundException(nameof(ProgramTemplate), request.ProgramTemplateId);
        }

        program.Update(request.Name, request.Description, request.DurationWeeks);

        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
