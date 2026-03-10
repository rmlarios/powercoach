using CoachPlatform.Application.Features.Exercises.Commands.CreateExercise;
using CoachPlatform.Application.Features.Exercises.Queries.GetExerciseById;
using CoachPlatform.Application.Features.Exercises.Queries.GetExercisesByCoach;
using CoachPlatform.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CoachPlatform.API.Controllers;

/// <summary>
/// API endpoints for managing exercises in the coach's library.
/// </summary>
[ApiController]
[Route("api/coaches/{coachId:guid}/exercises")]
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
    /// Get all exercises for a coach.
    /// </summary>
    /// <param name="coachId">The coach's unique identifier.</param>
    /// <param name="category">Optional filter by category.</param>
    /// <param name="muscleGroup">Optional filter by muscle group.</param>
    /// <param name="isActive">Optional filter by active status.</param>
    /// <returns>List of exercises.</returns>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByCoach(
        Guid coachId,
        [FromQuery] ExerciseCategory? category = null,
        [FromQuery] MuscleGroup? muscleGroup = null,
        [FromQuery] bool? isActive = null)
    {
        var query = new GetExercisesByCoachQuery
        {
            CoachId = coachId,
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
    /// <param name="coachId">The coach's unique identifier.</param>
    /// <param name="exerciseId">The exercise's unique identifier.</param>
    /// <returns>The exercise details.</returns>
    [HttpGet("{exerciseId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid coachId, Guid exerciseId)
    {
        var query = new GetExerciseByIdQuery { ExerciseId = exerciseId };
        var exercise = await _mediator.Send(query);

        if (exercise is null)
        {
            return NotFound();
        }

        // Verify the exercise belongs to the coach
        if (exercise.CoachId != coachId)
        {
            return NotFound();
        }

        return Ok(exercise);
    }

    /// <summary>
    /// Create a new exercise.
    /// </summary>
    /// <param name="coachId">The coach's unique identifier.</param>
    /// <param name="request">The exercise data.</param>
    /// <returns>The created exercise ID.</returns>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create(Guid coachId, [FromBody] CreateExerciseRequest request)
    {
        var command = new CreateExerciseCommand
        {
            CoachId = coachId,
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
        _logger.LogInformation("Exercise {ExerciseId} created for coach {CoachId}", exerciseId, coachId);

        return CreatedAtAction(nameof(GetById), new { coachId, exerciseId }, new { Id = exerciseId });
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
