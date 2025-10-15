using AuthForge.Domain.Entities;
using AuthForge.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AuthForge.Infrastructure.Data.Configurations;

public class EndUserPasswordResetTokenConfiguration : IEntityTypeConfiguration<EndUserPasswordResetToken>
{
    public void Configure(EntityTypeBuilder<EndUserPasswordResetToken> builder)
    {
        builder.ToTable("end_user_password_reset_tokens");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Id)
            .HasColumnName("id")
            .IsRequired();

        builder.Property(t => t.UserId)
            .HasColumnName("user_id")
            .HasConversion(
                id => id.Value,
                value => EndUserId.Create(value))
            .IsRequired();

        builder.HasOne<EndUser>()
            .WithMany()
            .HasForeignKey(t => t.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(t => t.Token)
            .HasColumnName("token")
            .HasMaxLength(500)
            .IsRequired();

        builder.HasIndex(t => t.Token)
            .IsUnique();

        builder.Property(t => t.ExpiresAtUtc)
            .HasColumnName("expires_at_utc")
            .IsRequired();

        builder.Property(t => t.CreatedAtUtc)
            .HasColumnName("created_at_utc")
            .IsRequired();

        builder.Property(t => t.IsUsed)
            .HasColumnName("is_used")
            .IsRequired();

        builder.Property(t => t.UsedAtUtc)
            .HasColumnName("used_at_utc");

        builder.Ignore(t => t.IsExpired);
        builder.Ignore(t => t.IsValid);
    }
}