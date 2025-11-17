using AuthForge.Api.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AuthForge.Api.Data.Configurations;

public class UserPasswordResetTokenConfiguration : IEntityTypeConfiguration<UserPasswordResetToken>
{
    public void Configure(EntityTypeBuilder<UserPasswordResetToken> entity)
    {
        entity.ToTable("end_user_password_reset_tokens");
        entity.HasKey(t => t.Id);

        entity.Property(t => t.Token).HasMaxLength(500).IsRequired();
        entity.HasIndex(t => t.Token).IsUnique();

        entity.Property(t => t.ExpiresAt).IsRequired();
        entity.Property(t => t.IsUsed).IsRequired();
        entity.Property(t => t.CreatedAtUtc).IsRequired();

        entity.HasOne(t => t.User)
            .WithMany()
            .HasForeignKey(t => t.EndUserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}