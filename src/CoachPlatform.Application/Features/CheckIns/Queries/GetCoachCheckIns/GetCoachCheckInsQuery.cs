using CoachPlatform.Application.Shared.DTOs;
using MediatR;

namespace CoachPlatform.Application.Features.CheckIns.Queries.GetCoachCheckIns;

/// <summary>
/// Query to get all check-ins for athletes assigned to a specific coach.
/// </summary>
public record GetCoachCheckInsQuery : IRequest<PagedResult<CheckInDto>>
{
    public Guid CoachId { get; init; }
    public bool? IsReviewed { get; init; }
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
}
