using CoachPlatform.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoachPlatform.Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(u => u.Id);
        
        builder.Property(u => u.Email)
            .IsRequired()
            .HasMaxLength(256);
        
        builder.Property(u => u.Username)
            .IsRequired()
            .HasMaxLength(256);
        
        builder.Property(u => u.PasswordHash)
            .IsRequired()
            .HasMaxLength(500);
        
        builder.Property(u => u.Role)
            .IsRequired()
            .HasConversion<string>();
        
        builder.Property(u => u.IsActive)
            .IsRequired()
            .HasDefaultValue(true);
        
        builder.Property(u => u.CoachId)
            .IsRequired(false);
        
        builder.Property(u => u.AthleteId)
            .IsRequired(false);
        
        builder.Property(u => u.LastLoginAt)
            .IsRequired(false);
        
        builder.Property(u => u.LastPasswordChangeAt)
            .IsRequired(false);
        
        builder.Property(u => u.RefreshTokenHash)
            .IsRequired(false)
            .HasMaxLength(500);
        
        builder.Property(u => u.RefreshTokenExpiresAt)
            .IsRequired(false);
        
        // Audit fields
        builder.Property(u => u.CreatedAt)
            .IsRequired()
            .HasColumnType("timestamp with time zone");
        
        builder.Property(u => u.UpdatedAt)
            .IsRequired()
            .HasColumnType("timestamp with time zone");
        
        // Relationships
        builder.HasOne(u => u.Coach)
            .WithMany()
            .HasForeignKey(u => u.CoachId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired(false);
        
        builder.HasOne(u => u.Athlete)
            .WithMany()
            .HasForeignKey(u => u.AthleteId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired(false);
        
        // Unique constraints
        builder.HasIndex(u => u.Email)
            .IsUnique()
            .HasDatabaseName("ix_users_email");
        
        builder.HasIndex(u => u.Username)
            .IsUnique()
            .HasDatabaseName("ix_users_username");
        
        // Check constraintfor role validity
        builder.ToTable(t => t.HasCheckConstraint(
            "ck_user_role",
            "\"Role\" IN ('Coach', 'Athlete', 'Admin')"
        ));
    }
}
