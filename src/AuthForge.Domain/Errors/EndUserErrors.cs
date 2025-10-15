namespace AuthForge.Domain.Errors;

public static class EndUserErrors
{
    public static readonly Error NotFound = new(
        "EndUser.NotFound",
        "User not found");

    public static readonly Error InvalidCredentials = new(
        "EndUser.InvalidCredentials",
        "Invalid email or password");

    public static readonly Error DuplicateEmail = new(
        "EndUser.DuplicateEmail",
        "A user with this email already exists in this application");

    public static readonly Error Inactive = new(
        "EndUser.Inactive",
        "User account is inactive");

    public static readonly Error LockedOut = new(
        "EndUser.LockedOut",
        "Account is locked due to too many failed login attempts");

    public static Error LockedOutUntil(DateTime lockedOutUntil) => new(
        "EndUser.LockedOut",
        $"Account is locked until {lockedOutUntil:yyyy-MM-dd HH:mm:ss} UTC");

    public static readonly Error EmailNotVerified = new(
        "EndUser.EmailNotVerified",
        "Email address must be verified before logging in");

    public static readonly Error InvalidEmailVerificationToken = new(
        "EndUser.InvalidEmailVerificationToken",
        "Email verification token is invalid or expired");

    public static readonly Error EmailAlreadyVerified = new(
        "EndUser.EmailAlreadyVerified",
        "Email address is already verified");
}