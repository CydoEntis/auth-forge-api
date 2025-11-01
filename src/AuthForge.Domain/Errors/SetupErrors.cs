namespace AuthForge.Domain.Errors;

public static class SetupErrors
{
    public static Error AlreadyConfigured(string component) => new(
        "Setup.AlreadyConfigured",
        $"{component} is already configured");

    public static Error ConnectionTestFailed(string component) => new(
        "Setup.ConnectionTestFailed",
        $"Failed to connect to {component}. Please check your configuration.");

    public static Error InvalidStep => new(
        "Setup.InvalidStep",
        "Cannot perform this action at the current setup step");

    public static Error AlreadyComplete => new(
        "Setup.AlreadyComplete",
        "Setup has already been completed");
}