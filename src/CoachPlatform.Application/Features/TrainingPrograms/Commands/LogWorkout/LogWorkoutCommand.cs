using CoachPlatform.Application.Shared.DTOs;
using MediatR;

namespace CoachPlatform.Application.Features.TrainingPrograms.Commands.LogWorkout;

/// <summary>
/// Command to log a completed workout by an athlete.
/// </summary>
public record LogWorkoutCommand : IRequest<Guid>
{
    public Guid WorkoutId { get; init; }
    public int? DurationMinutes { get; init; }
    public int? FatigueRating { get; init; }
    public string? Notes { get; init; }
    public IReadOnlyList<LogExerciseSetDto> ExerciseSets { get; init; } = [];
}
