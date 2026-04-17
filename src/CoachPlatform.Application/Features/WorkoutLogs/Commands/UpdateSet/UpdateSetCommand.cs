using CoachPlatform.Application.Shared.Interfaces;
using MediatR;

namespace CoachPlatform.Application.Features.WorkoutLogs.Commands.UpdateSet;

/// <summary>
/// Command to update the actual performance of a specific set (weight, reps, RPE).
/// </summary>
public record UpdateSetCommand : IRequest<Unit>, IAthleteOwnedRequest
{
    public Guid AthleteId { get; init; }
    public Guid WorkoutId { get; init; }
    public Guid ExerciseLogId { get; init; }
    public int Reps { get; init; }
    public decimal Weight { get; init; }
    public decimal? Rpe { get; init; }
    public string? Notes { get; init; }
}
