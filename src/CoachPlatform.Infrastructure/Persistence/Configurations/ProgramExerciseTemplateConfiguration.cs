using CoachPlatform.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoachPlatform.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for the ProgramExerciseTemplate entity.
/// </summary>
public class ProgramExerciseTemplateConfiguration : IEntityTypeConfiguration<ProgramExerciseTemplate>
{
    public void Configure(EntityTypeBuilder<ProgramExerciseTemplate> builder)
    {
        builder.ToTable("ProgramExerciseTemplates");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Sets)
            .IsRequired();

        builder.Property(e => e.Reps)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(e => e.TargetRpe)
            .HasPrecision(3, 1);

        builder.Property(e => e.RestSeconds);

        builder.Property(e => e.Notes)
            .HasMaxLength(1000);

        builder.Property(e => e.Order)
            .IsRequired();

        builder.Property(e => e.ExerciseType)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20)
            .HasDefaultValue(CoachPlatform.Domain.Enums.ExerciseType.Standard);

        builder.Property(e => e.PercentageRM)
            .HasPrecision(5, 2);

        builder.Property(e => e.RawNotation)
            .HasMaxLength(200);

        builder.Property(e => e.Weight)
            .HasPrecision(10, 2);

        builder.Property(e => e.EmomConfigJson)
            .HasMaxLength(500)
            .HasColumnName("EmomConfigJson");

        builder.Property(e => e.TempoConfigJson)
            .HasMaxLength(500)
            .HasColumnName("TempoConfigJson");

        builder.Property(e => e.SupersetConfigJson)
            .HasMaxLength(500)
            .HasColumnName("SupersetConfigJson");

        // Relationships
        builder.HasOne(e => e.DayTemplate)
            .WithMany(d => d.Exercises)
            .HasForeignKey(e => e.DayTemplateId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.Exercise)
            .WithMany()
            .HasForeignKey(e => e.ExerciseId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(e => e.DayTemplateId)
            .HasDatabaseName("IX_ProgramExerciseTemplates_DayTemplateId");

        builder.HasIndex(e => e.ExerciseId)
            .HasDatabaseName("IX_ProgramExerciseTemplates_ExerciseId");

        builder.HasIndex(e => new { e.DayTemplateId, e.Order })
            .HasDatabaseName("IX_ProgramExerciseTemplates_DayTemplateId_Order");
    }
}
