namespace AuthForge.Application.Admin.Commands.Login;

public sealed record LoginAdminResponse(
    string AccessToken,
    string RefreshToken,
    DateTime ExpiresAt
);