using System.Net;
using System.Text.Json;
using CoachPlatform.Application.Shared.Exceptions;

namespace CoachPlatform.API.Middleware;

/// <summary>
/// Global exception handling middleware that transforms exceptions into proper API responses.
/// </summary>
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, response) = exception switch
        {
            ValidationException validationEx => (
                HttpStatusCode.BadRequest,
                new ErrorResponse
                {
                    Type = "ValidationError",
                    Title = "One or more validation errors occurred.",
                    Status = (int)HttpStatusCode.BadRequest,
                    Errors = validationEx.Errors
                }),
            NotFoundException notFoundEx => (
                HttpStatusCode.NotFound,
                new ErrorResponse
                {
                    Type = "NotFound",
                    Title = notFoundEx.Message,
                    Status = (int)HttpStatusCode.NotFound
                }),
            ConflictException conflictEx => (
                HttpStatusCode.Conflict,
                new ErrorResponse
                {
                    Type = "Conflict",
                    Title = conflictEx.Message,
                    Status = (int)HttpStatusCode.Conflict
                }),
            ForbiddenAccessException forbiddenEx => (
                HttpStatusCode.Forbidden,
                new ErrorResponse
                {
                    Type = "Forbidden",
                    Title = forbiddenEx.Message,
                    Status = (int)HttpStatusCode.Forbidden
                }),
            UnauthorizedAccessException => (
                HttpStatusCode.Unauthorized,
                new ErrorResponse
                {
                    Type = "Unauthorized",
                    Title = "Unauthorized access.",
                    Status = (int)HttpStatusCode.Unauthorized
                }),
            _ => (
                HttpStatusCode.InternalServerError,
                new ErrorResponse
                {
                    Type = "InternalServerError",
                    Title = "An unexpected error occurred: " + exception.Message + "\n" + exception.StackTrace,
                    Status = (int)HttpStatusCode.InternalServerError
                })
        };

        if (statusCode == HttpStatusCode.InternalServerError)
        {
            _logger.LogError(exception, "Unhandled exception: {Message}", exception.Message);
        }
        else
        {
            _logger.LogWarning("Handled exception: {Type} - {Message}", 
                exception.GetType().Name, exception.Message);
        }

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(response, options));
    }
}

/// <summary>
/// Standard error response format following RFC 7807.
/// </summary>
public class ErrorResponse
{
    public string Type { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public int Status { get; set; }
    public IDictionary<string, string[]>? Errors { get; set; }
}

/// <summary>
/// Extension methods for registering the exception handling middleware.
/// </summary>
public static class ExceptionHandlingMiddlewareExtensions
{
    public static IApplicationBuilder UseExceptionHandling(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<ExceptionHandlingMiddleware>();
    }
}
