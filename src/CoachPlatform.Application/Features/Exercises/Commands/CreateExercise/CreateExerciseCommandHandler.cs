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
        // Verify coach exists
        var coachExists = await _context.Coaches
            .AnyAsync(c => c.Id == request.CoachId, cancellationToken);

        if (!coachExists)
        {
            throw new NotFoundException(nameof(Coach), request.CoachId);
        }

        // Check for duplicate exercise name for this coach
        var duplicateExists = await _context.Exercises
            .AnyAsync(e => e.CoachId == request.CoachId && e.Name == request.Name, cancellationToken);

        if (duplicateExists)
        {
            throw new ConflictException($"An exercise with name '{request.Name}' already exists.");
        }

        // Create the exercise
        var exercise = Exercise.Create(
            request.CoachId,
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
