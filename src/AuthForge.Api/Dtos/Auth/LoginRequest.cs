namespace AuthForge.Api.Dtos.Auth;

public sealed record LoginRequest(
    string TenantId,
    string Email,
    string Password);