using AuthForge.Domain.Common;
using AuthForge.Domain.ValueObjects;

namespace AuthForge.Domain.Events;

public sealed record AuthForgeRefreshTokenRevokedDomainEvent(
    AuthForgeUserId UserId,
    string Token) : IDomainEvent;