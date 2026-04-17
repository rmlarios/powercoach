using CoachPlatform.Application.Shared.Interfaces;
using MediatR;

namespace CoachPlatform.Application.Features.WorkoutLogs.Commands.StartWorkout;

/// <summary>
/// Command to start a workout session (marks it as InProgress).
/// </summary>
public record StartWorkoutCommand : IRequest<Unit>, IAthleteOwnedRequest
{
    public Guid AthleteId { get; init; }
    public Guid WorkoutId { get; init; }
}
