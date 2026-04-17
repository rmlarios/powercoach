using CoachPlatform.API.Middleware;
using CoachPlatform.Application;
using CoachPlatform.Infrastructure;
using CoachPlatform.Infrastructure.Persistence;
using CoachPlatform.Domain.Entities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Text.Json.Serialization;

// Allow Npgsql to accept DateTime with Kind=Unspecified (treats as UTC)
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// Configure JWT Authentication
var tokenSection = builder.Configuration.GetSection("TokenSettings");
var jwtSecret = tokenSection["Secret"] ?? throw new InvalidOperationException("TokenSettings:Secret not configured");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
            ValidateIssuer = true,
            ValidIssuer = tokenSection["Issuer"] ?? "CoachPlatform",
            ValidateAudience = true,
            ValidAudience = tokenSection["Audience"] ?? "CoachPlatformAPI",
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("CoachOnly", p => p.RequireRole("Coach"));
    options.AddPolicy("AthleteOnly", p => p.RequireRole("Athlete"));
    options.AddPolicy("AdminOnly", p => p.RequireRole("Admin"));
    options.AddPolicy("CoachOrAdmin", p => p.RequireRole("Coach", "Admin"));
});

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new()
    {
        Title = "Coach Platform API",
        Version = "v1",
        Description = "API for managing powerlifting/fitness coaching platform - athletes, subscriptions, payments, and check-ins."
    });

    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
        options.IncludeXmlComments(xmlPath);

    options.ResolveConflictingActions(apiDescriptions => apiDescriptions.First());

    // JWT support in Swagger UI
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Enter your JWT token"
    });
    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Add CORS for frontend applications
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
    ?? ["http://localhost:3000"];

// Ensure common dev ports are always allowed in Development
if (builder.Environment.IsDevelopment())
{
    var devOrigins = new HashSet<string>(allowedOrigins)
    {
        "http://localhost:3000",
        "http://localhost:3001",
        "http://localhost:3002",
        "http://localhost:3003",
        "http://localhost:5173"
    };
    allowedOrigins = devOrigins.ToArray();
}

Console.WriteLine($"CORS allowed origins: {string.Join(", ", allowedOrigins)}");

var isDev = builder.Environment.IsDevelopment();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        if (isDev)
        {
            // In Development, allow any localhost origin
            policy.SetIsOriginAllowed(origin =>
                    new Uri(origin).Host == "localhost")
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials();
        }
        else
        {
            policy.WithOrigins(allowedOrigins)
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials();
        }
    });
});

// Health checks
builder.Services.AddHealthChecks();

var app = builder.Build();

// Configure the HTTP request pipeline

// Global exception handling
app.UseExceptionHandling();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Coach Platform API v1");
        options.RoutePrefix = string.Empty;
    });
}

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseCors("AllowFrontend");
app.UseAuthentication();
app.UseAuthorization();

// Set tenant context based on authenticated user or X-Coach-Id header
app.UseTenantMiddleware();

app.MapControllers();
app.MapHealthChecks("/health");

// Auto-apply migrations and seed initial admin user
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await db.Database.MigrateAsync();

    if (!await db.Users.AnyAsync(u => u.Role == CoachPlatform.Domain.Entities.UserRole.Admin))
    {
        var adminHash = BCrypt.Net.BCrypt.HashPassword("Admin@123456");
        var admin = User.CreateAdmin("admin@powercoach.com", "admin", adminHash);
        db.Users.Add(admin);
        await db.SaveChangesAsync();
        Console.WriteLine("Seeded admin user: admin@powercoach.com / Admin@123456");
    }
}

app.Run();

// Make Program class accessible for integration tests
public partial class Program { }