using CoachPlatform.Application.Shared.Exceptions;
using CoachPlatform.Application.Shared.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CoachPlatform.Application.Shared.Behaviors;

/// <summary>
/// MediatR pipeline behavior that validates tenant access for requests implementing ITenantRequest.
/// Ensures that the current tenant (Coach) has access to the requested resources.
/// </summary>
/// <typeparam name="TRequest">The type of request being handled.</typeparam>
/// <typeparam name="TResponse">The type of response from the handler.</typeparam>
public class TenantValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly ITenantService _tenantService;
    private readonly IApplicationDbContext _context;
    private readonly ILogger<TenantValidationBehavior<TRequest, TResponse>> _logger;

    public TenantValidationBehavior(
        ITenantService tenantService,
        IApplicationDbContext context,
        ILogger<TenantValidationBehavior<TRequest, TResponse>> logger)
    {
        _tenantService = tenantService;
        _context = context;
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        // Check if request requires tenant validation
        if (request is ITenantRequest tenantRequest)
        {
            await ValidateTenantAccess(tenantRequest, cancellationToken);
        }

        // Check if request operates on athlete-owned resource
        if (request is IAthleteOwnedRequest athleteRequest)
        {
            await ValidateAthleteAccess(athleteRequest, cancellationToken);
        }

        return await next();
    }

    private Task ValidateTenantAccess(ITenantRequest request, CancellationToken cancellationToken)
    {
        if (!_tenantService.HasTenant)
        {
            _logger.LogWarning("Tenant validation failed: No tenant context set for request {RequestType}", 
                typeof(TRequest).Name);
            throw new UnauthorizedAccessException("Tenant context is required for this operation.");
        }

        if (request.CoachId != _tenantService.CoachId)
        {
            _logger.LogWarning(
                "Tenant validation failed: Request CoachId {RequestCoachId} does not match current tenant {CurrentTenant}", 
                request.CoachId, _tenantService.CoachId);
            throw new ForbiddenAccessException("You do not have access to this resource.");
        }

        return Task.CompletedTask;
    }

    private async Task ValidateAthleteAccess(IAthleteOwnedRequest request, CancellationToken cancellationToken)
    {
        if (!_tenantService.HasTenant)
        {
            _logger.LogWarning("Tenant validation failed: No tenant context set for athlete request {RequestType}", 
                typeof(TRequest).Name);
            throw new UnauthorizedAccessException("Tenant context is required for this operation.");
        }

        // Verify the athlete belongs to the current tenant
        var athlete = await _context.Athletes
            .FindAsync(new object[] { request.AthleteId }, cancellationToken);

        if (athlete == null)
        {
            throw new NotFoundException("Athlete", request.AthleteId);
        }

        if (athlete.CoachId != _tenantService.CoachId)
        {
            _logger.LogWarning(
                "Tenant validation failed: Athlete {AthleteId} belongs to Coach {AthleteCoachId}, not current tenant {CurrentTenant}", 
                request.AthleteId, athlete.CoachId, _tenantService.CoachId);
            throw new ForbiddenAccessException("You do not have access to this athlete.");
        }
    }
}
