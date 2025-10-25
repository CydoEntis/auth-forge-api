using AuthForge.Domain.Entities;
using AuthForge.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AuthForge.Infrastructure.Data.Configurations;

public class AdminRefreshTokenConfiguration : IEntityTypeConfiguration<AdminRefreshToken>
{
    public void Configure(EntityTypeBuilder<AdminRefreshToken> builder)
    {
        builder.ToTable("admin_refresh_tokens");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        // ✅ Add AdminId
        builder.Property(t => t.AdminId)
            .HasColumnName("admin_id")
            .HasConversion(
                id => id.Value,
                value => AdminId.Create(value))
            .IsRequired();

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

        builder.Property(t => t.RevokedAtUtc)
            .HasColumnName("revoked_at_utc");

        builder.Property(t => t.UsedAtUtc)
            .HasColumnName("used_at_utc");

        builder.Property(t => t.ReplacedByToken)
            .HasColumnName("replaced_by_token")
            .HasMaxLength(500);

        builder.Property(t => t.IpAddress)
            .HasColumnName("ip_address")
            .HasMaxLength(45);

        builder.Property(t => t.UserAgent)
            .HasColumnName("user_agent")
            .HasMaxLength(500);

        builder.Ignore(t => t.IsExpired);
        builder.Ignore(t => t.IsRevoked);
        builder.Ignore(t => t.IsActive);
    }
}