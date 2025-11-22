namespace AuthForge.Api.Common;

public static class ErrorCodes
{
    // Setup
    public const string SetupRequired = "Setup.Required";
    public const string SetupAlreadyComplete = "Setup.AlreadyComplete";

    // Validation
    public const string ValidationFailed = "Validation.Failed";
    public const string InvalidToken = "Validation.InvalidToken";

    // Database
    public const string DatabaseConnectionFailed = "Database.ConnectionFailed";

    // Auth
    public const string Unauthorized = "Auth.Unauthorized";
    public const string InvalidCredentials = "Auth.InvalidCredentials";

    // Rate Limit
    public const string RateLimitExceeded = "RateLimit.Exceeded";

    // Cors
    public const string Forbidden = "Forbidden";

    // General
    public const string BadRequest = "BadRequest";
    public const string NotFound = "NotFound";
    public const string InternalError = "Internal";
}