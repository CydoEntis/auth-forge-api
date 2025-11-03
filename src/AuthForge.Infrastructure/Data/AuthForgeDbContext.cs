using AuthForge.Application.Common.Interfaces;
using AuthForge.Domain.Entities;
using AuthForge.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using App = AuthForge.Domain.Entities.Application;

namespace AuthForge.Infrastructure.Data;

public class AuthForgeDbContext : DbContext
{
    private readonly IEncryptionService _encryptionService;

    public AuthForgeDbContext(
        DbContextOptions<AuthForgeDbContext> options,
        IEncryptionService encryptionService) 
        : base(options)
    {
        _encryptionService = encryptionService;
    }

    public DbSet<App> Applications => Set<App>();
    public DbSet<Admin> Admins { get; set; } = null!; 
    public DbSet<AdminRefreshToken> AdminRefreshTokens => Set<AdminRefreshToken>();
    public DbSet<EndUser> EndUsers => Set<EndUser>();
    public DbSet<EndUserRefreshToken> EndUserRefreshTokens => Set<EndUserRefreshToken>();
    public DbSet<EndUserPasswordResetToken> EndUserPasswordResetTokens => Set<EndUserPasswordResetToken>();
    public DbSet<AuditLog> AuditLogs { get; set; } = null!;
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Ignore<Email>();
        modelBuilder.Ignore<HashedPassword>();

        modelBuilder.ApplyConfiguration(new Configurations.ApplicationConfiguration(_encryptionService));

        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(AuthForgeDbContext).Assembly,
            t => t != typeof(Configurations.ApplicationConfiguration));
    }
}