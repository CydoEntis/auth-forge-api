using AuthForge.Api.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AuthForge.Api.Data.Configurations;

public class ApplicationEmailSettingsConfiguration : IEntityTypeConfiguration<ApplicationEmailSettings>
{
    public void Configure(EntityTypeBuilder<ApplicationEmailSettings> builder)
    {
        builder.ToTable("application_email_settings");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Provider)
            .HasMaxLength(50);

        builder.Property(e => e.FromEmail)
            .HasMaxLength(255);

        builder.Property(e => e.FromName)
            .HasMaxLength(255);

        builder.Property(e => e.SmtpHost)
            .HasMaxLength(255);

        builder.Property(e => e.SmtpUsername)
            .HasMaxLength(255);

        builder.Property(e => e.SmtpPasswordEncrypted)
            .HasMaxLength(500);

        builder.Property(e => e.ResendApiKeyEncrypted)
            .HasMaxLength(500);

        builder.HasOne(e => e.Application)
            .WithOne(a => a.EmailSettings)
            .HasForeignKey<ApplicationEmailSettings>(e => e.ApplicationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(e => e.ApplicationId)
            .IsUnique();
    }
}