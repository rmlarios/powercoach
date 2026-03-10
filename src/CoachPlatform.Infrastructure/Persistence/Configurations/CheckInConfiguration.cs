using CoachPlatform.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoachPlatform.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for the CheckIn entity.
/// </summary>
public class CheckInConfiguration : IEntityTypeConfiguration<CheckIn>
{
    public void Configure(EntityTypeBuilder<CheckIn> builder)
    {
        builder.ToTable("CheckIns");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.CheckInDate)
            .IsRequired();

        builder.Property(c => c.Weight)
            .HasColumnType("decimal(6,2)");

        builder.Property(c => c.Notes)
            .HasMaxLength(2000);

        // Store PhotoUrls as JSON/delimited string
        builder.Property(c => c.PhotoUrls)
            .HasConversion(
                v => string.Join("||", v),
                v => v.Split("||", StringSplitOptions.RemoveEmptyEntries).ToList());

        builder.Property(c => c.CoachFeedback)
            .HasMaxLength(2000);

        builder.Property(c => c.EnergyLevel);

        builder.Property(c => c.SleepHours)
            .HasColumnType("decimal(4,1)");

        builder.Property(c => c.SleepQuality);

        builder.Property(c => c.StressLevel);

        // Relationships
        builder.HasOne(c => c.Athlete)
            .WithMany(a => a.CheckIns)
            .HasForeignKey(c => c.AthleteId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(c => c.AthleteId);
        builder.HasIndex(c => c.CheckInDate);
        builder.HasIndex(c => new { c.AthleteId, c.CheckInDate });
    }
}
