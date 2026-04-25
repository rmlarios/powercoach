using CoachPlatform.Application.Features.Exercises.Shared;
using CoachPlatform.Application.Shared.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CoachPlatform.Application.Features.Exercises.Queries.GetExercisesByCoach;

/// <summary>
/// Handler for GetExercisesByCoachQuery.
/// </summary>
public class GetExercisesByCoachQueryHandler : IRequestHandler<GetExercisesByCoachQuery, IEnumerable<ExerciseListItemDto>>
{
    private readonly IApplicationDbContext _context;

    public GetExercisesByCoachQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<ExerciseListItemDto>> Handle(GetExercisesByCoachQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Exercises
            .AsNoTracking()
            .AsQueryable();

        // Apply filters
        if (request.Category.HasValue)
        {
            query = query.Where(e => e.Category == request.Category.Value);
        }

        if (request.MuscleGroup.HasValue)
        {
            query = query.Where(e => e.PrimaryMuscleGroup == request.MuscleGroup.Value);
        }

        if (request.IsActive.HasValue)
        {
            query = query.Where(e => e.IsActive == request.IsActive.Value);
        }

        var exercises = await query
            .OrderBy(e => e.DisplayOrder)
            .ThenBy(e => e.Name)
            .Select(e => new ExerciseListItemDto
            {
                Id = e.Id,
                Name = e.Name,
                Category = e.Category,
                PrimaryMuscleGroup = e.PrimaryMuscleGroup,
                Equipment = e.Equipment,
                IsCompound = e.IsCompound,
                IsActive = e.IsActive
            })
            .ToListAsync(cancellationToken);

        return exercises;
    }
}
