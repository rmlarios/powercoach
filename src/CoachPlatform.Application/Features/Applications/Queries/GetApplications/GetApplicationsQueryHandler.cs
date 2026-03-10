using CoachPlatform.Application.Shared.DTOs;
using CoachPlatform.Application.Shared.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CoachPlatform.Application.Features.Applications.Queries.GetApplications;

/// <summary>
/// Handler for GetApplicationsQuery.
/// </summary>
public class GetApplicationsQueryHandler : IRequestHandler<GetApplicationsQuery, PagedResult<ApplicationListItemDto>>
{
    private readonly IApplicationDbContext _context;

    public GetApplicationsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<ApplicationListItemDto>> Handle(
        GetApplicationsQuery request, 
        CancellationToken cancellationToken)
    {
        var query = _context.Applications
            .AsNoTracking()
            .Where(a => a.CoachId == request.CoachId);

        // Filter by status
        if (request.Status.HasValue)
        {
            query = query.Where(a => a.Status == request.Status.Value);
        }

        // Filter by email (partial match)
        if (!string.IsNullOrWhiteSpace(request.Email))
        {
            var emailLower = request.Email.ToLower();
            query = query.Where(a => a.Email.Value.ToLower().Contains(emailLower));
        }

        // Filter by date range
        if (request.FromDate.HasValue)
        {
            query = query.Where(a => a.CreatedAt >= request.FromDate.Value);
        }

        if (request.ToDate.HasValue)
        {
            var toDateEnd = request.ToDate.Value.Date.AddDays(1);
            query = query.Where(a => a.CreatedAt < toDateEnd);
        }

        // Search by name or email
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var searchLower = request.SearchTerm.ToLower();
            query = query.Where(a =>
                a.ApplicantName.FirstName.ToLower().Contains(searchLower) ||
                a.ApplicantName.LastName.ToLower().Contains(searchLower) ||
                a.Email.Value.ToLower().Contains(searchLower));
        }

        // Get total count
        var totalCount = await query.CountAsync(cancellationToken);

        // Apply pagination and ordering (newest first)
        var applications = await query
            .OrderByDescending(a => a.CreatedAt)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        var items = applications.Select(a => new ApplicationListItemDto
        {
            Id = a.Id,
            FullName = $"{a.ApplicantName.FirstName} {a.ApplicantName.LastName}",
            Email = a.Email.Value,
            Country = a.Country,
            TotalLifts = (a.CurrentSquat ?? 0) + (a.CurrentBench ?? 0) + (a.CurrentDeadlift ?? 0),
            Status = a.Status,
            CreatedAt = a.CreatedAt
        }).ToList();

        return PagedResult<ApplicationListItemDto>.Create(
            items,
            request.PageNumber,
            request.PageSize,
            totalCount);
    }
}
