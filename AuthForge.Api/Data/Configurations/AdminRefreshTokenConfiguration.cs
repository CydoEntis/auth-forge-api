using AuthForge.Api.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AuthForge.Api.Data.Configurations;

public class AdminRefreshTokenConfiguration : IEntityTypeConfiguration<AdminRefreshToken>
{
    public void Configure(EntityTypeBuilder<AdminRefreshToken> entity)
    {
        entity.ToTable("admin_refresh_tokens");
        entity.HasKey(t => t.Id);

        entity.Property(t => t.Token).HasMaxLength(500).IsRequired();
        entity.HasIndex(t => t.Token).IsUnique();

        entity.Property(t => t.ExpiresAt).IsRequired();
        entity.Property(t => t.IsRevoked).IsRequired();
        entity.Property(t => t.CreatedAtUtc).IsRequired();

        entity.HasOne(t => t.Admin)
            .WithMany()
            .HasForeignKey(t => t.AdminId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}