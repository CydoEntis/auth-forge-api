using AuthForge.Domain.Entities;

namespace AuthForge.Application.AuditLogs.Queries;

public record AuditLogResponse(
    Guid Id,
    Guid ApplicationId,
    string EventType,
    string? PerformedBy,
    string? TargetUserId,
    string Details,
    string? IpAddress,
    DateTime Timestamp);