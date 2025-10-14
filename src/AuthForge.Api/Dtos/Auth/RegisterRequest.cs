namespace AuthForge.Api.Dtos.Auth;

public sealed record RegisterRequest(
    string TenantId,
    string Email,
    string Password,
    string FirstName,
    string LastName);