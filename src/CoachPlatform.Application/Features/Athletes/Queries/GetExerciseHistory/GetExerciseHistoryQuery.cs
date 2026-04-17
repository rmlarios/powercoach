using CoachPlatform.Application.Shared.DTOs;
using MediatR;

namespace CoachPlatform.Application.Features.Athletes.Queries.GetExerciseHistory;

/// <summary>
/// Query to get exercise history for an athlete.
/// Returns historical performance data, 1RM records, and suggestions.
/// </summary>
public record GetExerciseHistoryQuery : IRequest<ExerciseHistoryDto?>
{
    /// <summary>
    /// The athlete's unique identifier.
    /// </summary>
    public Guid AthleteId { get; init; }
    
    /// <summary>
    /// The exercise's unique identifier.
    /// </summary>
    public Guid ExerciseId { get; init; }
}
