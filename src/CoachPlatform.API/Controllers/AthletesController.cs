using CoachPlatform.Application.Features.Athletes.Commands.CreateAthlete;
using CoachPlatform.Application.Features.Athletes.Commands.DeactivateAthlete;
using CoachPlatform.Application.Features.Athletes.Commands.UpdateAthlete;
using CoachPlatform.Application.Features.Athletes.Queries.GetAthleteById;
using CoachPlatform.Application.Features.Athletes.Queries.GetAthletesByCoach;
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
}
