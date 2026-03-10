using CoachPlatform.Application.Shared.Interfaces;

namespace CoachPlatform.API.Middleware;

/// <summary>
/// Middleware that establishes the tenant (Coach) context for each request.
/// In production, this will use the authenticated user's claims to determine the CoachId.
/// For development, it also supports X-Coach-Id header.
/// </summary>
public class TenantMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<TenantMiddleware> _logger;

    public TenantMiddleware(RequestDelegate next, ILogger<TenantMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, ITenantService tenantService, ICurrentUserService currentUserService)
    {
        Guid? coachId = null;

        // 1. Try to get CoachId from authenticated user claims
        if (currentUserService.IsAuthenticated)
        {
            // For coaches, the UserId is the CoachId
            // For athletes, we would need to look up their coach
            var coachIdClaim = context.User.FindFirst("coach_id")?.Value;
            
            if (!string.IsNullOrEmpty(coachIdClaim) && Guid.TryParse(coachIdClaim, out var parsedCoachId))
            {
                coachId = parsedCoachId;
            }
            else if (context.User.IsInRole("Coach") && currentUserService.UserId.HasValue)
            {
                // If user is a coach, their user ID is their coach ID
                coachId = currentUserService.UserId;
            }
        }

        // 2. Development fallback: X-Coach-Id header (should be disabled in production)
        if (!coachId.HasValue)
        {
            var headerValue = context.Request.Headers["X-Coach-Id"].FirstOrDefault();
            if (!string.IsNullOrEmpty(headerValue) && Guid.TryParse(headerValue, out var headerCoachId))
            {
                coachId = headerCoachId;
                _logger.LogDebug("Using CoachId from X-Coach-Id header: {CoachId}", coachId);
            }
        }

        // 3. Set tenant context if we have a CoachId
        if (coachId.HasValue)
        {
            tenantService.SetTenant(coachId.Value);
            _logger.LogDebug("Tenant context set to CoachId: {CoachId}", coachId.Value);
        }

        try
        {
            await _next(context);
        }
        finally
        {
            // Clear tenant context after request
            tenantService.ClearTenant();
        }
    }
}

/// <summary>
/// Extension methods for TenantMiddleware.
/// </summary>
public static class TenantMiddlewareExtensions
{
    /// <summary>
    /// Adds the tenant middleware to the application pipeline.
    /// </summary>
    public static IApplicationBuilder UseTenantMiddleware(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<TenantMiddleware>();
    }
}
