using CoachPlatform.Application.Shared.Exceptions;
using CoachPlatform.Application.Shared.Interfaces;
using CoachPlatform.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CoachPlatform.Application.Features.TrainingPrograms.Commands.DeleteProgramExercise;

/// <summary>
/// Handler for DeleteProgramExerciseCommand.
/// </summary>
public class DeleteProgramExerciseCommandHandler : IRequestHandler<DeleteProgramExerciseCommand, Unit>
{
    private readonly IApplicationDbContext _context;

    public DeleteProgramExerciseCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Unit> Handle(DeleteProgramExerciseCommand request, CancellationToken cancellationToken)
    {
        var exerciseTemplate = await _context.ProgramExerciseTemplates
            .Include(e => e.DayTemplate)
                .ThenInclude(d => d.WeekTemplate)
                    .ThenInclude(w => w.ProgramTemplate)
            .FirstOrDefaultAsync(e => e.Id == request.ExerciseTemplateId, cancellationToken);

        if (exerciseTemplate == null)
        {
            throw new NotFoundException(nameof(ProgramExerciseTemplate), request.ExerciseTemplateId);
        }

        // Verify coach ownership
        if (exerciseTemplate.DayTemplate.WeekTemplate.ProgramTemplate.CoachId != request.CoachId)
        {
            throw new ForbiddenAccessException();
        }

        _context.ProgramExerciseTemplates.Remove(exerciseTemplate);

        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
