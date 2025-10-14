using AuthForge.Domain.Entities;
using AuthForge.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AuthForge.Infrastructure.Data.Configurations;

public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("refresh_tokens");

        builder.HasKey(rt => rt.Id);

        builder.Property(rt => rt.Id)
            .HasColumnName("id")
            .IsRequired();

        builder.Property(rt => rt.UserId)
            .HasColumnName("user_id")
            .HasConversion(
                id => id.Value,
                value => UserId.Create(value))
            .IsRequired();

        builder.Property(rt => rt.Token)
            .HasColumnName("token")
            .HasMaxLength(500)
            .IsRequired();

        builder.HasIndex(rt => rt.Token)
            .IsUnique();

        builder.Property(rt => rt.ExpiresAtUtc)
            .HasColumnName("expires_at_utc")
            .IsRequired();

        builder.Property(rt => rt.CreatedAtUtc)
            .HasColumnName("created_at_utc")
            .IsRequired();

        builder.Property(rt => rt.RevokedAtUtc)
            .HasColumnName("revoked_at_utc");

        builder.Property(rt => rt.UsedAtUtc)
            .HasColumnName("used_at_utc");

        builder.Property(rt => rt.ReplacedByToken)
            .HasColumnName("replaced_by_token")
            .HasMaxLength(500);

        builder.Property(rt => rt.IpAddress)
            .HasColumnName("ip_address")
            .HasMaxLength(45);

        builder.Property(rt => rt.UserAgent)
            .HasColumnName("user_agent")
            .HasMaxLength(500);

        builder.Ignore(rt => rt.IsExpired);
        builder.Ignore(rt => rt.IsRevoked);
        builder.Ignore(rt => rt.IsActive);
        
        builder.Ignore(rt => rt.DomainEvents);

        builder.HasOne<User>()              
            .WithMany()                      
            .HasForeignKey(rt => rt.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}