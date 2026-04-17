using CoachPlatform.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoachPlatform.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for the AthleteExerciseLog entity.
/// </summary>
public class AthleteExerciseLogConfiguration : IEntityTypeConfiguration<AthleteExerciseLog>
{
    public void Configure(EntityTypeBuilder<AthleteExerciseLog> builder)
    {
        builder.ToTable("AthleteExerciseLogs");

        builder.HasKey(l => l.Id);

        builder.Property(l => l.SetNumber)
            .IsRequired();

        builder.Property(l => l.TargetReps);

        builder.Property(l => l.TargetWeight)
            .HasPrecision(10, 2);

        builder.Property(l => l.Reps)
            .IsRequired();

        builder.Property(l => l.Weight)
            .IsRequired()
            .HasPrecision(10, 2);

        builder.Property(l => l.Rpe)
            .HasPrecision(3, 1);

        builder.Property(l => l.IsCompleted)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(l => l.SkippedReason)
            .HasMaxLength(500);

        builder.Property(l => l.Notes)
            .HasMaxLength(500);

        // Relationships
        builder.HasOne(l => l.Workout)
            .WithMany(w => w.ExerciseLogs)
            .HasForeignKey(l => l.WorkoutId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(l => l.Exercise)
            .WithMany()
            .HasForeignKey(l => l.ExerciseId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(l => l.WorkoutId)
            .HasDatabaseName("IX_AthleteExerciseLogs_WorkoutId");

        builder.HasIndex(l => l.ExerciseId)
            .HasDatabaseName("IX_AthleteExerciseLogs_ExerciseId");

        builder.HasIndex(l => new { l.WorkoutId, l.ExerciseId, l.SetNumber })
            .HasDatabaseName("IX_AthleteExerciseLogs_WorkoutId_ExerciseId_SetNumber");
    }
}
