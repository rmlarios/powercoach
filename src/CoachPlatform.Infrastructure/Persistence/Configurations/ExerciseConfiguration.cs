using CoachPlatform.Domain.Entities;
using CoachPlatform.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoachPlatform.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for the Exercise entity.
/// </summary>
public class ExerciseConfiguration : IEntityTypeConfiguration<Exercise>
{
    public void Configure(EntityTypeBuilder<Exercise> builder)
    {
        builder.ToTable("Exercises");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(e => e.Description)
            .HasMaxLength(2000);

        builder.Property(e => e.Category)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(e => e.PrimaryMuscleGroup)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(e => e.SecondaryMuscleGroups)
            .HasConversion(
                v => string.Join(',', v.Select(m => m.ToString())),
                v => string.IsNullOrEmpty(v) 
                    ? new List<MuscleGroup>() 
                    : v.Split(',', StringSplitOptions.RemoveEmptyEntries)
                        .Select(s => Enum.Parse<MuscleGroup>(s))
                        .ToList());

        builder.Property(e => e.VideoUrl)
            .HasMaxLength(500);

        builder.Property(e => e.ImageUrl)
            .HasMaxLength(500);

        builder.Property(e => e.Equipment)
            .HasMaxLength(200);

        builder.Property(e => e.Instructions)
            .HasConversion(
                v => string.Join("|||", v),
                v => string.IsNullOrEmpty(v) 
                    ? new List<string>() 
                    : v.Split("|||", StringSplitOptions.RemoveEmptyEntries).ToList());

        builder.Property(e => e.CoachingCues)
            .HasConversion(
                v => string.Join("|||", v),
                v => string.IsNullOrEmpty(v) 
                    ? new List<string>() 
                    : v.Split("|||", StringSplitOptions.RemoveEmptyEntries).ToList());

        // Relationships (optional - exercise can exist without a coach)
        builder.HasOne(e => e.Coach)
            .WithMany(c => c.Exercises)
            .HasForeignKey(e => e.CoachId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);

        // Indexes
        builder.HasIndex(e => e.CoachId)
            .HasDatabaseName("IX_Exercises_CoachId");

        builder.HasIndex(e => e.Name)
            .IsUnique()
            .HasDatabaseName("IX_Exercises_Name");

        builder.HasIndex(e => e.Category)
            .HasDatabaseName("IX_Exercises_Category");

        builder.HasIndex(e => e.IsActive)
            .HasDatabaseName("IX_Exercises_IsActive");
    }
}
