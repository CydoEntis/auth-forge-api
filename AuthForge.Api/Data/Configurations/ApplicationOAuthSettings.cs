using AuthForge.Api.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AuthForge.Api.Data.Configurations;

public class ApplicationOAuthSettingsConfiguration : IEntityTypeConfiguration<ApplicationOAuthSettings>
{
    public void Configure(EntityTypeBuilder<ApplicationOAuthSettings> builder)
    {
        builder.ToTable("application_oauth_settings");
        
        builder.HasKey(e => e.Id);
        
        builder.Property(e => e.GoogleClientId)
            .HasMaxLength(255);
        
        builder.Property(e => e.GoogleClientSecretEncrypted)
            .HasMaxLength(500);
        
        builder.Property(e => e.GithubClientId)
            .HasMaxLength(255);
        
        builder.Property(e => e.GithubClientSecretEncrypted)
            .HasMaxLength(500);
        
        builder.HasOne(e => e.Application)
            .WithOne(a => a.OAuthSettings)
            .HasForeignKey<ApplicationOAuthSettings>(e => e.ApplicationId)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasIndex(e => e.ApplicationId)
            .IsUnique();
    }
}