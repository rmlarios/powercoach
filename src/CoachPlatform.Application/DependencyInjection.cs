using System.Reflection;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace CoachPlatform.Application;

/// <summary>
/// Dependency Injection configuration for the Application layer.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds Application layer services to the DI container.
    /// </summary>
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();

        // Register MediatR
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(assembly);
            cfg.AddOpenBehavior(typeof(Shared.Behaviors.ValidationBehavior<,>));
            cfg.AddOpenBehavior(typeof(Shared.Behaviors.LoggingBehavior<,>));
            cfg.AddOpenBehavior(typeof(Shared.Behaviors.TenantValidationBehavior<,>));
        });

        // Register FluentValidation validators
        services.AddValidatorsFromAssembly(assembly);

        return services;
    }
}
