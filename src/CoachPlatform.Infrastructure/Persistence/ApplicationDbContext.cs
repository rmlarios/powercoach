using System.Reflection;
using CoachPlatform.Application.Shared.Interfaces;
using CoachPlatform.Domain.Common;
using CoachPlatform.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CoachPlatform.Infrastructure.Persistence;

/// <summary>
/// Entity Framework Core DbContext for the CoachPlatform application.
/// Implements IApplicationDbContext for dependency injection.
/// </summary>
public class ApplicationDbContext : DbContext, IApplicationDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) 
        : base(options)
    {
    }

    public DbSet<Coach> Coaches => Set<Coach>();
    public DbSet<Athlete> Athletes => Set<Athlete>();
    public DbSet<Domain.Entities.Application> Applications => Set<Domain.Entities.Application>();
    public DbSet<Plan> Plans => Set<Plan>();
    public DbSet<Subscription> Subscriptions => Set<Subscription>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<CheckIn> CheckIns => Set<CheckIn>();
    public DbSet<TrainingCycle> TrainingCycles => Set<TrainingCycle>();
    public DbSet<WorkoutLog> WorkoutLogs => Set<WorkoutLog>();
    public DbSet<Exercise> Exercises => Set<Exercise>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Apply all entity configurations from the current assembly
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        base.OnModelCreating(modelBuilder);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Update audit fields before saving
        UpdateAuditFields();
        
        return await base.SaveChangesAsync(cancellationToken);
    }

    public override int SaveChanges()
    {
        UpdateAuditFields();
        return base.SaveChanges();
    }

    /// <summary>
    /// Updates CreatedAt and UpdatedAt fields for auditable entities.
    /// </summary>
    private void UpdateAuditFields()
    {
        var entries = ChangeTracker
            .Entries<AuditableEntity>()
            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

        foreach (var entry in entries)
        {
            var now = DateTime.UtcNow;

            if (entry.State == EntityState.Added)
            {
                entry.Entity.GetType()
                    .GetProperty(nameof(AuditableEntity.CreatedAt))?
                    .SetValue(entry.Entity, now);
            }

            entry.Entity.GetType()
                .GetProperty(nameof(AuditableEntity.UpdatedAt))?
                .SetValue(entry.Entity, now);
        }
    }
}
