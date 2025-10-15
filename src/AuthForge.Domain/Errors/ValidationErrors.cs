namespace AuthForge.Domain.Errors;

public static class ValidationErrors
{
    public static Error Required(string fieldName) => new(
        "Validation.Required",
        $"{fieldName} is required");

    public static Error InvalidEmail() => new(
        "Validation.InvalidEmail",
        "Email address is not in a valid format");

    public static Error InvalidGuid(string fieldName) => new(
        "Validation.InvalidGuid",
        $"{fieldName} must be a valid GUID");

    public static Error InvalidPassword() => new(
        "Validation.InvalidPassword",
        "Password does not meet the required criteria");

    public static Error MinLength(string fieldName, int minLength) => new(
        "Validation.MinLength",
        $"{fieldName} must be at least {minLength} characters");

    public static Error MaxLength(string fieldName, int maxLength) => new(
        "Validation.MaxLength",
        $"{fieldName} must not exceed {maxLength} characters");
    
    public static Error InvalidCredentials => new(
        "Validation.InvalidCredentials",
        "Invalid credentials");
}