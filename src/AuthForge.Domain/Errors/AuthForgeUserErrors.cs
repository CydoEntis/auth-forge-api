namespace AuthForge.Domain.Errors;

public static class AuthForgeUserErrors
{
    public static readonly Error NotFound = new(
        "AuthForgeUser.NotFound",
        "Developer account not found");

    public static readonly Error InvalidCredentials = new(
        "AuthForgeUser.InvalidCredentials",
        "Invalid email or password");

    public static readonly Error DuplicateEmail = new(
        "AuthForgeUser.DuplicateEmail",
        "A developer account with this email already exists");

    public static readonly Error Inactive = new(
        "AuthForgeUser.Inactive",
        "Developer account is inactive");

    public static readonly Error EmailNotVerified = new(
        "AuthForgeUser.EmailNotVerified",
        "Email address must be verified before logging in");

    public static readonly Error InvalidEmailVerificationToken = new(
        "AuthForgeUser.InvalidEmailVerificationToken",
        "Email verification token is invalid or expired");

    public static readonly Error EmailAlreadyVerified = new(
        "AuthForgeUser.EmailAlreadyVerified",
        "Email address is already verified");
}