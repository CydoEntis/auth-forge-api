using System.Text.Json;
using AuthForge.Api.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AuthForge.Api.Data.Configurations;

public class ApplicationConfiguration : IEntityTypeConfiguration<Application>
{
    public void Configure(EntityTypeBuilder<Application> entity)
    {
        entity.ToTable("applications");
        entity.HasKey(a => a.Id);

        entity.Property(a => a.Name).HasMaxLength(200).IsRequired();
        entity.Property(a => a.Slug).HasMaxLength(200).IsRequired();
        entity.HasIndex(a => a.Slug).IsUnique();
        entity.Property(a => a.Description).HasMaxLength(1000);

        entity.Property(a => a.ClientId).HasMaxLength(100).IsRequired();
        entity.HasIndex(a => a.ClientId).IsUnique();
        entity.Property(a => a.ClientSecretEncrypted).HasMaxLength(500).IsRequired();
        entity.Property(a => a.JwtSecretEncrypted).HasMaxLength(500).IsRequired();

        entity.Property(a => a.AllowedOrigins)
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null) ?? new List<string>())
            .HasColumnType("text");

        entity.Property(a => a.IsActive).HasDefaultValue(true);
        entity.Property(a => a.MaxFailedLoginAttempts).HasDefaultValue(5);
        entity.Property(a => a.LockoutDurationMinutes).HasDefaultValue(15);
        entity.Property(a => a.AccessTokenExpirationMinutes).HasDefaultValue(15);
        entity.Property(a => a.RefreshTokenExpirationDays).HasDefaultValue(7);

        entity.Property(a => a.PasswordResetCallbackUrl).HasMaxLength(500);
        entity.Property(a => a.EmailVerificationCallbackUrl).HasMaxLength(500);
        entity.Property(a => a.MagicLinkCallbackUrl).HasMaxLength(500);
        entity.Property(a => a.RequireEmailVerification).HasDefaultValue(false).IsRequired();

        entity.Property(a => a.IsDeleted).HasDefaultValue(false);
        entity.Property(a => a.DeletedAtUtc);
        entity.HasQueryFilter(a => !a.IsDeleted);

        entity.Property(a => a.CreatedAtUtc).IsRequired();
        entity.Property(a => a.UpdatedAtUtc);

        entity.HasOne(a => a.EmailSettings)
            .WithOne(e => e.Application)
            .HasForeignKey<ApplicationEmailSettings>(e => e.ApplicationId)
            .OnDelete(DeleteBehavior.Cascade);

        entity.HasOne(a => a.OAuthSettings)
            .WithOne(o => o.Application)
            .HasForeignKey<ApplicationOAuthSettings>(o => o.ApplicationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}