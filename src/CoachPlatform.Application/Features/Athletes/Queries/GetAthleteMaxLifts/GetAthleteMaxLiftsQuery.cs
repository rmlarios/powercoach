using CoachPlatform.Application.Shared.DTOs;
using MediatR;

namespace CoachPlatform.Application.Features.Athletes.Queries.GetAthleteMaxLifts;

/// <summary>
/// Query to get all max lifts (1RMs) for an athlete.
/// </summary>
public record GetAthleteMaxLiftsQuery : IRequest<AthleteMaxLiftsDto?>
{
    /// <summary>
    /// The athlete's unique identifier.
    /// </summary>
    public Guid AthleteId { get; init; }
    
    /// <summary>
    /// Optional: Filter by specific exercise ID.
    /// </summary>
    public Guid? ExerciseId { get; init; }
}
