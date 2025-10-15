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
}