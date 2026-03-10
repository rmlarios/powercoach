using CoachPlatform.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoachPlatform.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for the TrainingCycle entity.
/// </summary>
public class TrainingCycleConfiguration : IEntityTypeConfiguration<TrainingCycle>
{
    public void Configure(EntityTypeBuilder<TrainingCycle> builder)
    {
        builder.ToTable("TrainingCycles");

        builder.HasKey(tc => tc.Id);

        builder.Property(tc => tc.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(tc => tc.DurationWeeks)
            .IsRequired();

        builder.Property(tc => tc.StartDate)
            .IsRequired();

        builder.Property(tc => tc.EndDate)
            .IsRequired();

        // Relationships
        builder.HasOne(tc => tc.Athlete)
            .WithMany(a => a.TrainingCycles)
            .HasForeignKey(tc => tc.AthleteId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(tc => tc.AthleteId)
            .HasDatabaseName("IX_TrainingCycles_AthleteId");

        builder.HasIndex(tc => new { tc.AthleteId, tc.StartDate })
            .HasDatabaseName("IX_TrainingCycles_AthleteId_StartDate");
    }
}
