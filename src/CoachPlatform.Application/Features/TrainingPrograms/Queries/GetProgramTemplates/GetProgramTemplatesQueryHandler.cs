using CoachPlatform.Application.Shared.DTOs;
using CoachPlatform.Application.Shared.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CoachPlatform.Application.Features.TrainingPrograms.Queries.GetProgramTemplates;

/// <summary>
/// Handler for GetProgramTemplatesQuery.
/// </summary>
public class GetProgramTemplatesQueryHandler : IRequestHandler<GetProgramTemplatesQuery, PagedResult<ProgramTemplateListItemDto>>
{
    private readonly IApplicationDbContext _context;

    public GetProgramTemplatesQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<ProgramTemplateListItemDto>> Handle(
        GetProgramTemplatesQuery request, 
        CancellationToken cancellationToken)
    {
        var query = _context.ProgramTemplates
            .Where(p => p.CoachId == request.CoachId)
            .AsQueryable();

        // Apply filters
        if (request.IsActive.HasValue)
        {
            query = query.Where(p => p.IsActive == request.IsActive.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var searchTerm = request.SearchTerm.Trim().ToLower();
            query = query.Where(p => 
                p.Name.ToLower().Contains(searchTerm) ||
                (p.Description != null && p.Description.ToLower().Contains(searchTerm)));
        }

        // Get total count
        var totalCount = await query.CountAsync(cancellationToken);

        // Get paginated results
        var items = await query
            .OrderByDescending(p => p.CreatedAt)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(p => new ProgramTemplateListItemDto
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                DurationWeeks = p.DurationWeeks,
                IsActive = p.IsActive,
                WeekCount = p.Weeks.Count,
                CreatedAt = p.CreatedAt
            })
            .ToListAsync(cancellationToken);

        return PagedResult<ProgramTemplateListItemDto>.Create(items, request.PageNumber, request.PageSize, totalCount);
    }
}
