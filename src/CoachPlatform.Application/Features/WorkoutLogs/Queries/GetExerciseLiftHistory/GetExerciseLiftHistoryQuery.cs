using CoachPlatform.Application.Shared.DTOs;
using CoachPlatform.Application.Shared.Interfaces;
using MediatR;

namespace CoachPlatform.Application.Features.WorkoutLogs.Queries.GetExerciseLiftHistory;

/// <summary>
/// Query to get the lift history for a specific exercise for an athlete.
/// Returns exercise details (video, cues, instructions) plus session-by-session lift data.
/// </summary>
public record GetExerciseLiftHistoryQuery : IRequest<ExerciseLiftHistoryDto?>, IAthleteOwnedRequest
{
    public Guid AthleteId { get; init; }
    public Guid ExerciseId { get; init; }

    /// <summary>
    /// Maximum number of lift entries to return (most recent first). Default 20.
    /// </summary>
    public int Limit { get; init; } = 20;
}
