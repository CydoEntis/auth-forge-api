namespace AuthForge.Domain.Errors;

public static class DomainErrors
{
    public static class User
    {
        public static readonly Error NotFound = new(
            "User.NotFound",
            "User with the specified identifier was not found");

        public static readonly Error InvalidCredentials = new(
            "User.InvalidCredentials",
            "The provided credentials are invalid");

        public static readonly Error EmailAlreadyExists = new(
            "User.EmailAlreadyExists",
            "User with the specified email already exists");

        public static readonly Error LockedOut = new(
            "User.LockedOut",
            "User account is locked due to too many failed login attempts");

        public static readonly Error EmailNotVerified = new(
            "User.EmailNotVerified",
            "Email address must be verified before logging in");

        public static readonly Error Inactive = new(
            "User.Inactive",
            "User account is inactive");

        public static readonly Error InvalidEmailVerificationToken = new(
            "User.InvalidEmailVerificationToken",
            "Email verification token is invalid or expired");

        public static readonly Error EmailAlreadyVerified = new(
            "User.EmailAlreadyVerified",
            "Email address is already verified");
    }

    public static class Tenant
    {
        public static readonly Error NotFound = new(
            "Tenant.NotFound",
            "Tenant with the specified identifier was not found");

        public static readonly Error Inactive = new(
            "Tenant.Inactive",
            "Tenant is inactive");

        public static readonly Error SlugAlreadyExists = new(
            "Tenant.SlugAlreadyExists",
            "Tenant with the specified slug already exists");

        public static readonly Error MaxUsersReached = new(
            "Tenant.MaxUsersReached",
            "Tenant has reached maximum number of users");
    }

    public static class RefreshToken
    {
        public static readonly Error NotFound = new(
            "RefreshToken.NotFound",
            "Refresh token was not found");

        public static readonly Error Expired = new(
            "RefreshToken.Expired",
            "Refresh token has expired");

        public static readonly Error Revoked = new(
            "RefreshToken.Revoked",
            "Refresh token has been revoked");

        public static readonly Error Invalid = new(
            "RefreshToken.Invalid",
            "Refresh token is invalid");
    }

    public static class PasswordReset
    {
        public static readonly Error TokenNotFound = new(
            "PasswordReset.TokenNotFound",
            "Password reset token was not found");

        public static readonly Error TokenExpired = new(
            "PasswordReset.TokenExpired",
            "Password reset token has expired");

        public static readonly Error TokenAlreadyUsed = new(
            "PasswordReset.TokenAlreadyUsed",
            "Password reset token has already been used");

        public static readonly Error InvalidToken = new(
            "PasswordReset.InvalidToken",
            "Password reset token is invalid");
    }

    public static class Authentication
    {
        public static readonly Error InvalidToken = new(
            "Authentication.InvalidToken",
            "The provided token is invalid");

        public static readonly Error TokenExpired = new(
            "Authentication.TokenExpired",
            "The token has expired");

        public static readonly Error Unauthorized = new(
            "Authentication.Unauthorized",
            "User is not authorized to perform this action");
    }

    public static class Validation
    {
        public static Error Required(string fieldName) => new(
            "Validation.Required",
            $"{fieldName} is required");

        public static Error InvalidEmail() => new(
            "Validation.InvalidEmail",
            "Email address is not in a valid format");

        public static Error MinLength(string fieldName, int minLength) => new(
            "Validation.MinLength",
            $"{fieldName} must be at least {minLength} characters");

        public static Error MaxLength(string fieldName, int maxLength) => new(
            "Validation.MaxLength",
            $"{fieldName} must not exceed {maxLength} characters");
    }
}