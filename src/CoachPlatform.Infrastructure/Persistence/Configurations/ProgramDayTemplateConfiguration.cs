using CoachPlatform.Domain.Entities;
using CoachPlatform.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoachPlatform.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for the ProgramDayTemplate entity.
/// </summary>
public class ProgramDayTemplateConfiguration : IEntityTypeConfiguration<ProgramDayTemplate>
{
    public void Configure(EntityTypeBuilder<ProgramDayTemplate> builder)
    {
        builder.ToTable("ProgramDayTemplates");

        builder.HasKey(d => d.Id);

        builder.Property(d => d.DayNumber)
            .IsRequired();

        builder.Property(d => d.Name)
            .HasMaxLength(200);

        builder.Property(d => d.Focus)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(d => d.Notes)
            .HasMaxLength(2000);

        // Relationships
        builder.HasOne(d => d.WeekTemplate)
            .WithMany(w => w.Days)
            .HasForeignKey(d => d.WeekTemplateId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(d => d.Exercises)
            .WithOne(e => e.DayTemplate)
            .HasForeignKey(e => e.DayTemplateId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(d => d.WeekTemplateId)
            .HasDatabaseName("IX_ProgramDayTemplates_WeekTemplateId");

        builder.HasIndex(d => new { d.WeekTemplateId, d.DayNumber })
            .IsUnique()
            .HasDatabaseName("IX_ProgramDayTemplates_WeekTemplateId_DayNumber");
    }
}
