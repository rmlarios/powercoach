using CoachPlatform.Application.Shared.DTOs;
using CoachPlatform.Application.Shared.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CoachPlatform.Application.Features.TrainingPrograms.Queries.GetProgramTemplateById;

/// <summary>
/// Handler for GetProgramTemplateByIdQuery.
/// </summary>
public class GetProgramTemplateByIdQueryHandler : IRequestHandler<GetProgramTemplateByIdQuery, ProgramTemplateDetailDto?>
{
    private readonly IApplicationDbContext _context;

    public GetProgramTemplateByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ProgramTemplateDetailDto?> Handle(
        GetProgramTemplateByIdQuery request, 
        CancellationToken cancellationToken)
    {
        var programTemplate = await _context.ProgramTemplates
            .Include(p => p.Weeks.OrderBy(w => w.WeekNumber))
                .ThenInclude(w => w.Days.OrderBy(d => d.DayNumber))
                    .ThenInclude(d => d.Exercises.OrderBy(e => e.Order))
                        .ThenInclude(e => e.Exercise)
            .FirstOrDefaultAsync(p => p.Id == request.Id && p.CoachId == request.CoachId, cancellationToken);

        if (programTemplate == null)
        {
            return null;
        }

        return new ProgramTemplateDetailDto
        {
            Id = programTemplate.Id,
            CoachId = programTemplate.CoachId,
            Name = programTemplate.Name,
            Description = programTemplate.Description,
            DurationWeeks = programTemplate.DurationWeeks,
            IsActive = programTemplate.IsActive,
            CreatedAt = programTemplate.CreatedAt,
            UpdatedAt = programTemplate.UpdatedAt,
            Weeks = programTemplate.Weeks.Select(w => new ProgramWeekTemplateDto
            {
                Id = w.Id,
                WeekNumber = w.WeekNumber,
                Name = w.Name,
                Notes = w.Notes,
                Days = w.Days.Select(d => new ProgramDayTemplateDto
                {
                    Id = d.Id,
                    DayNumber = d.DayNumber,
                    Name = d.Name,
                    Focus = d.Focus,
                    Notes = d.Notes,
                    Exercises = d.Exercises.Select(e => new ProgramExerciseTemplateDto
                    {
                        Id = e.Id,
                        ExerciseId = e.ExerciseId,
                        ExerciseName = e.Exercise.Name,
                        Sets = e.Sets,
                        Reps = e.Reps,
                        TargetRpe = e.TargetRpe,
                        RestSeconds = e.RestSeconds,
                        Notes = e.Notes,
                        Order = e.Order,
                        ExerciseType = e.ExerciseType.ToString(),
                        PercentageRM = e.PercentageRM,
                        RawNotation = e.RawNotation,
                        Weight = e.Weight,
                        EmomConfigJson = e.EmomConfigJson,
                        TempoConfigJson = e.TempoConfigJson,
                        SupersetConfigJson = e.SupersetConfigJson
                    }).ToList()
                }).ToList()
            }).ToList()
        };
    }
}
