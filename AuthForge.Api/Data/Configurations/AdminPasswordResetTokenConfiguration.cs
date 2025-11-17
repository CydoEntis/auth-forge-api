using AuthForge.Api.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AuthForge.Api.Data.Configurations;

public class AdminPasswordResetTokenConfiguration : IEntityTypeConfiguration<AdminPasswordResetToken>
{
    public void Configure(EntityTypeBuilder<AdminPasswordResetToken> entity)
    {
        entity.ToTable("admin_password_reset_tokens");
        
        entity.HasKey(t => t.Id);
        
        entity.Property(t => t.Token)
            .HasMaxLength(500)
            .IsRequired();
        
        entity.HasIndex(t => t.Token)
            .IsUnique();
        
        entity.HasOne(t => t.Admin)
            .WithMany()
            .HasForeignKey(t => t.AdminId)
            .OnDelete(DeleteBehavior.Cascade);
        
        entity.HasIndex(t => t.AdminId);
    }
}