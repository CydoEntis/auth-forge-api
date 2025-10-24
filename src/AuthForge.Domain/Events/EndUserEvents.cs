using AuthForge.Domain.Common;
using AuthForge.Domain.ValueObjects;
using ApplicationId = AuthForge.Domain.ValueObjects.ApplicationId;

namespace AuthForge.Domain.Events;

// REGISTRATION & AUTHENTICATION

public sealed record EndUserRegisteredDomainEvent(
    EndUserId UserId,
    ApplicationId ApplicationId,
    Email Email,
    string FirstName,
    string LastName) : IDomainEvent;

public sealed record EndUserLoggedInDomainEvent(
    EndUserId UserId,
    ApplicationId ApplicationId,
    Email Email) : IDomainEvent;

public sealed record EndUserLoginFailedDomainEvent(
    EndUserId UserId,
    ApplicationId ApplicationId,
    Email Email,
    int FailedAttempts) : IDomainEvent;

// EMAIL VERIFICATION

public sealed record EndUserEmailVerifiedDomainEvent(
    EndUserId UserId,
    Email Email) : IDomainEvent;

public sealed record EndUserEmailVerificationRequestedDomainEvent(
    EndUserId UserId) : IDomainEvent;

public sealed record EndUserEmailVerifiedManuallyDomainEvent(
    EndUserId UserId,
    ApplicationId ApplicationId,
    Email Email) : IDomainEvent;

// PASSWORD MANAGEMENT

public sealed record EndUserPasswordChangedDomainEvent(
    EndUserId UserId) : IDomainEvent;

public sealed record EndUserPasswordResetDomainEvent(
    EndUserId UserId) : IDomainEvent;

public sealed record EndUserPasswordResetRequestedDomainEvent(
    EndUserId UserId) : IDomainEvent;

// ACCOUNT LOCKOUT

public sealed record EndUserLockedOutDomainEvent(
    EndUserId UserId,
    DateTime LockedOutUntil,
    int FailedAttempts) : IDomainEvent;

public sealed record EndUserManuallyLockedDomainEvent(
    EndUserId UserId,
    ApplicationId ApplicationId,
    DateTime LockedUntil) : IDomainEvent;

public sealed record EndUserUnlockedDomainEvent(
    EndUserId UserId,
    ApplicationId ApplicationId) : IDomainEvent;

// ACCOUNT STATUS

public sealed record EndUserDeactivatedDomainEvent(
    EndUserId UserId,
    ApplicationId ApplicationId) : IDomainEvent;

public sealed record EndUserActivatedDomainEvent(
    EndUserId UserId,
    ApplicationId ApplicationId) : IDomainEvent;

public sealed record EndUserDeletedDomainEvent(
    EndUserId UserId,
    ApplicationId ApplicationId,
    Email Email) : IDomainEvent;

// REFRESH TOKEN

public sealed record EndUserRefreshTokenRevokedDomainEvent(
    EndUserId UserId,
    string Token) : IDomainEvent;