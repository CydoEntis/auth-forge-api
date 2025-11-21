using AuthForge.Api.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AuthForge.Api.Data.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> entity)
    {
        entity.ToTable("users");
        entity.HasKey(u => u.Id);
        entity.HasIndex(u => new { u.ApplicationId, u.Email }).IsUnique();
        entity.Property(u => u.Email).HasMaxLength(255).IsRequired();
        entity.Property(u => u.PasswordHash).HasMaxLength(500).IsRequired();
        entity.Property(u => u.EmailVerificationToken).HasMaxLength(200);
        entity.Property(u => u.FirstName).HasMaxLength(100);
        entity.Property(u => u.LastName).HasMaxLength(100);
        entity.Property(u => u.ProfilePictureUrl).HasMaxLength(500);
        entity.Property(u => u.EmailVerified).HasDefaultValue(false);
        entity.Property(u => u.FailedLoginAttempts).HasDefaultValue(0);
        entity.HasOne(u => u.Application)
            .WithMany(a => a.Users)
            .HasForeignKey(u => u.ApplicationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}