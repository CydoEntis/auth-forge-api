using AuthForge.Domain.Entities;
using AuthForge.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using App = AuthForge.Domain.Entities.Application;
namespace AuthForge.Infrastructure.Data;

public class AuthForgeDbContext : DbContext
{
    public AuthForgeDbContext(DbContextOptions<AuthForgeDbContext> options)
        : base(options)
    {
    }

    public DbSet<App> Applications => Set<App>();
    public DbSet<EndUser> EndUsers => Set<EndUser>();
    public DbSet<EndUserRefreshToken> EndUserRefreshTokens => Set<EndUserRefreshToken>();
    public DbSet<EndUserPasswordResetToken> EndUserPasswordResetTokens => Set<EndUserPasswordResetToken>();
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Ignore<Email>();
        modelBuilder.Ignore<HashedPassword>();

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AuthForgeDbContext).Assembly);
    }
}