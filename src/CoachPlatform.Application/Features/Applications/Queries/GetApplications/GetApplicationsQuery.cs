using CoachPlatform.Application.Shared.DTOs;
using CoachPlatform.Application.Shared.Interfaces;
using CoachPlatform.Domain.Enums;
using MediatR;

namespace CoachPlatform.Application.Features.Applications.Queries.GetApplications;

/// <summary>
/// Query to get applications with optional filters.
/// </summary>
public record GetApplicationsQuery : IRequest<PagedResult<ApplicationListItemDto>>, ITenantRequest
{
    public Guid CoachId { get; init; }
    public ApplicationStatus? Status { get; init; }
    public string? Email { get; init; }
    public DateTime? FromDate { get; init; }
    public DateTime? ToDate { get; init; }
    public string? SearchTerm { get; init; }
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}
