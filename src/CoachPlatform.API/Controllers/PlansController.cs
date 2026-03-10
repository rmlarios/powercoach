using CoachPlatform.Application.Features.Plans.Commands.CreatePlan;
using CoachPlatform.Application.Features.Plans.Commands.DeactivatePlan;
using CoachPlatform.Application.Features.Plans.Commands.UpdatePlan;
using CoachPlatform.Application.Features.Plans.Queries.GetPlans;
using CoachPlatform.Application.Shared.DTOs;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CoachPlatform.API.Controllers;

/// <summary>
/// API endpoints for managing plans.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class PlansController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<PlansController> _logger;

    public PlansController(IMediator mediator, ILogger<PlansController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Get plans for a specific coach.
    /// </summary>
    /// <param name="coachId">The coach's unique identifier.</param>
    /// <param name="isActive">Optional filter by active status.</param>
    /// <returns>List of plans.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<PlanListItemDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<PlanListItemDto>>> GetByCoach(
        [FromQuery] Guid coachId,
        [FromQuery] bool? isActive = null)
    {
        var query = new GetPlansQuery
        {
            CoachId = coachId,
            IsActive = isActive
        };

        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Create a new plan.
    /// </summary>
    /// <param name="command">The plan creation data.</param>
    /// <returns>The created plan's ID.</returns>
    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Guid>> Create([FromBody] CreatePlanCommand command)
    {
        var planId = await _mediator.Send(command);
        
        _logger.LogInformation("Created plan {PlanId} for coach {CoachId}", 
            planId, command.CoachId);

        return CreatedAtAction(
            nameof(GetByCoach),
            new { coachId = command.CoachId },
            planId);
    }

    /// <summary>
    /// Update an existing plan.
    /// </summary>
    /// <param name="id">The plan's unique identifier.</param>
    /// <param name="dto">The update data.</param>
    /// <returns>No content on success.</returns>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdatePlanDto dto)
    {
        var command = new UpdatePlanCommand
        {
            Id = id,
            Name = dto.Name,
            Description = dto.Description,
            Price = dto.Price,
            Currency = dto.Currency,
            DurationInMonths = dto.DurationDays / 30
        };

        await _mediator.Send(command);
        
        _logger.LogInformation("Updated plan {PlanId}", id);

        return NoContent();
    }

    /// <summary>
    /// Deactivate a plan (mark as inactive without deleting).
    /// </summary>
    /// <param name="id">The plan's unique identifier.</param>
    /// <returns>No content on success.</returns>
    [HttpPost("{id:guid}/deactivate")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Deactivate(Guid id)
    {
        var command = new DeactivatePlanCommand { Id = id };
        await _mediator.Send(command);
        
        _logger.LogInformation("Deactivated plan {PlanId}", id);

        return NoContent();
    }
}
