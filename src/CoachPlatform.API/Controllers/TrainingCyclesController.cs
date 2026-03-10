using CoachPlatform.Application.Features.TrainingCycles.Commands.CreateTrainingCycle;
using CoachPlatform.Application.Features.TrainingCycles.Queries.GetAthleteTrainingCycles;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CoachPlatform.API.Controllers;

/// <summary>
/// API endpoints for managing training cycles.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class TrainingCyclesController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<TrainingCyclesController> _logger;

    public TrainingCyclesController(IMediator mediator, ILogger<TrainingCyclesController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Create a new training cycle for an athlete.
    /// </summary>
    /// <param name="request">The training cycle data.</param>
    /// <returns>The created training cycle ID.</returns>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Create([FromBody] CreateTrainingCycleRequest request)
    {
        var command = new CreateTrainingCycleCommand
        {
            AthleteId = request.AthleteId,
            Name = request.Name,
            DurationWeeks = request.DurationWeeks,
            StartDate = request.StartDate
        };

        var trainingCycleId = await _mediator.Send(command);
        _logger.LogInformation("TrainingCycle {TrainingCycleId} created for athlete {AthleteId}", trainingCycleId, request.AthleteId);

        return CreatedAtAction(nameof(Create), new { id = trainingCycleId }, new { Id = trainingCycleId });
    }
}

/// <summary>
/// Controller for athlete-specific training cycle operations.
/// </summary>
[ApiController]
[Route("api/athletes/{athleteId:guid}/training-cycles")]
[Produces("application/json")]
public class AthleteTrainingCyclesController : ControllerBase
{
    private readonly IMediator _mediator;

    public AthleteTrainingCyclesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get all training cycles for a specific athlete.
    /// </summary>
    /// <param name="athleteId">The athlete's unique identifier.</param>
    /// <returns>List of training cycles for the athlete.</returns>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByAthlete(Guid athleteId)
    {
        var query = new GetAthleteTrainingCyclesQuery { AthleteId = athleteId };
        var trainingCycles = await _mediator.Send(query);
        return Ok(trainingCycles);
    }
}

/// <summary>
/// Request model for creating a training cycle.
/// </summary>
public record CreateTrainingCycleRequest(
    Guid AthleteId,
    string Name,
    int DurationWeeks,
    DateTime StartDate);
