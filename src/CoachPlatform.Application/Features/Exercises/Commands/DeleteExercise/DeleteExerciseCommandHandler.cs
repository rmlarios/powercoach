using CoachPlatform.Application.Shared.Exceptions;
using CoachPlatform.Application.Shared.Interfaces;
using CoachPlatform.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CoachPlatform.Application.Features.Exercises.Commands.DeleteExercise;

/// <summary>
/// Handler for DeleteExerciseCommand.
/// Soft-deletes (deactivates) the exercise rather than removing it,
/// since it may be referenced by existing program templates and workout logs.
/// </summary>
public class DeleteExerciseCommandHandler : IRequestHandler<DeleteExerciseCommand>
{
    private readonly IApplicationDbContext _context;

    public DeleteExerciseCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(DeleteExerciseCommand request, CancellationToken cancellationToken)
    {
        var exercise = await _context.Exercises
            .FirstOrDefaultAsync(e => e.Id == request.ExerciseId, cancellationToken);

        if (exercise is null)
        {
            throw new NotFoundException(nameof(Exercise), request.ExerciseId);
        }

        exercise.Deactivate();
        await _context.SaveChangesAsync(cancellationToken);
    }
}
