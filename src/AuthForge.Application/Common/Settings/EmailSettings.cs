namespace AuthForge.Application.Common.Settings;

public class EmailSettings
{
    public string Provider { get; init; } = "Resend";
    public required string ApiKey { get; init; }
    public required string FromEmail { get; init; }
    public required string FromName { get; init; }
}