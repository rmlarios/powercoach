using CoachPlatform.Application.Shared.DTOs;
using CoachPlatform.Application.Shared.Interfaces;
using MediatR;

namespace CoachPlatform.Application.Features.WorkoutLogs.Queries.GetWorkoutHistory;

/// <summary>
/// Query to get the workout history for an athlete (paginated).
/// </summary>
public record GetWorkoutHistoryQuery : IRequest<PagedResult<WorkoutHistoryItemDto>>, IAthleteOwnedRequest
{
    public Guid AthleteId { get; init; }
    public Guid? AthleteProgramId { get; init; }
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}
