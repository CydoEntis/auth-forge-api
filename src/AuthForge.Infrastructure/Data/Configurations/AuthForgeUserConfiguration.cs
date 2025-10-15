using AuthForge.Domain.Entities;
using AuthForge.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AuthForge.Infrastructure.Data.Configurations;

public class AuthForgeUserConfiguration : IEntityTypeConfiguration<AuthForgeUser>
{
    public void Configure(EntityTypeBuilder<AuthForgeUser> builder)
    {
        builder.ToTable("authforge_users");

        builder.HasKey(u => u.Id);

        builder.Property(u => u.Id)
            .HasColumnName("id")
            .HasConversion(
                id => id.Value,
                value => AuthForgeUserId.Create(value))
            .IsRequired();

        builder.Property(u => u.Email)
            .HasColumnName("email")
            .HasMaxLength(255)
            .HasConversion(
                email => email.Value,
                value => Email.Create(value))
            .IsRequired();

        builder.HasIndex(u => u.Email)
            .IsUnique();

        builder.Property(u => u.FirstName)
            .HasColumnName("first_name")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(u => u.LastName)
            .HasColumnName("last_name")
            .HasMaxLength(100)
            .IsRequired();

        builder.OwnsOne(u => u.HashedPassword, password =>
        {
            password.Property(p => p.Hash)
                .HasColumnName("password_hash")
                .HasMaxLength(255)
                .IsRequired();

            password.Property(p => p.Salt)
                .HasColumnName("password_salt")
                .HasMaxLength(255)
                .IsRequired();
        });

        builder.Property(u => u.IsEmailVerified)
            .HasColumnName("is_email_verified")
            .IsRequired();

        builder.Property(u => u.EmailVerificationToken)
            .HasColumnName("email_verification_token")
            .HasMaxLength(255);

        builder.Property(u => u.EmailVerificationTokenExpiresAt)
            .HasColumnName("email_verification_token_expires_at");

        builder.Property(u => u.IsActive)
            .HasColumnName("is_active")
            .IsRequired();

        builder.Property(u => u.CreatedAtUtc)
            .HasColumnName("created_at_utc")
            .IsRequired();

        builder.Property(u => u.UpdatedAtUtc)
            .HasColumnName("updated_at_utc");

        builder.Property(u => u.LastLoginAtUtc)
            .HasColumnName("last_login_at_utc");

        builder.Ignore(u => u.DomainEvents);
        builder.Ignore(u => u.FullName);
    }
}