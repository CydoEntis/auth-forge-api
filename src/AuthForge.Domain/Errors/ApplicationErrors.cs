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

    public static readonly Error InvalidId = new(
        "Application.InvalidId",
        "Invalid application ID format. Must be a valid GUID.");


    public static readonly Error InvalidOrigin = new(
        "Application.InvalidOrigin",
        "The provided origin URL is invalid");

    public static readonly Error OriginNotFound = new(
        "Application.OriginNotFound",
        "The specified origin does not exist");

    public static readonly Error OriginAlreadyExists = new(
        "Application.OriginAlreadyExists",
        "This origin already exists for the application");

    public static Error InvalidSettingsDetail(string message) => new(
        "Application.InvalidSettings",
        message);

    public static Error InvalidOriginDetail(string message) => new(
        "Application.InvalidOrigin",
        message);

    public static Error OriginErrorDetail(string message) => new(
        "Application.OriginError",
        message);
}