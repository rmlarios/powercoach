using CoachPlatform.Application.Features.WorkoutLogs.Commands.CompleteSet;
using CoachPlatform.Application.Features.WorkoutLogs.Commands.CompleteWorkout;
using CoachPlatform.Application.Features.WorkoutLogs.Commands.LogWorkout;
using CoachPlatform.Application.Features.WorkoutLogs.Commands.SaveSet;
using CoachPlatform.Application.Features.WorkoutLogs.Commands.SkipWorkout;
using CoachPlatform.Application.Features.WorkoutLogs.Commands.StartWorkout;
using CoachPlatform.Application.Features.WorkoutLogs.Commands.UpdateSet;
using CoachPlatform.Application.Features.WorkoutLogs.Queries.GetAthleteWorkouts;
using CoachPlatform.Application.Features.WorkoutLogs.Queries.GetExerciseLiftHistory;
using CoachPlatform.Application.Features.WorkoutLogs.Queries.GetTodayWorkout;
using CoachPlatform.Application.Features.WorkoutLogs.Queries.GetWeekWorkouts;
using CoachPlatform.Application.Features.WorkoutLogs.Queries.GetWorkoutHistory;
using CoachPlatform.Application.Features.WorkoutLogs.Queries.GetWorkoutWithSets;
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
/// Controller for athlete-specific workout operations (tracking system).
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
    /// Get all legacy workout logs for a specific athlete.
    /// </summary>
    [HttpGet("logs")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetLegacyWorkouts(Guid athleteId)
    {
        var query = new GetAthleteWorkoutsQuery { AthleteId = athleteId };
        var workouts = await _mediator.Send(query);
        return Ok(workouts);
    }

    /// <summary>
    /// Get today's workout for an athlete (or the next upcoming one).
    /// </summary>
    [HttpGet("today")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> GetTodayWorkout(Guid athleteId)
    {
        var query = new GetTodayWorkoutQuery { AthleteId = athleteId };
        var workout = await _mediator.Send(query);

        if (workout is null)
            return NoContent();

        return Ok(workout);
    }

    /// <summary>
    /// Get a specific workout with all exercises and sets.
    /// </summary>
    [HttpGet("{workoutId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetWorkout(Guid athleteId, Guid workoutId)
    {
        var query = new GetWorkoutWithSetsQuery
        {
            AthleteId = athleteId,
            WorkoutId = workoutId
        };

        var workout = await _mediator.Send(query);

        if (workout is null)
            return NotFound();

        return Ok(workout);
    }

    /// <summary>
    /// Get all workouts for a specific week (for week-strip navigation).
    /// </summary>
    [HttpGet("week")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> GetWeekWorkouts(
        Guid athleteId,
        [FromQuery] int? weekNumber = null)
    {
        var query = new GetWeekWorkoutsQuery
        {
            AthleteId = athleteId,
            WeekNumber = weekNumber
        };

        var result = await _mediator.Send(query);

        if (result is null)
            return NoContent();

        return Ok(result);
    }

    /// <summary>
    /// Get lift history for a specific exercise (for exercise detail sheet).
    /// </summary>
    [HttpGet("exercises/{exerciseId:guid}/lift-history")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetExerciseLiftHistory(
        Guid athleteId,
        Guid exerciseId,
        [FromQuery] int limit = 20)
    {
        var query = new GetExerciseLiftHistoryQuery
        {
            AthleteId = athleteId,
            ExerciseId = exerciseId,
            Limit = limit
        };

        var result = await _mediator.Send(query);

        if (result is null)
            return NotFound();

        return Ok(result);
    }

    /// <summary>
    /// Get workout history for an athlete (paginated).
    /// </summary>
    [HttpGet("history")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetWorkoutHistory(
        Guid athleteId,
        [FromQuery] Guid? programId = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var query = new GetWorkoutHistoryQuery
        {
            AthleteId = athleteId,
            AthleteProgramId = programId,
            PageNumber = page,
            PageSize = pageSize
        };

        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Start a workout session.
    /// </summary>
    [HttpPost("{workoutId:guid}/start")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> StartWorkout(Guid athleteId, Guid workoutId)
    {
        var command = new StartWorkoutCommand
        {
            AthleteId = athleteId,
            WorkoutId = workoutId
        };

        await _mediator.Send(command);
        return Ok(new { message = "Workout started" });
    }

    /// <summary>
    /// Save (create or update) a specific set within a workout.
    /// </summary>
    [HttpPut("{workoutId:guid}/sets")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SaveSet(Guid athleteId, Guid workoutId, [FromBody] SaveSetRequest request)
    {
        var command = new SaveSetCommand
        {
            AthleteId = athleteId,
            WorkoutId = workoutId,
            ExerciseLogId = request.ExerciseLogId,
            ExerciseId = request.ExerciseId,
            SetNumber = request.SetNumber,
            Reps = request.Reps,
            Weight = request.Weight,
            Rpe = request.Rpe,
            TargetReps = request.TargetReps,
            TargetWeight = request.TargetWeight,
            IsCompleted = request.IsCompleted,
            Notes = request.Notes
        };

        var result = await _mediator.Send(command);
        return Ok(result);
    }

    /// <summary>
    /// Update an existing set's actual performance.
    /// </summary>
    [HttpPut("{workoutId:guid}/sets/{exerciseLogId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateSet(Guid athleteId, Guid workoutId, Guid exerciseLogId, [FromBody] UpdateSetRequest request)
    {
        var command = new UpdateSetCommand
        {
            AthleteId = athleteId,
            WorkoutId = workoutId,
            ExerciseLogId = exerciseLogId,
            Reps = request.Reps,
            Weight = request.Weight,
            Rpe = request.Rpe,
            Notes = request.Notes
        };

        await _mediator.Send(command);
        return Ok(new { message = "Set updated" });
    }

    /// <summary>
    /// Mark a specific set as completed.
    /// </summary>
    [HttpPost("{workoutId:guid}/sets/{exerciseLogId:guid}/complete")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CompleteSet(Guid athleteId, Guid workoutId, Guid exerciseLogId)
    {
        var command = new CompleteSetCommand
        {
            AthleteId = athleteId,
            WorkoutId = workoutId,
            ExerciseLogId = exerciseLogId
        };

        await _mediator.Send(command);
        return Ok(new { message = "Set completed" });
    }

    /// <summary>
    /// Skip the entire workout with an optional reason.
    /// </summary>
    [HttpPost("{workoutId:guid}/skip")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SkipWorkout(Guid athleteId, Guid workoutId, [FromBody] SkipWorkoutRequest? request = null)
    {
        var command = new SkipWorkoutCommand
        {
            AthleteId = athleteId,
            WorkoutId = workoutId,
            Reason = request?.Reason
        };

        await _mediator.Send(command);
        return Ok(new { message = "Workout skipped" });
    }

    /// <summary>
    /// Complete the entire workout session.
    /// </summary>
    [HttpPost("{workoutId:guid}/complete")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CompleteWorkout(Guid athleteId, Guid workoutId, [FromBody] CompleteWorkoutRequest? request = null)
    {
        var command = new CompleteWorkoutCommand
        {
            AthleteId = athleteId,
            WorkoutId = workoutId,
            DurationMinutes = request?.DurationMinutes,
            FatigueRating = request?.FatigueRating,
            Notes = request?.Notes
        };

        await _mediator.Send(command);
        return Ok(new { message = "Workout completed" });
    }
}

/// <summary>
/// Request model for logging a workout (legacy).
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

// ========================
// Workout Tracking Request DTOs (F-015)
// ========================

/// <summary>
/// Request model for saving (creating or updating) a set.
/// </summary>
public record SaveSetRequest(
    Guid? ExerciseLogId,
    Guid ExerciseId,
    int SetNumber,
    int Reps,
    decimal Weight,
    decimal? Rpe,
    int? TargetReps,
    decimal? TargetWeight,
    bool IsCompleted,
    string? Notes);

/// <summary>
/// Request model for updating an existing set.
/// </summary>
public record UpdateSetRequest(
    int Reps,
    decimal Weight,
    decimal? Rpe,
    string? Notes);

/// <summary>
/// Request model for skipping a workout.
/// </summary>
public record SkipWorkoutRequest(
    string? Reason);

/// <summary>
/// Request model for completing a workout.
/// </summary>
public record CompleteWorkoutRequest(
    int? DurationMinutes,
    int? FatigueRating,
    string? Notes);
