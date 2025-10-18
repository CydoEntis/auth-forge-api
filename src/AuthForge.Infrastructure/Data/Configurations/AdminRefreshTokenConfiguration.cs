using AuthForge.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AuthForge.Infrastructure.Data.Configurations;

public class AdminRefreshTokenConfiguration : IEntityTypeConfiguration<AdminRefreshToken>
{
    public void Configure(EntityTypeBuilder<AdminRefreshToken> builder)
    {
        builder.ToTable("admin_refresh_tokens");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Token)
            .IsRequired()
            .HasMaxLength(500);

        builder.HasIndex(t => t.Token)
            .IsUnique();

        builder.Property(t => t.ExpiresAtUtc)
            .IsRequired();

        builder.Property(t => t.CreatedAtUtc)
            .IsRequired();

        builder.Property(t => t.RevokedAtUtc);

        builder.Property(t => t.UsedAtUtc);

        builder.Property(t => t.ReplacedByToken)
            .HasMaxLength(500);

        builder.Property(t => t.IpAddress)
            .HasMaxLength(45);

        builder.Property(t => t.UserAgent)
            .HasMaxLength(500);
    }
}