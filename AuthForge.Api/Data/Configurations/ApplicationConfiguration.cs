using System.Text.Json;
using AuthForge.Api.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AuthForge.Api.Data.Configurations;

public class ApplicationConfiguration : IEntityTypeConfiguration<Application>
{
    public void Configure(EntityTypeBuilder<Application> entity)
    {
        entity.ToTable("applications");
        entity.HasKey(a => a.Id);

        
        entity.Property(a => a.Name).HasMaxLength(200).IsRequired();
        entity.Property(a => a.Slug).HasMaxLength(200).IsRequired();
        entity.HasIndex(a => a.Slug).IsUnique();

        entity.Property(a => a.PublicKey).HasMaxLength(100).IsRequired();
        entity.HasIndex(a => a.PublicKey).IsUnique();

        entity.Property(a => a.SecretKey).HasMaxLength(500).IsRequired();
        entity.Property(a => a.JwtSecret).HasMaxLength(500).IsRequired();

        entity.Property(a => a.Description).HasMaxLength(1000);

        entity.Property(a => a.AllowedOrigins)
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null) ?? new List<string>())
            .HasColumnType("text");
    }
}