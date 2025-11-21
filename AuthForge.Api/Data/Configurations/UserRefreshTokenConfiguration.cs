using AuthForge.Api.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AuthForge.Api.Data.Configurations;

public class UserRefreshTokenConfiguration : IEntityTypeConfiguration<UserRefreshToken>
{
    public void Configure(EntityTypeBuilder<UserRefreshToken> entity)
    {
        entity.ToTable("user_refresh_tokens");
        entity.HasKey(rt => rt.Id);
        entity.Property(rt => rt.Token).HasMaxLength(200).IsRequired();
        entity.HasIndex(rt => rt.Token).IsUnique();
        entity.Property(rt => rt.ExpiresAt).IsRequired();
        entity.Property(rt => rt.IsRevoked).HasDefaultValue(false);
        entity.Property(rt => rt.CreatedAtUtc).IsRequired();
        entity.HasOne(rt => rt.User)
            .WithMany(u => u.RefreshTokens)
            .HasForeignKey(rt => rt.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}