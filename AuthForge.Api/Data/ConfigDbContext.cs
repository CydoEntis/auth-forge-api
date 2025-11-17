using AuthForge.Api.Entities;

namespace AuthForge.Api.Data;

using Microsoft.EntityFrameworkCore;

// Separate database for storing setup configuration.
// This is always SQLite and exists before the main database is configured.
// This is required for the application to function correctly -- Do not remove.
public class ConfigDbContext : DbContext
{
    public ConfigDbContext(DbContextOptions<ConfigDbContext> options) : base(options)
    {
    }

    public DbSet<Configuration> Configuration => Set<Configuration>();


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Configuration>(entity =>
        {
            entity.ToTable("configurations");
            entity.HasKey(e => e.Id);

            entity.Property(c => c.AuthForgeDomain).HasMaxLength(500);

            entity.HasKey(c => c.Id);

            entity.Property(c => c.DatabaseProvider).HasMaxLength(50);
            entity.Property(c => c.DatabaseConnectionString).HasMaxLength(1000);

            entity.Property(c => c.JwtSecretEncrypted).HasMaxLength(500);

            entity.Property(c => c.EmailProvider).HasMaxLength(50);
            entity.Property(c => c.EmailFromAddress).HasMaxLength(255);
            entity.Property(c => c.EmailFromName).HasMaxLength(200);
            entity.Property(c => c.SmtpHost).HasMaxLength(200);
            entity.Property(c => c.SmtpUsername).HasMaxLength(200);
            entity.Property(c => c.SmtpPasswordEncrypted).HasMaxLength(500);
            entity.Property(c => c.ResendApiKeyEncrypted).HasMaxLength(500);
        });
    }
}