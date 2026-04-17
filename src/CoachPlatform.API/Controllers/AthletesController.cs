using CoachPlatform.Application.Features.Athletes.Commands.CreateAthlete;
using CoachPlatform.Application.Features.Athletes.Commands.DeactivateAthlete;
using CoachPlatform.Application.Features.Athletes.Commands.RegisterMaxLift;
using CoachPlatform.Application.Features.Athletes.Commands.UpdateAthlete;
using CoachPlatform.Application.Features.Athletes.Queries.GetAthleteById;
using CoachPlatform.Application.Features.Athletes.Queries.GetAthleteMaxLifts;
using CoachPlatform.Application.Features.Athletes.Queries.GetAthletesByCoach;
using CoachPlatform.Application.Features.Athletes.Queries.GetExerciseHistory;
using CoachPlatform.Application.Shared.DTOs;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CoachPlatform.API.Controllers;

/// <summary>
/// API endpoints for managing athletes.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class AthletesController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<AthletesController> _logger;

    public AthletesController(IMediator mediator, ILogger<AthletesController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Get athletes for a specific coach with optional filtering and pagination.
    /// </summary>
    /// <param name="coachId">The coach's unique identifier.</param>
    /// <param name="status">Optional status filter (Active, Inactive, OnHold, Graduated, Suspended).</param>
    /// <param name="email">Optional email filter.</param>
    /// <param name="country">Optional country filter.</param>
    /// <param name="searchTerm">Optional search term for name/email.</param>
    /// <param name="pageNumber">Page number (default: 1).</param>
    /// <param name="pageSize">Items per page (default: 20, max: 100).</param>
    /// <returns>Paginated list of athletes.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<AthleteListItemDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<AthleteListItemDto>>> GetByCoach(
        [FromQuery] Guid coachId,
        [FromQuery] string? status = null,
        [FromQuery] string? email = null,
        [FromQuery] string? country = null,
        [FromQuery] string? searchTerm = null,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20)
    {
        var query = new GetAthletesByCoachQuery
        {
            CoachId = coachId,
            Status = status,
            Email = email,
            Country = country,
            SearchTerm = searchTerm,
            PageNumber = pageNumber,
            PageSize = Math.Min(pageSize, 100)
        };

        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Get a specific athlete by ID.
    /// </summary>
    /// <param name="id">The athlete's unique identifier.</param>
    /// <returns>The athlete details.</returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(AthleteDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AthleteDetailDto>> GetById(Guid id)
    {
        var query = new GetAthleteByIdQuery { Id = id };
        var result = await _mediator.Send(query);
        
        if (result is null)
        {
            return NotFound();
        }
        
        return Ok(result);
    }

    /// <summary>
    /// Create a new athlete.
    /// </summary>
    /// <param name="command">The athlete creation data.</param>
    /// <returns>The created athlete's ID.</returns>
    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<Guid>> Create([FromBody] CreateAthleteCommand command)
    {
        var athleteId = await _mediator.Send(command);
        
        _logger.LogInformation("Created athlete {AthleteId} for coach {CoachId}", 
            athleteId, command.CoachId);

        return CreatedAtAction(
            nameof(GetById),
            new { id = athleteId },
            athleteId);
    }

    /// <summary>
    /// Update an existing athlete's physical and training data.
    /// </summary>
    /// <param name="id">The athlete's unique identifier.</param>
    /// <param name="dto">The update data.</param>
    /// <returns>No content on success.</returns>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateAthleteDto dto)
    {
        var command = new UpdateAthleteCommand
        {
            Id = id,
            Country = dto.Country,
            Height = dto.Height,
            Weight = dto.Weight,
            ExperienceLevel = dto.ExperienceLevel
        };

        await _mediator.Send(command);
        
        _logger.LogInformation("Updated athlete {AthleteId}", id);

        return NoContent();
    }

    /// <summary>
    /// Deactivate an athlete (mark as inactive without deleting).
    /// </summary>
    /// <param name="id">The athlete's unique identifier.</param>
    /// <returns>No content on success.</returns>
    [HttpPost("{id:guid}/deactivate")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Deactivate(Guid id)
    {
        var command = new DeactivateAthleteCommand { Id = id };
        await _mediator.Send(command);
        
        _logger.LogInformation("Deactivated athlete {AthleteId}", id);

        return NoContent();
    }

    // ==========================================
    // Max Lifts (1RM) endpoints
    // ==========================================

    /// <summary>
    /// Get all max lifts (1RM records) for an athlete.
    /// </summary>
    /// <param name="id">The athlete's unique identifier.</param>
    /// <param name="exerciseId">Optional filter by exercise.</param>
    /// <returns>The athlete's max lift records.</returns>
    [HttpGet("{id:guid}/max-lifts")]
    [ProducesResponseType(typeof(AthleteMaxLiftsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AthleteMaxLiftsDto>> GetMaxLifts(
        Guid id,
        [FromQuery] Guid? exerciseId = null)
    {
        var query = new GetAthleteMaxLiftsQuery
        {
            AthleteId = id,
            ExerciseId = exerciseId
        };

        var result = await _mediator.Send(query);

        if (result is null)
        {
            return NotFound();
        }

        return Ok(result);
    }

    /// <summary>
    /// Register a new max lift (1RM) for an athlete.
    /// </summary>
    /// <param name="id">The athlete's unique identifier.</param>
    /// <param name="dto">The max lift data.</param>
    /// <returns>The created max lift ID.</returns>
    [HttpPost("{id:guid}/max-lifts")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Guid>> RegisterMaxLift(
        Guid id,
        [FromBody] RegisterMaxLiftDto dto)
    {
        var command = new RegisterMaxLiftCommand
        {
            AthleteId = id,
            ExerciseId = dto.ExerciseId,
            Weight = dto.Weight,
            IsTested = dto.IsTested,
            RecordedAt = dto.RecordedAt,
            Notes = dto.Notes,
            EstimationDetails = dto.EstimationDetails
        };

        var maxLiftId = await _mediator.Send(command);

        _logger.LogInformation("Registered max lift {MaxLiftId} for athlete {AthleteId}",
            maxLiftId, id);

        return CreatedAtAction(
            nameof(GetMaxLifts),
            new { id },
            maxLiftId);
    }

    // ==========================================
    // Exercise History endpoints
    // ==========================================

    /// <summary>
    /// Get exercise history for an athlete.
    /// Returns historical performance data, 1RM records, and training suggestions.
    /// </summary>
    /// <param name="id">The athlete's unique identifier.</param>
    /// <param name="exerciseId">The exercise's unique identifier.</param>
    /// <returns>Exercise history with stats and suggestions.</returns>
    [HttpGet("{id:guid}/exercise-history/{exerciseId:guid}")]
    [ProducesResponseType(typeof(ExerciseHistoryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ExerciseHistoryDto>> GetExerciseHistory(
        Guid id,
        Guid exerciseId)
    {
        var query = new GetExerciseHistoryQuery
        {
            AthleteId = id,
            ExerciseId = exerciseId
        };

        var result = await _mediator.Send(query);

        if (result is null)
        {
            return NotFound();
        }

        return Ok(result);
    }
}

/// <summary>
/// DTO for registering a new max lift.
/// </summary>
public record RegisterMaxLiftDto
{
    public Guid ExerciseId { get; init; }
    public decimal Weight { get; init; }
    public bool IsTested { get; init; } = true;
    public DateTime? RecordedAt { get; init; }
    public string? Notes { get; init; }
    public string? EstimationDetails { get; init; }
}
