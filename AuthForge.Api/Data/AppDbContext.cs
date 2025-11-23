using AuthForge.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace AuthForge.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Application> Applications => Set<Application>();
    public DbSet<Admin> Admins => Set<Admin>();
    public DbSet<AdminRefreshToken> AdminRefreshTokens => Set<AdminRefreshToken>();
    public DbSet<AdminPasswordResetToken> AdminPasswordResetTokens => Set<AdminPasswordResetToken>();
    public DbSet<User> Users => Set<User>();
    public DbSet<UserRefreshToken> UserRefreshTokens => Set<UserRefreshToken>();
    public DbSet<UserPasswordResetToken> UserPasswordResetTokens => Set<UserPasswordResetToken>();
    public DbSet<UserOAuthIdentity> UserOAuthIdentities => Set<UserOAuthIdentity>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}