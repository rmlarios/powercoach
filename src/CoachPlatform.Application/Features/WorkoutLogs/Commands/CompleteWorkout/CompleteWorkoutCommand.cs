using CoachPlatform.Application.Shared.Interfaces;
using MediatR;

namespace CoachPlatform.Application.Features.WorkoutLogs.Commands.CompleteWorkout;

/// <summary>
/// Command to complete an entire workout session.
/// </summary>
public record CompleteWorkoutCommand : IRequest<Unit>, IAthleteOwnedRequest
{
    public Guid AthleteId { get; init; }
    public Guid WorkoutId { get; init; }
    public int? DurationMinutes { get; init; }
    public int? FatigueRating { get; init; }
    public string? Notes { get; init; }
}
