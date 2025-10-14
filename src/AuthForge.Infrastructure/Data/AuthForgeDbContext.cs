using AuthForge.Domain.Entities;
using AuthForge.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace AuthForge.Infrastructure.Data;

public class AuthForgeDbContext : DbContext
{
    public AuthForgeDbContext(DbContextOptions<AuthForgeDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Ignore<UserId>();
        modelBuilder.Ignore<TenantId>();
        modelBuilder.Ignore<Email>();
        modelBuilder.Ignore<HashedPassword>();

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AuthForgeDbContext).Assembly);
    }
}