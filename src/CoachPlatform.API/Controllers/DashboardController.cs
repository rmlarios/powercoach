using CoachPlatform.Application.Features.Dashboard.Queries.GetCoachDashboard;
using CoachPlatform.Application.Shared.DTOs;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CoachPlatform.API.Controllers;

/// <summary>
/// API endpoints for the coach dashboard.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class DashboardController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<DashboardController> _logger;

    public DashboardController(IMediator mediator, ILogger<DashboardController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Get the coach's intelligent dashboard — stats, alerts, athlete statuses, and activity feed.
    /// </summary>
    /// <param name="coachId">The coach's unique identifier.</param>
    /// <returns>The complete dashboard data.</returns>
    [HttpGet("coach")]
    [ProducesResponseType(typeof(CoachDashboardDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<CoachDashboardDto>> GetCoachDashboard([FromQuery] Guid coachId)
    {
        var query = new GetCoachDashboardQuery { CoachId = coachId };
        var result = await _mediator.Send(query);

        _logger.LogInformation("Dashboard loaded for coach {CoachId}: {ActiveAthletes} athletes, {AlertCount} alerts",
            coachId, result.Stats.ActiveAthletes, result.Alerts.Count);

        return Ok(result);
    }
}
