using AuthForge.Api.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AuthForge.Api.Data.Configurations;

public class UserOAuthIdentityConfiguration : IEntityTypeConfiguration<UserOAuthIdentity>
{
    public void Configure(EntityTypeBuilder<UserOAuthIdentity> entity)
    {
        entity.ToTable("user_oauth_identities");

        entity.HasKey(o => o.Id);

        entity.Property(o => o.Provider)
            .HasMaxLength(50)
            .IsRequired();

        entity.Property(o => o.ProviderUserId)
            .HasMaxLength(200)
            .IsRequired();

        entity.Property(o => o.ProviderEmail)
            .HasMaxLength(255);

        entity.Property(o => o.ProviderDisplayName)
            .HasMaxLength(200);

        entity.Property(o => o.ProviderAvatarUrl)
            .HasMaxLength(500);

        entity.HasIndex(o => new { o.UserId, o.Provider })
            .IsUnique();

        entity.HasIndex(o => new { o.Provider, o.ProviderUserId });

        entity.HasOne(o => o.User)
            .WithMany(u => u.OAuthIdentities)
            .HasForeignKey(o => o.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}