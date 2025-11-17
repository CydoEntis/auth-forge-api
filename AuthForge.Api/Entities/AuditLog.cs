namespace AuthForge.Api.Entities;

public class AuditLog
{
    public Guid Id { get; set; }
    public Guid? ApplicationId { get; set; }
    public Guid? AdminId { get; set; }
    public Guid? UserId { get; set; }
    public string Action { get; set; } = null!;
    public string? Details { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}