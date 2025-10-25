namespace AuthForge.Domain.Errors;

public static class AdminErrors
{
    public static readonly Error NotFound = new(
        "Admin.NotFound",
        "Admin account not found");

    public static readonly Error InvalidCredentials = new(
        "Admin.InvalidCredentials",
        "Invalid email or password");

    public static readonly Error DuplicateEmail = new(
        "Admin.DuplicateEmail",
        "An admin account already exists");

    public static readonly Error AlreadyExists = new(
        "Admin.AlreadyExists",
        "Admin account has already been set up. Use login instead.");

    public static readonly Error LockedOut = new(
        "Admin.LockedOut",
        "Account is locked due to too many failed login attempts");

    public static readonly Error InvalidResetToken = new(
        "Admin.InvalidResetToken",
        "Invalid or expired password reset token");

    public static readonly Error Unauthorized = new(
        "Admin.Unauthorized",
        "Unauthorized access");
}