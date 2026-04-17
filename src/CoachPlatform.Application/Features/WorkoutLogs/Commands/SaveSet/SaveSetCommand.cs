using CoachPlatform.Application.Shared.DTOs;
using CoachPlatform.Application.Shared.Interfaces;
using MediatR;

namespace CoachPlatform.Application.Features.WorkoutLogs.Commands.SaveSet;

/// <summary>
/// Command to save (create or update) an exercise set within a workout.
/// If ExerciseLogId is provided and exists, updates it. Otherwise creates a new log entry.
/// This is the primary command for the frontend auto-save flow.
/// </summary>
public record SaveSetCommand : IRequest<WorkoutSetDto>, IAthleteOwnedRequest
{
    public Guid AthleteId { get; init; }
    public Guid WorkoutId { get; init; }
    public Guid? ExerciseLogId { get; init; }
    public Guid ExerciseId { get; init; }
    public int SetNumber { get; init; }
    public int Reps { get; init; }
    public decimal Weight { get; init; }
    public decimal? Rpe { get; init; }
    public int? TargetReps { get; init; }
    public decimal? TargetWeight { get; init; }
    public bool IsCompleted { get; init; }
    public string? Notes { get; init; }
}
