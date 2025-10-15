namespace AuthForge.Domain.Errors;

public static class PasswordResetErrors
{
    public static readonly Error TokenNotFound = new(
        "PasswordReset.TokenNotFound",
        "Password reset token not found");

    public static readonly Error TokenExpired = new(
        "PasswordReset.TokenExpired",
        "Password reset token has expired");

    public static readonly Error TokenAlreadyUsed = new(
        "PasswordReset.TokenAlreadyUsed",
        "Password reset token has already been used");

    public static readonly Error InvalidToken = new(
        "PasswordReset.InvalidToken",
        "Password reset token is invalid");
}