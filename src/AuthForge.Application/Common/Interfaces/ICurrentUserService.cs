namespace AuthForge.Application.Common.Interfaces;

public interface ICurrentUserService
{
    string? Email { get; }
    string? UserId { get; }
    string? IpAddress { get; }
    string? UserAgent { get; }
    bool IsAuthenticated { get; }
}