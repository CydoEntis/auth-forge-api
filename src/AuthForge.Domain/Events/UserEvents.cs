using AuthForge.Domain.Common;
using AuthForge.Domain.ValueObjects;

namespace AuthForge.Domain.Events;

public sealed record UserRegisteredDomainEvent(
    UserId UserId,
    TenantId TenantId,
    Email Email,
    string FirstName,
    string LastName) : IDomainEvent;

public sealed record UserLoggedInDomainEvent(
    UserId UserId,
    TenantId TenantId,
    Email Email) : IDomainEvent;

public sealed record UserEmailVerifiedDomainEvent(
    UserId UserId,
    Email Email) : IDomainEvent;

public sealed record UserLockedOutDomainEvent(
    UserId UserId,
    DateTime LockedOutUntil,
    int FailedAttempts) : IDomainEvent;

public sealed record UserPasswordChangedDomainEvent(
    UserId UserId) : IDomainEvent;

public sealed record UserDeactivatedDomainEvent(
    UserId UserId) : IDomainEvent;

public sealed record RefreshTokenRevokedDomainEvent(
    UserId UserId,
    string Token) : IDomainEvent;