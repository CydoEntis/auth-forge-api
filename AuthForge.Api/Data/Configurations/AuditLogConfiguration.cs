using AuthForge.Api.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AuthForge.Api.Data.Configurations;

public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> entity)
    {
        entity.ToTable("audit_logs");
        entity.HasKey(a => a.Id);
        
        entity.Property(a => a.Action).HasMaxLength(100).IsRequired();
        entity.Property(a => a.Details).HasColumnType("text");
        entity.Property(a => a.IpAddress).HasMaxLength(50);
        entity.Property(a => a.UserAgent).HasMaxLength(500);
        entity.Property(a => a.CreatedAtUtc).IsRequired();
        
        entity.HasIndex(a => a.ApplicationId);
        entity.HasIndex(a => a.AdminId);
        entity.HasIndex(a => a.UserId);
        entity.HasIndex(a => a.CreatedAtUtc);
        
        entity.HasIndex(a => new { a.ApplicationId, a.CreatedAtUtc });
    }
}