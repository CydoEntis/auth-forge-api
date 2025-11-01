namespace AuthForge.Domain.Errors;

public static class SetupErrors
{
    public static readonly Error SetupAlreadyComplete = new(
        "Setup.AlreadyComplete",
        "Setup has already been completed.");

    public static readonly Error SetupNotComplete = new(
        "Setup.NotComplete",
        "Setup has not been completed yet.");

    public static readonly Error InvalidDatabaseConfiguration = new(
        "Setup.InvalidDatabaseConfiguration",
        "The database configuration is invalid.");

    public static readonly Error InvalidEmailConfiguration = new(
        "Setup.InvalidEmailConfiguration",
        "The email configuration is invalid.");

    public static readonly Error DatabaseConnectionFailed = new(
        "Setup.DatabaseConnectionFailed",
        "Failed to connect to the database. Please check your connection settings and try again.");

    public static readonly Error EmailTestFailed = new(
        "Setup.EmailTestFailed",
        "Failed to send test email. Please check your email configuration and try again.");
}