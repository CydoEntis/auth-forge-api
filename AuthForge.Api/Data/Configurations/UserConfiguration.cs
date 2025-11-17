using AuthForge.Api.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AuthForge.Api.Data.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> entity)
    {
        entity.ToTable("end_users");
        entity.HasKey(u => u.Id);
        
        entity.Property(u => u.Email).HasMaxLength(255).IsRequired();
        entity.HasIndex(u => new { u.ApplicationId, u.Email }).IsUnique();
        
        entity.Property(u => u.PasswordHash).HasMaxLength(500).IsRequired();
        entity.Property(u => u.EmailVerificationToken).HasMaxLength(500);
        
        entity.HasOne(u => u.Application)
            .WithMany()
            .HasForeignKey(u => u.ApplicationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}