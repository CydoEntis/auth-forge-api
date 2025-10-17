namespace AuthForge.Application.Admin.Commands;

public sealed record LoginAdminResponse(
    string AccessToken,
    string RefreshToken,
    DateTime ExpiresAt
);