namespace AuthForge.Domain.Errors;

public static class EndUserRefreshTokenErrors
{
    public static readonly Error NotFound = new(
        "EndUserRefreshToken.NotFound",
        "Refresh token not found");

    public static readonly Error Expired = new(
        "EndUserRefreshToken.Expired",
        "Refresh token has expired");

    public static readonly Error Revoked = new(
        "EndUserRefreshToken.Revoked",
        "Refresh token has been revoked");

    public static readonly Error Invalid = new(
        "EndUserRefreshToken.Invalid",
        "Refresh token is invalid");
}