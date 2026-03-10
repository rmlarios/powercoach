using CoachPlatform.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoachPlatform.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for the Application entity.
/// </summary>
public class ApplicationConfiguration : IEntityTypeConfiguration<Domain.Entities.Application>
{
    public void Configure(EntityTypeBuilder<Domain.Entities.Application> builder)
    {
        builder.ToTable("Applications");

        builder.HasKey(a => a.Id);

        // Configure PersonName value object as owned type
        builder.OwnsOne(a => a.ApplicantName, nameBuilder =>
        {
            nameBuilder.Property(n => n.FirstName)
                .HasColumnName("ApplicantFirstName")
                .HasMaxLength(100)
                .IsRequired();

            nameBuilder.Property(n => n.LastName)
                .HasColumnName("ApplicantLastName")
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
        });

        builder.Property(a => a.Phone)
            .HasMaxLength(20);

        builder.Property(a => a.Age);

        builder.Property(a => a.Gender)
            .HasMaxLength(20);

        builder.Property(a => a.Country)
            .HasMaxLength(100);

        builder.Property(a => a.TrainingExperience)
            .HasMaxLength(2000);

        builder.Property(a => a.CurrentSquat)
            .HasPrecision(10, 2);

        builder.Property(a => a.CurrentBench)
            .HasPrecision(10, 2);

        builder.Property(a => a.CurrentDeadlift)
            .HasPrecision(10, 2);

        builder.Property(a => a.Motivation)
            .HasMaxLength(2000);

        builder.Property(a => a.Goals)
            .HasMaxLength(2000);

        builder.Property(a => a.Message)
            .HasMaxLength(2000);

        builder.Property(a => a.ReferralSource)
            .HasMaxLength(500);

        builder.Property(a => a.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .HasDefaultValue(ApplicationStatus.Pending);

        builder.Property(a => a.CoachNotes)
            .HasMaxLength(2000);

        builder.Property(a => a.ReviewedAt);

        builder.Property(a => a.RejectionReason)
            .HasMaxLength(1000);

        // Relationships
        builder.HasOne(a => a.Coach)
            .WithMany(c => c.Applications)
            .HasForeignKey(a => a.CoachId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(a => a.CoachId);
        builder.HasIndex(a => a.Status);
        builder.HasIndex(a => new { a.CoachId, a.Status });
        builder.HasIndex(a => a.CreatedAt);
    }
}
