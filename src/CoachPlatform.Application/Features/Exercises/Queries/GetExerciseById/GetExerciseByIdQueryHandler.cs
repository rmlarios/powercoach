using CoachPlatform.Application.Features.Exercises.Shared;
using CoachPlatform.Application.Shared.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CoachPlatform.Application.Features.Exercises.Queries.GetExerciseById;

/// <summary>
/// Handler for GetExerciseByIdQuery.
/// </summary>
public class GetExerciseByIdQueryHandler : IRequestHandler<GetExerciseByIdQuery, ExerciseDto?>
{
    private readonly IApplicationDbContext _context;

    public GetExerciseByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ExerciseDto?> Handle(GetExerciseByIdQuery request, CancellationToken cancellationToken)
    {
        var exercise = await _context.Exercises
            .AsNoTracking()
            .Where(e => e.Id == request.ExerciseId)
            .Select(e => new ExerciseDto
            {
                Id = e.Id,
                CoachId = e.CoachId,
                Name = e.Name,
                Description = e.Description,
                Category = e.Category,
                PrimaryMuscleGroup = e.PrimaryMuscleGroup,
                SecondaryMuscleGroups = e.SecondaryMuscleGroups,
                VideoUrl = e.VideoUrl,
                ImageUrl = e.ImageUrl,
                Equipment = e.Equipment,
                IsCompound = e.IsCompound,
                IsActive = e.IsActive,
                CreatedAt = e.CreatedAt,
                UpdatedAt = e.UpdatedAt
            })
            .FirstOrDefaultAsync(cancellationToken);

        return exercise;
    }
}
