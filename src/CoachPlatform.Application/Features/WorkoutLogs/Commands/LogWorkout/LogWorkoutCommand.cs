using CoachPlatform.Application.Shared.Interfaces;
using MediatR;

namespace CoachPlatform.Application.Features.WorkoutLogs.Commands.LogWorkout;

/// <summary>
/// Command to log a new workout.
/// </summary>
public record LogWorkoutCommand : IRequest<Guid>, IAthleteOwnedRequest
{
    public Guid AthleteId { get; init; }
    public Guid ExerciseId { get; init; }
    public int Sets { get; init; }
    public int Reps { get; init; }
    public decimal? Weight { get; init; }
    public int? RPE { get; init; }
    public string? Notes { get; init; }
    public DateTime WorkoutDate { get; init; }
}
