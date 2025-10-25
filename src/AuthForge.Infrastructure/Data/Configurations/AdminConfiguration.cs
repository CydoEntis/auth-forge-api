using AuthForge.Domain.Entities;
using AuthForge.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AuthForge.Infrastructure.Data.Configurations;

public class AdminConfiguration : IEntityTypeConfiguration<Admin>
{
    public void Configure(EntityTypeBuilder<Admin> builder)
    {
        builder.ToTable("admins");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Id)
            .HasColumnName("id")
            .HasConversion(
                id => id.Value,
                value => AdminId.Create(value))
            .IsRequired();

        builder.OwnsOne(a => a.Email, email =>
        {
            email.Property(e => e.Value)
                .HasColumnName("email")
                .HasMaxLength(255)
                .IsRequired();

            email.HasIndex(e => e.Value).IsUnique();
            
            email.ToTable("admins");
        });

        builder.OwnsOne(a => a.PasswordHash, password =>
        {
            password.Property(p => p.Hash)
                .HasColumnName("password_hash")
                .HasMaxLength(255)
                .IsRequired();

            password.Property(p => p.Salt)
                .HasColumnName("password_salt")
                .HasMaxLength(255)
                .IsRequired();
                
            password.ToTable("admins");
        });

        builder.Property(a => a.IsEmailVerified)
            .HasColumnName("is_email_verified")
            .IsRequired();

        builder.Property(a => a.FailedLoginAttempts)
            .HasColumnName("failed_login_attempts")
            .IsRequired();

        builder.Property(a => a.LockedOutUntil)
            .HasColumnName("locked_out_until");

        builder.Property(a => a.CreatedAtUtc)
            .HasColumnName("created_at_utc")
            .IsRequired();

        builder.Property(a => a.UpdatedAtUtc)
            .HasColumnName("updated_at_utc");

        builder.Property(a => a.LastLoginAtUtc)
            .HasColumnName("last_login_at_utc");

        builder.Property(a => a.PasswordResetToken)
            .HasColumnName("password_reset_token")
            .HasMaxLength(500);

        builder.Property(a => a.PasswordResetTokenExpiresAt)
            .HasColumnName("password_reset_token_expires_at");

        builder.Ignore(a => a.DomainEvents);
    }
}