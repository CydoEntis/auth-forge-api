namespace AuthForge.Domain.Errors;

public static class AdminErrors
{
    public static readonly Error InvalidCredentials = new(
        "Admin.InvalidCredentials",
        "Invalid credentials");

    public static readonly Error InvalidRefreshToken = new(
        "Admin.InvalidRefreshToken",
        "Refresh token is invalid or has been revoked");
}