using CoachPlatform.Application.Features.Subscriptions.Commands.CancelSubscription;
using CoachPlatform.Application.Features.Subscriptions.Commands.CreateSubscription;
using CoachPlatform.Application.Features.Subscriptions.Queries.GetAthleteSubscriptions;
using CoachPlatform.Application.Shared.DTOs;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CoachPlatform.API.Controllers;

/// <summary>
/// API endpoints for managing subscriptions.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class SubscriptionsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<SubscriptionsController> _logger;

    public SubscriptionsController(IMediator mediator, ILogger<SubscriptionsController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Create a new subscription for an athlete.
    /// </summary>
    /// <param name="command">The subscription creation data.</param>
    /// <returns>The created subscription's ID.</returns>
    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Guid>> Create([FromBody] CreateSubscriptionCommand command)
    {
        var subscriptionId = await _mediator.Send(command);
        
        _logger.LogInformation("Created subscription {SubscriptionId} for athlete {AthleteId}", 
            subscriptionId, command.AthleteId);

        return CreatedAtAction(
            nameof(AthletesController.GetById),
            "Athletes",
            new { id = command.AthleteId },
            subscriptionId);
    }

    /// <summary>
    /// Cancel a subscription.
    /// </summary>
    /// <param name="id">The subscription's unique identifier.</param>
    /// <param name="dto">The cancellation data.</param>
    /// <returns>No content on success.</returns>
    [HttpPost("{id:guid}/cancel")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Cancel(Guid id, [FromBody] CancelSubscriptionDto? dto)
    {
        var command = new CancelSubscriptionCommand
        {
            Id = id,
            Reason = dto?.Reason
        };

        await _mediator.Send(command);
        
        _logger.LogInformation("Cancelled subscription {SubscriptionId}", id);

        return NoContent();
    }
}

/// <summary>
/// Controller for athlete-related subscription endpoints.
/// </summary>
[ApiController]
[Route("api/athletes/{athleteId:guid}/subscriptions")]
[Produces("application/json")]
public class AthleteSubscriptionsController : ControllerBase
{
    private readonly IMediator _mediator;

    public AthleteSubscriptionsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get all subscriptions for a specific athlete.
    /// </summary>
    /// <param name="athleteId">The athlete's unique identifier.</param>
    /// <returns>List of subscriptions.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<SubscriptionListItemDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<SubscriptionListItemDto>>> GetByAthlete(Guid athleteId)
    {
        var query = new GetAthleteSubscriptionsQuery { AthleteId = athleteId };
        var result = await _mediator.Send(query);
        return Ok(result);
    }
}
