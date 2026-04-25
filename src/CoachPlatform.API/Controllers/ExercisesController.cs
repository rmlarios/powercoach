using CoachPlatform.Application.Features.Exercises.Commands.CreateExercise;
using CoachPlatform.Application.Features.Exercises.Commands.UpdateExercise;
using CoachPlatform.Application.Features.Exercises.Commands.DeleteExercise;
using CoachPlatform.Application.Features.Exercises.Queries.GetExerciseById;
using CoachPlatform.Application.Features.Exercises.Queries.GetExercisesByCoach;
using CoachPlatform.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CoachPlatform.API.Controllers;

/// <summary>
/// API endpoints for managing the global exercise catalog.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ExercisesController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<ExercisesController> _logger;

    public ExercisesController(IMediator mediator, ILogger<ExercisesController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Get all exercises from the global catalog.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] ExerciseCategory? category = null,
        [FromQuery] MuscleGroup? muscleGroup = null,
        [FromQuery] bool? isActive = null)
    {
        var query = new GetExercisesByCoachQuery
        {
            Category = category,
            MuscleGroup = muscleGroup,
            IsActive = isActive
        };

        var exercises = await _mediator.Send(query);
        return Ok(exercises);
    }

    /// <summary>
    /// Get a specific exercise by ID.
    /// </summary>
    [HttpGet("{exerciseId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid exerciseId)
    {
        var query = new GetExerciseByIdQuery { ExerciseId = exerciseId };
        var exercise = await _mediator.Send(query);

        if (exercise is null)
        {
            return NotFound();
        }

        return Ok(exercise);
    }

    /// <summary>
    /// Create a new exercise.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create([FromBody] CreateExerciseRequest request)
    {
        var command = new CreateExerciseCommand
        {
            Name = request.Name,
            Description = request.Description,
            Category = request.Category,
            PrimaryMuscleGroup = request.PrimaryMuscleGroup,
            SecondaryMuscleGroups = request.SecondaryMuscleGroups,
            VideoUrl = request.VideoUrl,
            Equipment = request.Equipment,
            IsCompound = request.IsCompound
        };

        var exerciseId = await _mediator.Send(command);
        _logger.LogInformation("Exercise {ExerciseId} created", exerciseId);

        return CreatedAtAction(nameof(GetById), new { exerciseId }, new { Id = exerciseId });
    }

    /// <summary>
    /// Update an existing exercise.
    /// </summary>
    [HttpPut("{exerciseId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(Guid exerciseId, [FromBody] UpdateExerciseRequest request)
    {
        var command = new UpdateExerciseCommand
        {
            ExerciseId = exerciseId,
            Name = request.Name,
            Description = request.Description,
            Category = request.Category,
            PrimaryMuscleGroup = request.PrimaryMuscleGroup,
            SecondaryMuscleGroups = request.SecondaryMuscleGroups,
            VideoUrl = request.VideoUrl,
            Equipment = request.Equipment,
            IsCompound = request.IsCompound,
        };

        await _mediator.Send(command);
        _logger.LogInformation("Exercise {ExerciseId} updated", exerciseId);

        return NoContent();
    }

    /// <summary>
    /// Delete (deactivate) an exercise.
    /// </summary>
    [HttpDelete("{exerciseId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid exerciseId)
    {
        var command = new DeleteExerciseCommand
        {
            ExerciseId = exerciseId,
        };

        await _mediator.Send(command);
        _logger.LogInformation("Exercise {ExerciseId} deactivated", exerciseId);

        return NoContent();
    }
}

/// <summary>
/// Request model for creating an exercise.
/// </summary>
public record CreateExerciseRequest(
    string Name,
    string? Description,
    ExerciseCategory Category,
    MuscleGroup PrimaryMuscleGroup,
    List<MuscleGroup>? SecondaryMuscleGroups,
    string? VideoUrl,
    string? Equipment,
    bool IsCompound = true);

/// <summary>
/// Request model for updating an exercise.
/// </summary>
public record UpdateExerciseRequest(
    string Name,
    string? Description,
    ExerciseCategory Category,
    MuscleGroup PrimaryMuscleGroup,
    List<MuscleGroup>? SecondaryMuscleGroups,
    string? VideoUrl,
    string? Equipment,
    bool IsCompound = true);
