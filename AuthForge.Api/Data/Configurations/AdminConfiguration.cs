using AuthForge.Api.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AuthForge.Api.Data.Configurations;

public class AdminConfiguration : IEntityTypeConfiguration<Admin>
{
    public void Configure(EntityTypeBuilder<Admin> entity)
    {
        entity.ToTable("admins");
        entity.HasKey(a => a.Id);
        
        entity.Property(a => a.Email).HasMaxLength(255).IsRequired();
        entity.HasIndex(a => a.Email).IsUnique();
        
        entity.Property(a => a.PasswordHash).HasMaxLength(500).IsRequired();
    }
}