using CoachPlatform.Application.Shared.Exceptions;
using CoachPlatform.Application.Shared.Interfaces;
using CoachPlatform.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CoachPlatform.Application.Features.Exercises.Commands.CreateExercise;

/// <summary>
/// Handler for CreateExerciseCommand.
/// </summary>
public class CreateExerciseCommandHandler : IRequestHandler<CreateExerciseCommand, Guid>
{
    private readonly IApplicationDbContext _context;

    public CreateExerciseCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(CreateExerciseCommand request, CancellationToken cancellationToken)
    {
        // Check for duplicate exercise name globally
        var duplicateExists = await _context.Exercises
            .AnyAsync(e => e.Name == request.Name, cancellationToken);

        if (duplicateExists)
        {
            throw new ConflictException($"An exercise with name '{request.Name}' already exists.");
        }

        // Create the exercise (no coach ownership)
        var exercise = Exercise.Create(
            null,
            request.Name,
            request.Category,
            request.PrimaryMuscleGroup,
            request.Description,
            request.IsCompound);

        // Set optional fields
        if (request.SecondaryMuscleGroups?.Any() == true)
        {
            exercise.SetSecondaryMuscleGroups(request.SecondaryMuscleGroups);
        }

        if (!string.IsNullOrWhiteSpace(request.VideoUrl))
        {
            exercise.SetVideoUrl(request.VideoUrl);
        }

        if (!string.IsNullOrWhiteSpace(request.Equipment))
        {
            exercise.SetEquipment(request.Equipment);
        }

        _context.Exercises.Add(exercise);
        await _context.SaveChangesAsync(cancellationToken);

        return exercise.Id;
    }
}
