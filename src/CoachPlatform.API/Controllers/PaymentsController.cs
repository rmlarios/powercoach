using CoachPlatform.Application.Features.Payments.Commands.RegisterPayment;
using CoachPlatform.Application.Features.Payments.Queries.GetPayments;
using CoachPlatform.Application.Features.Payments.Queries.GetPaymentsByAthlete;
using CoachPlatform.Application.Shared.DTOs;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CoachPlatform.API.Controllers;

/// <summary>
/// API endpoints for managing payments.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class PaymentsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<PaymentsController> _logger;

    public PaymentsController(IMediator mediator, ILogger<PaymentsController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Get all payments with optional filters.
    /// </summary>
    /// <param name="athleteId">Optional athlete ID filter.</param>
    /// <param name="fromDate">Optional start date filter.</param>
    /// <param name="toDate">Optional end date filter.</param>
    /// <param name="paymentMethod">Optional payment method filter.</param>
    /// <returns>List of payments.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<PaymentListItemDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<PaymentListItemDto>>> GetPayments(
        [FromQuery] Guid? athleteId = null,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        [FromQuery] string? paymentMethod = null)
    {
        var query = new GetPaymentsQuery
        {
            AthleteId = athleteId,
            FromDate = fromDate,
            ToDate = toDate,
            PaymentMethod = paymentMethod
        };

        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Register a new payment.
    /// </summary>
    /// <param name="command">The payment registration data.</param>
    /// <returns>The created payment's ID.</returns>
    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Guid>> Register([FromBody] RegisterPaymentCommand command)
    {
        var paymentId = await _mediator.Send(command);
        
        _logger.LogInformation("Registered payment {PaymentId} for athlete {AthleteId}", 
            paymentId, command.AthleteId);

        return CreatedAtAction(
            nameof(GetPayments),
            new { athleteId = command.AthleteId },
            paymentId);
    }
}

/// <summary>
/// Controller for athlete-related payment endpoints.
/// </summary>
[ApiController]
[Route("api/athletes/{athleteId:guid}/payments")]
[Produces("application/json")]
public class AthletePaymentsController : ControllerBase
{
    private readonly IMediator _mediator;

    public AthletePaymentsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get all payments for a specific athlete.
    /// </summary>
    /// <param name="athleteId">The athlete's unique identifier.</param>
    /// <returns>List of payments.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<PaymentListItemDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<PaymentListItemDto>>> GetByAthlete(Guid athleteId)
    {
        var query = new GetPaymentsByAthleteQuery { AthleteId = athleteId };
        var result = await _mediator.Send(query);
        return Ok(result);
    }
}
