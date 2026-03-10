using CoachPlatform.Application.Features.WorkoutLogs.Commands.LogWorkout;
using CoachPlatform.Application.Features.WorkoutLogs.Queries.GetAthleteWorkouts;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CoachPlatform.API.Controllers;

/// <summary>
/// API endpoints for managing workout logs.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class WorkoutsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<WorkoutsController> _logger;

    public WorkoutsController(IMediator mediator, ILogger<WorkoutsController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Log a new workout for an athlete.
    /// </summary>
    /// <param name="request">The workout data.</param>
    /// <returns>The created workout log ID.</returns>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> LogWorkout([FromBody] LogWorkoutRequest request)
    {
        var command = new LogWorkoutCommand
        {
            AthleteId = request.AthleteId,
            ExerciseId = request.ExerciseId,
            Sets = request.Sets,
            Reps = request.Reps,
            Weight = request.Weight,
            RPE = request.RPE,
            Notes = request.Notes,
            WorkoutDate = request.WorkoutDate
        };

        var workoutLogId = await _mediator.Send(command);
        _logger.LogInformation("WorkoutLog {WorkoutLogId} created for athlete {AthleteId}", workoutLogId, request.AthleteId);

        return CreatedAtAction(nameof(LogWorkout), new { id = workoutLogId }, new { Id = workoutLogId });
    }
}

/// <summary>
/// Controller for athlete-specific workout operations.
/// </summary>
[ApiController]
[Route("api/athletes/{athleteId:guid}/workouts")]
[Produces("application/json")]
public class AthleteWorkoutsController : ControllerBase
{
    private readonly IMediator _mediator;

    public AthleteWorkoutsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get all workouts for a specific athlete.
    /// </summary>
    /// <param name="athleteId">The athlete's unique identifier.</param>
    /// <returns>List of workouts for the athlete.</returns>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByAthlete(Guid athleteId)
    {
        var query = new GetAthleteWorkoutsQuery { AthleteId = athleteId };
        var workouts = await _mediator.Send(query);
        return Ok(workouts);
    }
}

/// <summary>
/// Request model for logging a workout.
/// </summary>
public record LogWorkoutRequest(
    Guid AthleteId,
    Guid ExerciseId,
    int Sets,
    int Reps,
    decimal? Weight,
    int? RPE,
    string? Notes,
    DateTime WorkoutDate);
