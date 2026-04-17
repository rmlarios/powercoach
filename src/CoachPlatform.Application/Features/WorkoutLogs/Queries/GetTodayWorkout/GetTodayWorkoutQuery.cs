using CoachPlatform.Application.Shared.DTOs;
using CoachPlatform.Application.Shared.Interfaces;
using MediatR;

namespace CoachPlatform.Application.Features.WorkoutLogs.Queries.GetTodayWorkout;

/// <summary>
/// Query to get today's workout for a specific athlete.
/// Returns null if the athlete has no workout scheduled for today.
/// </summary>
public record GetTodayWorkoutQuery : IRequest<TodayWorkoutDto?>, IAthleteOwnedRequest
{
    public Guid AthleteId { get; init; }
}
