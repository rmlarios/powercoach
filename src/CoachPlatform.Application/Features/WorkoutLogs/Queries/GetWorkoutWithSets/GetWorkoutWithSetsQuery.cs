using CoachPlatform.Application.Shared.DTOs;
using CoachPlatform.Application.Shared.Interfaces;
using MediatR;

namespace CoachPlatform.Application.Features.WorkoutLogs.Queries.GetWorkoutWithSets;

/// <summary>
/// Query to get a specific workout with all exercises and sets.
/// </summary>
public record GetWorkoutWithSetsQuery : IRequest<TodayWorkoutDto?>, IAthleteOwnedRequest
{
    public Guid AthleteId { get; init; }
    public Guid WorkoutId { get; init; }
}
