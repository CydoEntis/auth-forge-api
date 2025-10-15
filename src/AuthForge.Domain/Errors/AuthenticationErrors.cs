namespace AuthForge.Domain.Errors;

public static class AuthenticationErrors
{
    public static readonly Error InvalidToken = new(
        "Authentication.InvalidToken",
        "The provided token is invalid");

    public static readonly Error TokenExpired = new(
        "Authentication.TokenExpired",
        "The token has expired");

    public static readonly Error Unauthorized = new(
        "Authentication.Unauthorized",
        "You are not authorized to perform this action");
}