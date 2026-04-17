using CoachPlatform.Application.Shared.Interfaces;
using MediatR;

namespace CoachPlatform.Application.Features.TrainingPrograms.Commands.AddProgramExercise;

/// <summary>
/// Command to add an exercise to a program day.
/// </summary>
public record AddProgramExerciseCommand : IRequest<Guid>, ITenantRequest
{
    public Guid CoachId { get; init; }
    public Guid DayTemplateId { get; init; }
    public Guid ExerciseId { get; init; }
    public int Sets { get; init; }
    public string Reps { get; init; } = null!;
    public decimal? TargetRpe { get; init; }
    public int? RestSeconds { get; init; }
    public string? Notes { get; init; }
    public string? ExerciseType { get; init; }
    public decimal? PercentageRM { get; init; }
    public string? RawNotation { get; init; }
    public decimal? Weight { get; init; }
    public string? EmomConfigJson { get; init; }
    public string? TempoConfigJson { get; init; }
    public string? SupersetConfigJson { get; init; }
}
