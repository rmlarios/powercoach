using CoachPlatform.Domain.Entities;
using CoachPlatform.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoachPlatform.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for the Athlete entity.
/// </summary>
public class AthleteConfiguration : IEntityTypeConfiguration<Athlete>
{
    public void Configure(EntityTypeBuilder<Athlete> builder)
    {
        builder.ToTable("Athletes");

        builder.HasKey(a => a.Id);

        // Configure PersonName value object as owned type
        builder.OwnsOne(a => a.Name, nameBuilder =>
        {
            nameBuilder.Property(n => n.FirstName)
                .HasColumnName("FirstName")
                .HasMaxLength(100)
                .IsRequired();

            nameBuilder.Property(n => n.LastName)
                .HasColumnName("LastName")
                .HasMaxLength(100)
                .IsRequired();
        });

        // Configure Email value object
        builder.OwnsOne(a => a.Email, emailBuilder =>
        {
            emailBuilder.Property(e => e.Value)
                .HasColumnName("Email")
                .HasMaxLength(256)
                .IsRequired();

            // Index for faster lookups, unique per coach
            emailBuilder.HasIndex(e => e.Value);
        });

        builder.Property(a => a.Phone)
            .HasMaxLength(20);

        builder.Property(a => a.Goals)
            .HasMaxLength(2000);

        builder.Property(a => a.Notes)
            .HasMaxLength(2000);

        builder.Property(a => a.Country)
            .HasMaxLength(100);

        builder.Property(a => a.Gender)
            .HasMaxLength(20);

        builder.Property(a => a.DateOfBirth);

        builder.Property(a => a.Height)
            .HasPrecision(10, 2);

        builder.Property(a => a.Weight)
            .HasPrecision(10, 2);

        builder.Property(a => a.ExperienceLevel)
            .HasMaxLength(50);

        builder.Property(a => a.ProfilePictureUrl)
            .HasMaxLength(500);

        builder.Property(a => a.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .HasDefaultValue(AthleteStatus.Active);

        builder.Property(a => a.StartDate)
            .IsRequired();

        // Configure audit fields
        builder.Property(a => a.CreatedAt)
            .IsRequired();

        builder.Property(a => a.UpdatedAt)
            .IsRequired();

        // Relationships
        builder.HasOne(a => a.Coach)
            .WithMany(c => c.Athletes)
            .HasForeignKey(a => a.CoachId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(a => a.Application)
            .WithOne()
            .HasForeignKey<Athlete>(a => a.ApplicationId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(a => a.Subscriptions)
            .WithOne(s => s.Athlete)
            .HasForeignKey(s => s.AthleteId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(a => a.CheckIns)
            .WithOne(c => c.Athlete)
            .HasForeignKey(c => c.AthleteId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(a => a.CoachId);
        builder.HasIndex(a => a.Status);
        builder.HasIndex(a => new { a.CoachId, a.Status });
    }
}
