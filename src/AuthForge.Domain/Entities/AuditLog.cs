using AuthForge.Domain.Common;
using AuthForge.Domain.ValueObjects;
using ApplicationId = AuthForge.Domain.ValueObjects.ApplicationId;

namespace AuthForge.Domain.Entities;

public sealed class AuditLog : Entity<AuditLogId>
{
    private AuditLog() { }

    private AuditLog(
        AuditLogId id,
        ApplicationId applicationId,
        string eventType,
        string? performedBy,
        string? targetUserId,
        string details,
        string? ipAddress) : base(id)
    {
        ApplicationId = applicationId;
        EventType = eventType;
        PerformedBy = performedBy;
        TargetUserId = targetUserId;
        Details = details;
        IpAddress = ipAddress;
        Timestamp = DateTime.UtcNow;
    }

    public ApplicationId ApplicationId { get; private set; } = default!;
    public string EventType { get; private set; } = string.Empty;
    public string? PerformedBy { get; private set; }  
    public string? TargetUserId { get; private set; } 
    public string Details { get; private set; } = string.Empty;  
    public string? IpAddress { get; private set; }
    public DateTime Timestamp { get; private set; }

    public static AuditLog Create(
        ApplicationId applicationId,
        string eventType,
        string? performedBy,
        string? targetUserId,
        string details,
        string? ipAddress)
    {
        if (string.IsNullOrWhiteSpace(eventType))
            throw new ArgumentException("Event type cannot be empty", nameof(eventType));

        if (string.IsNullOrWhiteSpace(details))
            throw new ArgumentException("Details cannot be empty", nameof(details));

        return new AuditLog(
            AuditLogId.CreateUnique(),
            applicationId,
            eventType,
            performedBy,
            targetUserId,
            details,
            ipAddress);
    }
}