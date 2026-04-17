using CoachPlatform.Application.Shared.Interfaces;
using CoachPlatform.Domain.Interfaces;
using CoachPlatform.Infrastructure.Persistence;
using CoachPlatform.Infrastructure.Persistence.Repositories;
using CoachPlatform.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CoachPlatform.Infrastructure;

/// <summary>
/// Extension methods for setting up Infrastructure services in the DI container.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds Infrastructure layer services to the specified IServiceCollection.
    /// </summary>
    /// <param name="services">The IServiceCollection to add services to.</param>
    /// <param name="configuration">The configuration instance.</param>
    /// <returns>The IServiceCollection for chaining.</returns>
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Register DbContext
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        
        services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseNpgsql(connectionString, npgsqlOptions =>
            {
                npgsqlOptions.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName);
                npgsqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 3,
                    maxRetryDelay: TimeSpan.FromSeconds(30),
                    errorCodesToAdd: null);
            });
        });

        // Register IApplicationDbContext
        services.AddScoped<IApplicationDbContext>(provider =>
            provider.GetRequiredService<ApplicationDbContext>());

        // Register repositories
        services.AddScoped<ICoachRepository, CoachRepository>();
        services.AddScoped<IAthleteRepository, AthleteRepository>();
        services.AddScoped<IApplicationRepository, ApplicationRepository>();
        services.AddScoped<IPlanRepository, PlanRepository>();
        services.AddScoped<ISubscriptionRepository, SubscriptionRepository>();
        services.AddScoped<IPaymentRepository, PaymentRepository>();
        services.AddScoped<ICheckInRepository, CheckInRepository>();
        services.AddScoped<CoachPlatform.Application.Shared.Interfaces.IUserRepository, UserRepository>();

        // Register Unit of Work
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Register JWT service
        services.AddScoped<IJwtService, JwtService>();

        // Register User and Tenant services
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<ITenantService, TenantService>();

        return services;
    }
}
