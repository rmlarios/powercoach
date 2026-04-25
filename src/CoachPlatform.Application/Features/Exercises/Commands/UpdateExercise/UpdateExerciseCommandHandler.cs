using CoachPlatform.Application.Shared.Exceptions;
using CoachPlatform.Application.Shared.Interfaces;
using CoachPlatform.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CoachPlatform.Application.Features.Exercises.Commands.UpdateExercise;

/// <summary>
/// Handler for UpdateExerciseCommand.
/// </summary>
public class UpdateExerciseCommandHandler : IRequestHandler<UpdateExerciseCommand>
{
    private readonly IApplicationDbContext _context;

    public UpdateExerciseCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(UpdateExerciseCommand request, CancellationToken cancellationToken)
    {
        var exercise = await _context.Exercises
            .FirstOrDefaultAsync(e => e.Id == request.ExerciseId, cancellationToken);

        if (exercise is null)
        {
            throw new NotFoundException(nameof(Exercise), request.ExerciseId);
        }

        // Check for duplicate name (excluding the current exercise)
        var duplicateExists = await _context.Exercises
            .AnyAsync(e => e.Name == request.Name
                        && e.Id != request.ExerciseId, cancellationToken);

        if (duplicateExists)
        {
            throw new ConflictException($"An exercise with name '{request.Name}' already exists.");
        }

        exercise.Update(
            request.Name,
            request.Category,
            request.PrimaryMuscleGroup,
            request.Description,
            request.IsCompound);

        if (request.SecondaryMuscleGroups != null)
        {
            exercise.SetSecondaryMuscleGroups(request.SecondaryMuscleGroups);
        }

        exercise.SetVideoUrl(request.VideoUrl);
        exercise.SetEquipment(request.Equipment);

        await _context.SaveChangesAsync(cancellationToken);
    }
}
