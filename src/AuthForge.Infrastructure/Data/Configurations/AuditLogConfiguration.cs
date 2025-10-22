using AuthForge.Domain.Entities;
using AuthForge.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ApplicationId = AuthForge.Domain.ValueObjects.ApplicationId;

namespace AuthForge.Infrastructure.Data.Configurations;

public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("audit_logs");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Id)
            .HasColumnName("id")
            .HasConversion(
                id => id.Value,
                value => AuditLogId.Create(value));

        builder.Property(a => a.ApplicationId)
            .HasColumnName("application_id")
            .HasConversion(
                id => id.Value,
                value => ApplicationId.Create(value))
            .IsRequired();

        builder.Property(a => a.EventType)
            .HasColumnName("event_type")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(a => a.PerformedBy)
            .HasColumnName("performed_by")
            .HasMaxLength(255);

        builder.Property(a => a.TargetUserId)
            .HasColumnName("target_user_id")
            .HasMaxLength(255);

        builder.Property(a => a.Details)
            .HasColumnName("details")
            .HasColumnType("TEXT")
            .IsRequired();

        builder.Property(a => a.IpAddress)
            .HasColumnName("ip_address")
            .HasMaxLength(45); 

        builder.Property(a => a.Timestamp)
            .HasColumnName("timestamp")
            .IsRequired();

        builder.HasIndex(a => a.ApplicationId);
        builder.HasIndex(a => a.EventType);
        builder.HasIndex(a => a.TargetUserId);
        builder.HasIndex(a => a.Timestamp);
    }
}