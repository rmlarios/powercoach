using CoachPlatform.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoachPlatform.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for the ProgramWeekTemplate entity.
/// </summary>
public class ProgramWeekTemplateConfiguration : IEntityTypeConfiguration<ProgramWeekTemplate>
{
    public void Configure(EntityTypeBuilder<ProgramWeekTemplate> builder)
    {
        builder.ToTable("ProgramWeekTemplates");

        builder.HasKey(w => w.Id);

        builder.Property(w => w.WeekNumber)
            .IsRequired();

        builder.Property(w => w.Name)
            .HasMaxLength(200);

        builder.Property(w => w.Notes)
            .HasMaxLength(1000);

        // Relationships
        builder.HasOne(w => w.ProgramTemplate)
            .WithMany(p => p.Weeks)
            .HasForeignKey(w => w.ProgramTemplateId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(w => w.Days)
            .WithOne(d => d.WeekTemplate)
            .HasForeignKey(d => d.WeekTemplateId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(w => w.ProgramTemplateId)
            .HasDatabaseName("IX_ProgramWeekTemplates_ProgramTemplateId");

        builder.HasIndex(w => new { w.ProgramTemplateId, w.WeekNumber })
            .IsUnique()
            .HasDatabaseName("IX_ProgramWeekTemplates_ProgramTemplateId_WeekNumber");
    }
}
