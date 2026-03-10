using CoachPlatform.Application.Shared.DTOs;
using CoachPlatform.Application.Shared.Interfaces;
using MediatR;

namespace CoachPlatform.Application.Features.WorkoutLogs.Queries.GetAthleteWorkouts;

/// <summary>
/// Query to get all workouts for a specific athlete.
/// </summary>
public record GetAthleteWorkoutsQuery : IRequest<IEnumerable<WorkoutLogListItemDto>>, IAthleteOwnedRequest
{
    public Guid AthleteId { get; init; }
}
