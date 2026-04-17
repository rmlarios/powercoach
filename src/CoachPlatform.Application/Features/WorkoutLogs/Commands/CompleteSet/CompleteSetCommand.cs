using CoachPlatform.Application.Shared.Interfaces;
using MediatR;

namespace CoachPlatform.Application.Features.WorkoutLogs.Commands.CompleteSet;

/// <summary>
/// Command to mark a specific set as completed.
/// </summary>
public record CompleteSetCommand : IRequest<Unit>, IAthleteOwnedRequest
{
    public Guid AthleteId { get; init; }
    public Guid WorkoutId { get; init; }
    public Guid ExerciseLogId { get; init; }
}
