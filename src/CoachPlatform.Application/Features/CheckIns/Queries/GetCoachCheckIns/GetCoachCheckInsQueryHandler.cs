using CoachPlatform.Application.Shared.DTOs;
using CoachPlatform.Application.Shared.Exceptions;
using CoachPlatform.Application.Shared.Interfaces;
using CoachPlatform.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CoachPlatform.Application.Features.CheckIns.Queries.GetCoachCheckIns;

/// <summary>
/// Handler for GetCoachCheckInsQuery.
/// </summary>
public class GetCoachCheckInsQueryHandler : IRequestHandler<GetCoachCheckInsQuery, PagedResult<CheckInDto>>
{
    private readonly IApplicationDbContext _context;

    public GetCoachCheckInsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<CheckInDto>> Handle(GetCoachCheckInsQuery request, CancellationToken cancellationToken)
    {
        // Get athletes assigned to this coach
        var assignedAthleteIds = await _context.Athletes
            .Where(a => a.CoachId == request.CoachId && a.Status == Domain.Enums.AthleteStatus.Active)
            .Select(a => a.Id)
            .ToListAsync(cancellationToken);

        var query = _context.CheckIns
            .Include(c => c.Athlete)
            .Where(c => assignedAthleteIds.Contains(c.AthleteId));

        if (request.IsReviewed.HasValue)
        {
            query = query.Where(c => c.IsReviewed == request.IsReviewed.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(c => c.CheckInDate)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(c => new CheckInDto
            {
                Id = c.Id,
                AthleteId = c.AthleteId,
                AthleteName = c.Athlete.Name.FirstName + " " + c.Athlete.Name.LastName,
                CheckInDate = c.CheckInDate,
                Weight = c.Weight,
                WeightUnit = "kg",
                Notes = c.Notes,
                PhotoUrls = c.PhotoUrls,
                CoachFeedback = c.CoachFeedback,
                ReviewedAt = c.FeedbackDate,
                CreatedAt = c.CreatedAt,
                UpdatedAt = c.UpdatedAt,
                EnergyLevel = c.EnergyLevel,
                SleepQuality = c.SleepQuality,
                SleepHours = c.SleepHours,
                StressLevel = c.StressLevel,
                NutritionAdherence = c.NutritionAdherence,
                TrainingAdherence = c.TrainingAdherence
            })
            .ToListAsync(cancellationToken);

        return PagedResult<CheckInDto>.Create(items, request.PageNumber, request.PageSize, totalCount);
    }
}