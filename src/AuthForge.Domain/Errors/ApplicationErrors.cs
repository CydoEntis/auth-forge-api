namespace AuthForge.Domain.Errors;

public static class ApplicationErrors
{
    public static readonly Error NotFound = new(
        "Application.NotFound",
        "Application not found");

    public static readonly Error Inactive = new(
        "Application.Inactive",
        "Application is inactive");
    
    
    public static readonly Error SlugAlreadyExists = new(
        "Application.SlugAlreadyExists",
        "An application with this slug already exists");

    public static readonly Error Unauthorized = new(
        "Application.Unauthorized",
        "You don't have permission to access this application");

    public static readonly Error InvalidSettings = new(
        "Application.InvalidSettings",
        "Application settings are invalid");

    public static Error InvalidSettingsDetail(string message) => new(
        "Application.InvalidSettings",
        message);
}