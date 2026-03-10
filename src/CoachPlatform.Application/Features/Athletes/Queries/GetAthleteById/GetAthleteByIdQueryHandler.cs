using CoachPlatform.Application.Shared.DTOs;
using CoachPlatform.Application.Shared.Interfaces;
using CoachPlatform.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CoachPlatform.Application.Features.Athletes.Queries.GetAthleteById;

/// <summary>
/// Handler for GetAthleteByIdQuery.
/// </summary>
public class GetAthleteByIdQueryHandler : IRequestHandler<GetAthleteByIdQuery, AthleteDetailDto?>
{
    private readonly IApplicationDbContext _context;

    public GetAthleteByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<AthleteDetailDto?> Handle(GetAthleteByIdQuery request, CancellationToken cancellationToken)
    {
        var athlete = await _context.Athletes
            .Include(a => a.Subscriptions)
                .ThenInclude(s => s.Plan)
            .Include(a => a.CheckIns.OrderByDescending(c => c.CheckInDate).Take(5))
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == request.Id, cancellationToken);

        if (athlete is null)
        {
            return null;
        }

        return new AthleteDetailDto
        {
            Id = athlete.Id,
            CoachId = athlete.CoachId,
            FirstName = athlete.Name.FirstName,
            LastName = athlete.Name.LastName,
            FullName = $"{athlete.Name.FirstName} {athlete.Name.LastName}",
            Email = athlete.Email.Value,
            Phone = athlete.Phone,
            Goals = athlete.Goals,
            Notes = athlete.Notes,
            Country = athlete.Country,
            Gender = athlete.Gender,
            DateOfBirth = athlete.DateOfBirth,
            Height = athlete.Height,
            Weight = athlete.Weight,
            ExperienceLevel = athlete.ExperienceLevel,
            StartDate = athlete.StartDate,
            EndDate = athlete.EndDate,
            Status = athlete.Status,
            ProfilePictureUrl = athlete.ProfilePictureUrl,
            ApplicationId = athlete.ApplicationId,
            CreatedAt = athlete.CreatedAt,
            UpdatedAt = athlete.UpdatedAt,
            ActiveSubscriptions = athlete.Subscriptions
                .Where(s => s.Status == SubscriptionStatus.Active)
                .Select(s => new SubscriptionSummaryDto
                {
                    Id = s.Id,
                    PlanName = s.Plan.Name,
                    StartDate = s.StartDate,
                    EndDate = s.EndDate,
                    Status = s.Status.ToString()
                })
                .ToList(),
            RecentCheckIns = athlete.CheckIns
                .Select(c => new CheckInSummaryDto
                {
                    Id = c.Id,
                    CheckInDate = c.CheckInDate,
                    Weight = c.Weight,
                    HasCoachFeedback = !string.IsNullOrEmpty(c.CoachFeedback)
                })
                .ToList()
        };
    }
}
