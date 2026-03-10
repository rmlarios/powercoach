using CoachPlatform.Application.Features.CheckIns.Commands.CreateCheckIn;
using CoachPlatform.Application.Features.CheckIns.Queries.GetAthleteCheckIns;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CoachPlatform.API.Controllers;

/// <summary>
/// API endpoints for managing check-ins.
/// </summary>
[ApiController]
[Route("api/[controller]")]
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
    /// <param name="request">The check-in data.</param>
    /// <returns>The created check-in ID.</returns>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
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
        _logger.LogInformation("CheckIn {CheckInId} created for athlete {AthleteId}", checkInId, request.AthleteId);
        
        return CreatedAtAction(nameof(Create), new { id = checkInId }, new { Id = checkInId });
    }
}

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
