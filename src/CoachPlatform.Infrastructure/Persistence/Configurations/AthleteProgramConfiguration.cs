using CoachPlatform.Domain.Entities;
using CoachPlatform.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoachPlatform.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for the AthleteProgram entity.
/// </summary>
public class AthleteProgramConfiguration : IEntityTypeConfiguration<AthleteProgram>
{
    public void Configure(EntityTypeBuilder<AthleteProgram> builder)
    {
        builder.ToTable("AthletePrograms");

        builder.HasKey(ap => ap.Id);

        builder.Property(ap => ap.StartDate)
            .IsRequired();

        builder.Property(ap => ap.EndDate);

        builder.Property(ap => ap.CurrentWeek)
            .IsRequired()
            .HasDefaultValue(1);

        builder.Property(ap => ap.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(ap => ap.Notes)
            .HasMaxLength(2000);

        // Relationships
        builder.HasOne(ap => ap.Athlete)
            .WithMany()
            .HasForeignKey(ap => ap.AthleteId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(ap => ap.ProgramTemplate)
            .WithMany()
            .HasForeignKey(ap => ap.ProgramTemplateId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(ap => ap.Workouts)
            .WithOne(w => w.AthleteProgram)
            .HasForeignKey(w => w.AthleteProgramId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(ap => ap.AthleteId)
            .HasDatabaseName("IX_AthletePrograms_AthleteId");

        builder.HasIndex(ap => ap.ProgramTemplateId)
            .HasDatabaseName("IX_AthletePrograms_ProgramTemplateId");

        builder.HasIndex(ap => new { ap.AthleteId, ap.Status })
            .HasDatabaseName("IX_AthletePrograms_AthleteId_Status");

        builder.HasIndex(ap => ap.StartDate)
            .HasDatabaseName("IX_AthletePrograms_StartDate");
    }
}
