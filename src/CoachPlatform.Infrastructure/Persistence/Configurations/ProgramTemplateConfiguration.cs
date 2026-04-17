using CoachPlatform.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoachPlatform.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for the ProgramTemplate entity.
/// </summary>
public class ProgramTemplateConfiguration : IEntityTypeConfiguration<ProgramTemplate>
{
    public void Configure(EntityTypeBuilder<ProgramTemplate> builder)
    {
        builder.ToTable("ProgramTemplates");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(p => p.Description)
            .HasMaxLength(2000);

        builder.Property(p => p.DurationWeeks)
            .IsRequired();

        builder.Property(p => p.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        // Relationships
        builder.HasOne(p => p.Coach)
            .WithMany()
            .HasForeignKey(p => p.CoachId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(p => p.Weeks)
            .WithOne(w => w.ProgramTemplate)
            .HasForeignKey(w => w.ProgramTemplateId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(p => p.CoachId)
            .HasDatabaseName("IX_ProgramTemplates_CoachId");

        builder.HasIndex(p => new { p.CoachId, p.Name })
            .IsUnique()
            .HasDatabaseName("IX_ProgramTemplates_CoachId_Name");

        builder.HasIndex(p => new { p.CoachId, p.IsActive })
            .HasDatabaseName("IX_ProgramTemplates_CoachId_IsActive");
    }
}
