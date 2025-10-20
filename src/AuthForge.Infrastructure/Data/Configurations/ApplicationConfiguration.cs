using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.Text.Json;
using App = AuthForge.Domain.Entities.Application;
using ApplicationId = AuthForge.Domain.ValueObjects.ApplicationId;

namespace AuthForge.Infrastructure.Data.Configurations;

public class ApplicationConfiguration : IEntityTypeConfiguration<App>
{
    public void Configure(EntityTypeBuilder<App> builder)
    {
        builder.ToTable("applications");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Id)
            .HasColumnName("id")
            .HasConversion(
                id => id.Value,
                value => ApplicationId.Create(value))
            .IsRequired();

        builder.Property(a => a.Name)
            .HasColumnName("name")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(a => a.Slug)
            .HasColumnName("slug")
            .HasMaxLength(200)
            .IsRequired();

        builder.HasIndex(a => a.Slug)
            .IsUnique();

        builder.Property(a => a.PublicKey)
            .HasColumnName("public_key")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(a => a.SecretKey)
            .HasColumnName("secret_key")
            .HasMaxLength(100)
            .IsRequired();

        builder.HasIndex(a => a.PublicKey)
            .IsUnique();

        builder.HasIndex(a => a.SecretKey)
            .IsUnique();

        var allowedOriginsConverter = new ValueConverter<List<string>, string>(
            v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
            v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null) ?? new List<string>());

        builder.Property("_allowedOrigins")
            .HasColumnName("allowed_origins")
            .HasColumnType("TEXT")
            .HasConversion(allowedOriginsConverter)
            .IsRequired();

        builder.Property(a => a.IsActive)
            .HasColumnName("is_active")
            .IsRequired();

        builder.OwnsOne(a => a.Settings, settings =>
        {
            settings.Property(s => s.MaxFailedLoginAttempts)
                .HasColumnName("max_failed_login_attempts")
                .IsRequired();

            settings.Property(s => s.LockoutDurationMinutes)
                .HasColumnName("lockout_duration_minutes")
                .IsRequired();

            settings.Property(s => s.AccessTokenExpirationMinutes)
                .HasColumnName("access_token_expiration_minutes")
                .IsRequired();

            settings.Property(s => s.RefreshTokenExpirationDays)
                .HasColumnName("refresh_token_expiration_days")
                .IsRequired();
        });

        builder.Property(a => a.CreatedAtUtc)
            .HasColumnName("created_at_utc")
            .IsRequired();

        builder.Property(a => a.UpdatedAtUtc)
            .HasColumnName("updated_at_utc");

        builder.Property(a => a.DeactivatedAtUtc)
            .HasColumnName("deactivated_at_utc");

        builder.OwnsOne(a => a.ApplicationEmailSettings, email =>
        {
            email.Property(e => e.Provider)
                .HasColumnName("email_provider")
                .HasConversion<int>();

            email.Property(e => e.ApiKey)
                .HasColumnName("email_api_key")
                .HasMaxLength(500)
                .IsRequired(false);

            email.Property(e => e.FromEmail)
                .HasColumnName("email_from_email")
                .HasMaxLength(255)
                .IsRequired(false);

            email.Property(e => e.FromName)
                .HasColumnName("email_from_name")
                .HasMaxLength(255)
                .IsRequired(false);

            email.Property(e => e.PasswordResetCallbackUrl)
                .HasColumnName("email_password_reset_callback_url")
                .HasMaxLength(500)
                .IsRequired(false);

            email.Property(e => e.EmailVerificationCallbackUrl)
                .HasColumnName("email_verification_callback_url")
                .HasMaxLength(500)
                .IsRequired(false);

            email.ToTable("applications");
        });

        builder.Ignore(a => a.DomainEvents);
    }
}