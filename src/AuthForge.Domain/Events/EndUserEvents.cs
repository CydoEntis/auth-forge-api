using AuthForge.Domain.Common;
using AuthForge.Domain.ValueObjects;
using ApplicationId = AuthForge.Domain.ValueObjects.ApplicationId;

namespace AuthForge.Domain.Events;

public sealed record EndUserLoggedInDomainEvent(
    EndUserId UserId,
    ApplicationId ApplicationId,
    Email Email) : IDomainEvent;

public sealed record EndUserEmailVerifiedDomainEvent(
    EndUserId UserId,
    Email Email) : IDomainEvent;

public sealed record EndUserLockedOutDomainEvent(
    EndUserId UserId,
    DateTime LockedOutUntil,
    int FailedAttempts) : IDomainEvent;

public sealed record EndUserPasswordChangedDomainEvent(
    EndUserId UserId) : IDomainEvent;

public sealed record EndUserDeactivatedDomainEvent(
    EndUserId UserId) : IDomainEvent;
    
public sealed record EndUserRefreshTokenRevokedDomainEvent(
    EndUserId UserId,
    string Token) : IDomainEvent;

public sealed record EndUserPasswordResetDomainEvent(EndUserId UserId) : IDomainEvent;

public sealed record EndUserPasswordResetRequestedDomainEvent(
    EndUserId UserId) : IDomainEvent;

public sealed record EndUserEmailVerificationRequestedDomainEvent(
    EndUserId UserId) : IDomainEvent;