namespace AuthForge.Domain.Errors;

public static class AdminErrors
{
    public static readonly Error InvalidCredentials = new(
        "Admin.InvalidCredentials",
        "Invalid credentials");
}