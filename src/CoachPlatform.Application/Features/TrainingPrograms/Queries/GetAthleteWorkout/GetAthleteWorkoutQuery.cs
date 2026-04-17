using CoachPlatform.Application.Shared.DTOs;
using MediatR;

namespace CoachPlatform.Application.Features.TrainingPrograms.Queries.GetAthleteWorkout;

/// <summary>
/// Query to get a specific athlete workout.
/// </summary>
public record GetAthleteWorkoutQuery : IRequest<AthleteWorkoutDto?>
{
    public Guid WorkoutId { get; init; }
}
