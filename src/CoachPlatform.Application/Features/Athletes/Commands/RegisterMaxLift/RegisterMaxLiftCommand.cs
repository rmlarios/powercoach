using MediatR;

namespace CoachPlatform.Application.Features.Athletes.Commands.RegisterMaxLift;

/// <summary>
/// Command to register a new max lift (1RM) for an athlete.
/// </summary>
public record RegisterMaxLiftCommand : IRequest<Guid>
{
    /// <summary>
    /// The athlete's unique identifier.
    /// </summary>
    public Guid AthleteId { get; init; }
    
    /// <summary>
    /// The exercise's unique identifier.
    /// </summary>
    public Guid ExerciseId { get; init; }
    
    /// <summary>
    /// The max lift weight in kilograms.
    /// </summary>
    public decimal Weight { get; init; }
    
    /// <summary>
    /// Whether this is a tested (actual) max or an estimated max.
    /// </summary>
    public bool IsTested { get; init; } = true;
    
    /// <summary>
    /// Optional date when the max was achieved. Defaults to now.
    /// </summary>
    public DateTime? RecordedAt { get; init; }
    
    /// <summary>
    /// Optional notes about the max lift.
    /// </summary>
    public string? Notes { get; init; }
    
    /// <summary>
    /// If estimated, details about how it was calculated.
    /// </summary>
    public string? EstimationDetails { get; init; }
}
