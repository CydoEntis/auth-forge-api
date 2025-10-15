namespace AuthForge.Domain.Errors;

public static class AuthForgeRefreshTokenErrors
{
    public static readonly Error NotFound = new(
        "AuthForgeRefreshToken.NotFound",
        "Refresh token not found");

    public static readonly Error Expired = new(
        "AuthForgeRefreshToken.Expired",
        "Refresh token has expired");

    public static readonly Error Revoked = new(
        "AuthForgeRefreshToken.Revoked",
        "Refresh token has been revoked");

    public static readonly Error Invalid = new(
        "AuthForgeRefreshToken.Invalid",
        "Refresh token is invalid");
}