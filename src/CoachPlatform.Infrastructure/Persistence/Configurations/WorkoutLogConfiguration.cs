using CoachPlatform.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoachPlatform.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for the WorkoutLog entity.
/// </summary>
public class WorkoutLogConfiguration : IEntityTypeConfiguration<WorkoutLog>
{
    public void Configure(EntityTypeBuilder<WorkoutLog> builder)
    {
        builder.ToTable("WorkoutLogs");

        builder.HasKey(wl => wl.Id);

        builder.Property(wl => wl.ExerciseName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(wl => wl.Sets)
            .IsRequired();

        builder.Property(wl => wl.Reps)
            .IsRequired();

        builder.Property(wl => wl.Weight)
            .HasPrecision(10, 2);

        builder.Property(wl => wl.RPE);

        builder.Property(wl => wl.Notes)
            .HasMaxLength(1000);

        builder.Property(wl => wl.WorkoutDate)
            .IsRequired();

        // Relationships
        builder.HasOne(wl => wl.Athlete)
            .WithMany(a => a.WorkoutLogs)
            .HasForeignKey(wl => wl.AthleteId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(wl => wl.Exercise)
            .WithMany()
            .HasForeignKey(wl => wl.ExerciseId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(wl => wl.AthleteId)
            .HasDatabaseName("IX_WorkoutLogs_AthleteId");

        builder.HasIndex(wl => wl.ExerciseId)
            .HasDatabaseName("IX_WorkoutLogs_ExerciseId");

        builder.HasIndex(wl => new { wl.AthleteId, wl.WorkoutDate })
            .HasDatabaseName("IX_WorkoutLogs_AthleteId_WorkoutDate");

        builder.HasIndex(wl => new { wl.AthleteId, wl.ExerciseId, wl.WorkoutDate })
            .HasDatabaseName("IX_WorkoutLogs_AthleteId_ExerciseId_WorkoutDate");
    }
}
