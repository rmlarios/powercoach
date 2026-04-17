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

    public DbSet<User> Users => Set<User>();
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
    
    // Training Programs module
    public DbSet<ProgramTemplate> ProgramTemplates => Set<ProgramTemplate>();
    public DbSet<ProgramWeekTemplate> ProgramWeekTemplates => Set<ProgramWeekTemplate>();
    public DbSet<ProgramDayTemplate> ProgramDayTemplates => Set<ProgramDayTemplate>();
    public DbSet<ProgramExerciseTemplate> ProgramExerciseTemplates => Set<ProgramExerciseTemplate>();
    public DbSet<AthleteProgram> AthletePrograms => Set<AthleteProgram>();
    public DbSet<AthleteWorkout> AthleteWorkouts => Set<AthleteWorkout>();
    public DbSet<AthleteExerciseLog> AthleteExerciseLogs => Set<AthleteExerciseLog>();
    public DbSet<AthleteMaxLift> AthleteMaxLifts => Set<AthleteMaxLift>();

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
        // Normalize all DateTime properties to UTC for PostgreSQL compatibility
        NormalizeDateTimesToUtc();
        
        return await base.SaveChangesAsync(cancellationToken);
    }

    public override int SaveChanges()
    {
        UpdateAuditFields();
        NormalizeDateTimesToUtc();
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

    /// <summary>
    /// Ensures all DateTime properties have Kind=Utc before saving to PostgreSQL.
    /// PostgreSQL 'timestamp with time zone' rejects DateTime with Kind=Unspecified.
    /// </summary>
    private void NormalizeDateTimesToUtc()
    {
        var entries = ChangeTracker.Entries()
            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

        foreach (var entry in entries)
        {
            foreach (var prop in entry.Properties)
            {
                if (prop.CurrentValue is DateTime dt && dt.Kind == DateTimeKind.Unspecified)
                {
                    prop.CurrentValue = DateTime.SpecifyKind(dt, DateTimeKind.Utc);
                }
            }
        }
    }
}
