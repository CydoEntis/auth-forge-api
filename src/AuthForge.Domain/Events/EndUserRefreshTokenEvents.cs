using AuthForge.Domain.Common;
using AuthForge.Domain.ValueObjects;

namespace AuthForge.Domain.Events;

public sealed record EndUserRefreshTokenRevokedDomainEvent(
    EndUserId UserId,
    string Token) : IDomainEvent;