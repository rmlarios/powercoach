using CoachPlatform.Application.Features.Applications.Commands.ApproveApplication;
using CoachPlatform.Application.Features.Applications.Commands.CreateApplication;
using CoachPlatform.Application.Features.Applications.Commands.RejectApplication;
using CoachPlatform.Application.Features.Applications.Queries.GetApplicationById;
using CoachPlatform.Application.Features.Applications.Queries.GetApplications;
using CoachPlatform.Application.Shared.DTOs;
using CoachPlatform.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CoachPlatform.API.Controllers;

/// <summary>
/// API endpoints for managing applications (postulaciones).
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ApplicationsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<ApplicationsController> _logger;

    public ApplicationsController(IMediator mediator, ILogger<ApplicationsController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Get applications with optional filtering and pagination.
    /// </summary>
    /// <param name="coachId">The coach's unique identifier.</param>
    /// <param name="status">Optional status filter (Pending, UnderReview, Accepted, Rejected, Withdrawn).</param>
    /// <param name="email">Optional email filter (partial match).</param>
    /// <param name="fromDate">Optional start date filter.</param>
    /// <param name="toDate">Optional end date filter.</param>
    /// <param name="searchTerm">Optional search term for name/email.</param>
    /// <param name="pageNumber">Page number (default: 1).</param>
    /// <param name="pageSize">Items per page (default: 20, max: 100).</param>
    /// <returns>Paginated list of applications.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<ApplicationListItemDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<ApplicationListItemDto>>> GetApplications(
        [FromQuery] Guid coachId,
        [FromQuery] ApplicationStatus? status = null,
        [FromQuery] string? email = null,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        [FromQuery] string? searchTerm = null,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20)
    {
        var query = new GetApplicationsQuery
        {
            CoachId = coachId,
            Status = status,
            Email = email,
            FromDate = fromDate,
            ToDate = toDate,
            SearchTerm = searchTerm,
            PageNumber = pageNumber,
            PageSize = Math.Min(pageSize, 100)
        };

        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Get a specific application by ID.
    /// </summary>
    /// <param name="id">The application's unique identifier.</param>
    /// <returns>The application details.</returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApplicationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApplicationDto>> GetById(Guid id)
    {
        var query = new GetApplicationByIdQuery { Id = id };
        var result = await _mediator.Send(query);

        if (result is null)
        {
            return NotFound();
        }

        return Ok(result);
    }

    /// <summary>
    /// Create a new application (postulación).
    /// </summary>
    /// <param name="command">The application data.</param>
    /// <returns>The created application's ID.</returns>
    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<Guid>> Create([FromBody] CreateApplicationCommand command)
    {
        var applicationId = await _mediator.Send(command);

        _logger.LogInformation(
            "Created application {ApplicationId} for coach {CoachId}", 
            applicationId, command.CoachId);

        return CreatedAtAction(
            nameof(GetById),
            new { id = applicationId },
            applicationId);
    }

    /// <summary>
    /// Approve an application and create an athlete.
    /// </summary>
    /// <param name="id">The application's unique identifier.</param>
    /// <param name="dto">Optional approval notes.</param>
    /// <returns>The approval result with application and athlete IDs.</returns>
    [HttpPost("{id:guid}/approve")]
    [ProducesResponseType(typeof(ApproveApplicationResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(object), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ApproveApplicationResult>> Approve(
        Guid id, 
        [FromBody] ApproveApplicationDto? dto = null)
    {
        var command = new ApproveApplicationCommand
        {
            ApplicationId = id,
            Notes = dto?.Notes
        };

        var result = await _mediator.Send(command);

        _logger.LogInformation(
            "Approved application {ApplicationId}, created athlete {AthleteId}", 
            result.ApplicationId, result.AthleteId);

        return Ok(result);
    }

    /// <summary>
    /// Reject an application.
    /// </summary>
    /// <param name="id">The application's unique identifier.</param>
    /// <param name="dto">Optional rejection reason and notes.</param>
    /// <returns>The rejected application's ID.</returns>
    [HttpPost("{id:guid}/reject")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(object), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<Guid>> Reject(
        Guid id, 
        [FromBody] RejectApplicationDto? dto = null)
    {
        var command = new RejectApplicationCommand
        {
            ApplicationId = id,
            Reason = dto?.Reason,
            Notes = dto?.Notes
        };

        var result = await _mediator.Send(command);

        _logger.LogInformation("Rejected application {ApplicationId}", result);

        return Ok(result);
    }
}
