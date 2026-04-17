using CoachPlatform.Application.Shared.DTOs;
using CoachPlatform.Application.Shared.Interfaces;
using MediatR;

namespace CoachPlatform.Application.Features.WorkoutLogs.Queries.GetWeekWorkouts;

/// <summary>
/// Query to get all workouts for a specific week within an athlete's active program.
/// Returns lightweight summaries for the week strip navigation.
/// </summary>
public record GetWeekWorkoutsQuery : IRequest<WeekWorkoutsDto?>, IAthleteOwnedRequest
{
    public Guid AthleteId { get; init; }

    /// <summary>
    /// Week number to fetch. If null, returns the current week.
    /// </summary>
    public int? WeekNumber { get; init; }
}
