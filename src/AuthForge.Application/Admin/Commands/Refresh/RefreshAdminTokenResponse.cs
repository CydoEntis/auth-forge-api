namespace AuthForge.Application.Admin.Commands.Refresh;

public sealed record RefreshAdminTokenResponse(
    string AccessToken,
    string RefreshToken,
    DateTime ExpiresAt
);