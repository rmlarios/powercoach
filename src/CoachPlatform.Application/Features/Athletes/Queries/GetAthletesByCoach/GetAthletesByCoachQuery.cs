using CoachPlatform.Application.Shared.DTOs;
using CoachPlatform.Application.Shared.Interfaces;
using MediatR;

namespace CoachPlatform.Application.Features.Athletes.Queries.GetAthletesByCoach;

/// <summary>
/// Query to get all athletes for a specific coach with pagination and filtering.
/// </summary>
public record GetAthletesByCoachQuery : IRequest<PagedResult<AthleteListItemDto>>, ITenantRequest
{
    public Guid CoachId { get; init; }
    public string? Status { get; init; }
    public string? Email { get; init; }
    public string? Country { get; init; }
    public string? SearchTerm { get; init; }
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}
