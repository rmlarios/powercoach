using CoachPlatform.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoachPlatform.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for the Plan entity.
/// </summary>
public class PlanConfiguration : IEntityTypeConfiguration<Plan>
{
    public void Configure(EntityTypeBuilder<Plan> builder)
    {
        builder.ToTable("Plans");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Name)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(p => p.Description)
            .HasMaxLength(2000);

        // Configure Money value object for Price
        builder.OwnsOne(p => p.Price, priceBuilder =>
        {
            priceBuilder.Property(m => m.Amount)
                .HasColumnName("PriceAmount")
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            priceBuilder.Property(m => m.Currency)
                .HasColumnName("PriceCurrency")
                .HasMaxLength(3)
                .IsRequired();
        });

        builder.Property(p => p.PlanType)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(p => p.DurationDays)
            .IsRequired();

        builder.Property(p => p.MaxAthletes)
            .IsRequired(false);

        // Store Features as JSON
        builder.Property(p => p.Features)
            .HasConversion(
                v => string.Join("||", v),
                v => v.Split("||", StringSplitOptions.RemoveEmptyEntries).ToList());

        builder.Property(p => p.IsActive)
            .HasDefaultValue(true);

        builder.Property(p => p.DisplayOrder)
            .HasDefaultValue(0);

        // Relationships
        builder.HasOne(p => p.Coach)
            .WithMany(c => c.Plans)
            .HasForeignKey(p => p.CoachId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(p => p.Subscriptions)
            .WithOne(s => s.Plan)
            .HasForeignKey(s => s.PlanId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(p => p.CoachId);
        builder.HasIndex(p => p.IsActive);
    }
}
