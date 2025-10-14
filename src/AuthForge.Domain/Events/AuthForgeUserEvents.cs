using AuthForge.Domain.Common;
using AuthForge.Domain.ValueObjects;

namespace AuthForge.Domain.Events;

public sealed record AuthForgeUserRegisteredDomainEvent(
    AuthForgeUserId UserId,
    Email Email,
    string FirstName,
    string LastName) : IDomainEvent;

public sealed record AuthForgeUserLoggedInDomainEvent(
    AuthForgeUserId UserId,
    Email Email) : IDomainEvent;

public sealed record AuthForgeUserEmailVerifiedDomainEvent(
    AuthForgeUserId UserId,
    Email Email) : IDomainEvent;

public sealed record AuthForgeUserPasswordChangedDomainEvent(
    AuthForgeUserId UserId) : IDomainEvent;

public sealed record AuthForgeUserDeactivatedDomainEvent(
    AuthForgeUserId UserId) : IDomainEvent;