using CoachPlatform.Application.Shared.DTOs;
using CoachPlatform.Application.Shared.Interfaces;
using CoachPlatform.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CoachPlatform.Application.Features.Athletes.Queries.GetAthletesByCoach;

/// <summary>
/// Handler for GetAthletesByCoachQuery.
/// </summary>
public class GetAthletesByCoachQueryHandler : IRequestHandler<GetAthletesByCoachQuery, PagedResult<AthleteListItemDto>>
{
    private readonly IApplicationDbContext _context;

    public GetAthletesByCoachQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<AthleteListItemDto>> Handle(
        GetAthletesByCoachQuery request,
        CancellationToken cancellationToken)
    {
        var query = _context.Athletes
            .AsNoTracking()
            .Where(a => a.CoachId == request.CoachId);

        // Apply status filter
        if (!string.IsNullOrWhiteSpace(request.Status) && 
            Enum.TryParse<AthleteStatus>(request.Status, true, out var status))
        {
            query = query.Where(a => a.Status == status);
        }

        // Apply email filter
        if (!string.IsNullOrWhiteSpace(request.Email))
        {
            var emailLower = request.Email.ToLower();
            query = query.Where(a => a.Email.Value.ToLower().Contains(emailLower));
        }

        // Apply country filter
        if (!string.IsNullOrWhiteSpace(request.Country))
        {
            var countryLower = request.Country.ToLower();
            query = query.Where(a => a.Country != null && a.Country.ToLower().Contains(countryLower));
        }

        // Apply search filter (name or email)
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var searchLower = request.SearchTerm.ToLower();
            query = query.Where(a =>
                a.Name.FirstName.ToLower().Contains(searchLower) ||
                a.Name.LastName.ToLower().Contains(searchLower) ||
                a.Email.Value.ToLower().Contains(searchLower));
        }

        // Get total count for pagination
        var totalCount = await query.CountAsync(cancellationToken);

        // Apply pagination and ordering
        var athletes = await query
            .OrderBy(a => a.Name.LastName)
            .ThenBy(a => a.Name.FirstName)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        var items = athletes.Select(a => new AthleteListItemDto
        {
            Id = a.Id,
            FullName = $"{a.Name.FirstName} {a.Name.LastName}",
            Email = a.Email.Value,
            Country = a.Country,
            Status = a.Status,
            StartDate = a.StartDate,
            ProfilePictureUrl = a.ProfilePictureUrl
        }).ToList();

        return PagedResult<AthleteListItemDto>.Create(
            items,
            request.PageNumber,
            request.PageSize,
            totalCount);
    }
}
