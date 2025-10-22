namespace AuthForge.Domain.ValueObjects;

public sealed record AuditLogId(Guid Value)
{
    public static AuditLogId CreateUnique() => new(Guid.NewGuid());
    public static AuditLogId Create(Guid value) => new(value);
}