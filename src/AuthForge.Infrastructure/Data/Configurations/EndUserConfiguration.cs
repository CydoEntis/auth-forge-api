using AuthForge.Domain.Entities;
using AuthForge.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ApplicationId = AuthForge.Domain.ValueObjects.ApplicationId;
using App = AuthForge.Domain.Entities.Application;

namespace AuthForge.Infrastructure.Data.Configurations;

public class EndUserConfiguration : IEntityTypeConfiguration<EndUser>
{
    public void Configure(EntityTypeBuilder<EndUser> builder)
    {
        builder.ToTable("end_users");

        builder.HasKey(u => u.Id);

        builder.Property(u => u.Id)
            .HasColumnName("id")
            .HasConversion(
                id => id.Value,
                value => EndUserId.Create(value))
            .IsRequired();

        builder.Property(u => u.ApplicationId)
            .HasColumnName("application_id")
            .HasConversion(
                id => id.Value,
                value => ApplicationId.Create(value))
            .IsRequired();

        builder.HasOne<App>()
            .WithMany()
            .HasForeignKey(u => u.ApplicationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.OwnsOne(u => u.Email, email =>
        {
            email.Property(e => e.Value)
                .HasColumnName("email")
                .HasMaxLength(255)
                .IsRequired();
    
            email.HasIndex(e => e.Value).IsUnique();
            
            email.ToTable("end_users");
        });

        builder.Property(u => u.FirstName)
            .HasColumnName("first_name")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(u => u.LastName)
            .HasColumnName("last_name")
            .HasMaxLength(100)
            .IsRequired();

        builder.OwnsOne(u => u.PasswordHash, password =>
        {
            password.Property(p => p.Hash)
                .HasColumnName("password_hash")
                .HasMaxLength(255)
                .IsRequired();

            password.Property(p => p.Salt)
                .HasColumnName("password_salt")
                .HasMaxLength(255)
                .IsRequired();
                
            password.ToTable("end_users");
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

        builder.Property(u => u.FailedLoginAttempts)
            .HasColumnName("failed_login_attempts")
            .IsRequired();

        builder.Property(u => u.LockedOutUntil)
            .HasColumnName("locked_out_until");

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