using CoachPlatform.Application.Features.CheckIns.Commands.CreateCheckIn;
using CoachPlatform.Application.Features.CheckIns.Commands.ReviewCheckIn;
using CoachPlatform.Application.Features.CheckIns.Queries.GetAthleteCheckIns;
using CoachPlatform.Application.Features.CheckIns.Queries.GetCheckInById;
using CoachPlatform.Application.Features.CheckIns.Queries.GetCoachCheckIns;
using CoachPlatform.Application.Shared.DTOs;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CoachPlatform.API.Controllers;

/// <summary>
/// API endpoints for managing check-ins.
/// </summary>
[ApiController]
[Route("api/check-ins")]
[Produces("application/json")]
public class CheckInsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<CheckInsController> _logger;

    public CheckInsController(IMediator mediator, ILogger<CheckInsController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Create a new check-in for an athlete.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] CreateCheckInRequest request)
    {
        var command = new CreateCheckInCommand
        {
            AthleteId = request.AthleteId,
            Weight = request.Weight,
            FatigueLevel = request.FatigueLevel,
            SleepQuality = request.SleepQuality,
            MotivationLevel = request.MotivationLevel,
            Notes = request.Notes
        };

        var checkInId = await _mediator.Send(command);
        return CreatedAtAction(nameof(Create), new { id = checkInId }, new { Id = checkInId });
    }

    /// <summary>
    /// Get all check-ins for a coach's athletes.
    /// </summary>
    [HttpGet("coach")]
    [ProducesResponseType(typeof(PagedResult<CheckInDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCoachCheckIns([FromQuery] Guid coachId, [FromQuery] bool? isReviewed, [FromQuery] int page = 1, [FromQuery] int size = 10)
    {
        var query = new GetCoachCheckInsQuery
        {
            CoachId = coachId,
            IsReviewed = isReviewed,
            PageNumber = page,
            PageSize = size
        };

        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Get a check-in by id.
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(CheckInDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetById(Guid id, [FromQuery] Guid coachId)
    {
        var query = new GetCheckInByIdQuery
        {
            CheckInId = id,
            CoachId = coachId
        };
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Review and add feedback to a check-in.
    /// </summary>
    [HttpPut("{id}/review")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> ReviewCheckIn(Guid id, [FromBody] ReviewCheckInRequest request)
    {
        var command = new ReviewCheckInCommand
        {
            CheckInId = id,
            CoachId = request.CoachId,
            Feedback = request.Feedback
        };

        await _mediator.Send(command);
        return NoContent();
    }
}

/// <summary>
/// Request model for reviewing a check-in.
/// </summary>
public record ReviewCheckInRequest(
    Guid CoachId,
    string Feedback);

/// <summary>
/// Controller for athlete-specific check-in operations.
/// </summary>
[ApiController]
[Route("api/athletes/{athleteId:guid}/checkins")]
[Produces("application/json")]
public class AthleteCheckInsController : ControllerBase
{
    private readonly IMediator _mediator;

    public AthleteCheckInsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get all check-ins for a specific athlete.
    /// </summary>
    /// <param name="athleteId">The athlete's unique identifier.</param>
    /// <returns>List of check-ins for the athlete.</returns>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByAthlete(Guid athleteId)
    {
        var query = new GetAthleteCheckInsQuery { AthleteId = athleteId };
        var checkIns = await _mediator.Send(query);
        return Ok(checkIns);
    }
}

/// <summary>
/// Request model for creating a check-in.
/// </summary>
public record CreateCheckInRequest(
    Guid AthleteId,
    decimal? Weight,
    int? FatigueLevel,
    int? SleepQuality,
    int? MotivationLevel,
    string? Notes);
