using CoachPlatform.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoachPlatform.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for the AthleteMaxLift entity.
/// </summary>
public class AthleteMaxLiftConfiguration : IEntityTypeConfiguration<AthleteMaxLift>
{
    public void Configure(EntityTypeBuilder<AthleteMaxLift> builder)
    {
        builder.ToTable("AthleteMaxLifts");

        builder.HasKey(m => m.Id);

        builder.Property(m => m.Weight)
            .IsRequired()
            .HasPrecision(10, 2);

        builder.Property(m => m.IsTested)
            .IsRequired();

        builder.Property(m => m.RecordedAt)
            .IsRequired();

        builder.Property(m => m.Notes)
            .HasMaxLength(500);

        builder.Property(m => m.EstimationDetails)
            .HasMaxLength(500);

        // Relationships
        builder.HasOne(m => m.Athlete)
            .WithMany()
            .HasForeignKey(m => m.AthleteId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(m => m.Exercise)
            .WithMany()
            .HasForeignKey(m => m.ExerciseId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(m => m.AthleteId)
            .HasDatabaseName("IX_AthleteMaxLifts_AthleteId");

        builder.HasIndex(m => m.ExerciseId)
            .HasDatabaseName("IX_AthleteMaxLifts_ExerciseId");

        // Composite index for finding latest max by athlete and exercise
        builder.HasIndex(m => new { m.AthleteId, m.ExerciseId, m.RecordedAt })
            .HasDatabaseName("IX_AthleteMaxLifts_Athlete_Exercise_Date")
            .IsDescending(false, false, true);
    }
}
