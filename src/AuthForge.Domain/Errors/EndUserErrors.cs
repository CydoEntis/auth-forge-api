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

    public static Error VerificationTokenExpired =>
        new(
            "EndUser.VerificationTokenExpired",
            "Email verification token has expired. Please request a new verification email.");


    public static readonly Error InvalidApiKey = new(
        "Auth.InvalidApiKey",
        "Valid API key is required");

    public static readonly Error InvalidResetToken = new(
        "EndUser.InvalidResetToken",
        "Invalid or expired password reset token");

    public static readonly Error Unauthorized = new(
        "EndUser.Unauthorized",
        "Unauthorized access");

    public static readonly Error EmailAlreadyVerified = new(
        "EndUser.EmailAlreadyVerified",
        "Email is already verified");

    public static readonly Error InvalidVerificationToken = new(
        "EndUser.InvalidVerificationToken",
        "Invalid or expired email verification token");

    public static readonly Error InvalidId = new(
        "EndUser.InvalidId",
        "The provided user ID is invalid");

    public static readonly Error AlreadyActive = new(
        "EndUser.AlreadyActive",
        "User is already active");

    public static readonly Error AlreadyDeactivated = new(
        "EndUser.AlreadyDeactivated",
        "User is already deactivated");


    public static readonly Error AlreadyLockedOut = new(
        "EndUser.AlreadyLockedOut",
        "User account is already locked out");

    public static readonly Error NotLockedOut = new(
        "EndUser.NotLockedOut",
        "User account is not locked");
}