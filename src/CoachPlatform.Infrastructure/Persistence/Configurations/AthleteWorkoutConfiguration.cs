using CoachPlatform.Domain.Entities;
using CoachPlatform.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoachPlatform.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for the AthleteWorkout entity.
/// </summary>
public class AthleteWorkoutConfiguration : IEntityTypeConfiguration<AthleteWorkout>
{
    public void Configure(EntityTypeBuilder<AthleteWorkout> builder)
    {
        builder.ToTable("AthleteWorkouts");

        builder.HasKey(w => w.Id);

        builder.Property(w => w.WeekNumber)
            .IsRequired();

        builder.Property(w => w.DayNumber)
            .IsRequired();

        builder.Property(w => w.ScheduledDate)
            .IsRequired();

        builder.Property(w => w.Status)
            .IsRequired()
            .HasDefaultValue(WorkoutStatus.NotStarted)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(w => w.StartedAt);

        builder.Property(w => w.CompletedDate);

        builder.Property(w => w.IsCompleted)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(w => w.DurationMinutes);

        builder.Property(w => w.FatigueRating);

        builder.Property(w => w.SkippedReason)
            .HasMaxLength(500);

        builder.Property(w => w.Notes)
            .HasMaxLength(2000);

        // Relationships
        builder.HasOne(w => w.AthleteProgram)
            .WithMany(ap => ap.Workouts)
            .HasForeignKey(w => w.AthleteProgramId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(w => w.ExerciseLogs)
            .WithOne(l => l.Workout)
            .HasForeignKey(l => l.WorkoutId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(w => w.AthleteProgramId)
            .HasDatabaseName("IX_AthleteWorkouts_AthleteProgramId");

        builder.HasIndex(w => new { w.AthleteProgramId, w.WeekNumber, w.DayNumber })
            .IsUnique()
            .HasDatabaseName("IX_AthleteWorkouts_AthleteProgramId_Week_Day");

        builder.HasIndex(w => w.ScheduledDate)
            .HasDatabaseName("IX_AthleteWorkouts_ScheduledDate");

        builder.HasIndex(w => new { w.AthleteProgramId, w.IsCompleted })
            .HasDatabaseName("IX_AthleteWorkouts_AthleteProgramId_IsCompleted");
    }
}
