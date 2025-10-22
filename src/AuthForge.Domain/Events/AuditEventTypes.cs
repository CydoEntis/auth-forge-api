namespace AuthForge.Domain.Events;

// TODO: Temporary for now - Refactor to match other DomainEvents,
public static class AuditEventTypes
{
    // Authentication Events
    public const string UserLoginSuccess = "user.login.success";
    public const string UserLoginFailed = "user.login.failed";
    public const string UserLockedOut = "user.locked_out";
    public const string UserRegistered = "user.registered";

    // Password Events
    public const string PasswordChanged = "password.changed";
    public const string PasswordReset = "password.reset";
    public const string PasswordResetRequested = "password.reset.requested";

    // Email Events
    public const string EmailVerified = "email.verified";
    public const string EmailVerificationRequested = "email.verification.requested";

    // Admin Actions
    public const string UserDeactivated = "admin.user.deactivated";
    public const string UserActivated = "admin.user.activated";
    public const string UserLocked = "admin.user.locked";
    public const string UserUnlocked = "admin.user.unlocked";
    public const string UserDeleted = "admin.user.deleted";
    public const string EmailVerifiedManually = "admin.email.verified";
}