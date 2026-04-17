using CoachPlatform.Application.Shared.Interfaces;
using MediatR;

namespace CoachPlatform.Application.Features.WorkoutLogs.Commands.SkipWorkout;

/// <summary>
/// Command to skip an entire workout with an optional reason.
/// </summary>
public record SkipWorkoutCommand : IRequest<Unit>, IAthleteOwnedRequest
{
    public Guid AthleteId { get; init; }
    public Guid WorkoutId { get; init; }
    public string? Reason { get; init; }
}
