using AuthForge.Domain.Common;
using AuthForge.Domain.ValueObjects;
using ApplicationId = AuthForge.Domain.ValueObjects.ApplicationId;

namespace AuthForge.Domain.Events;

public sealed record ApplicationCreatedDomainEvent(
    ApplicationId ApplicationId,
    string Name,
    string Slug
) : IDomainEvent;

public sealed record ApplicationDeactivatedDomainEvent(
    ApplicationId ApplicationId) : IDomainEvent;

public sealed record ApplicationKeysRegeneratedDomainEvent(
    ApplicationId ApplicationId,
    DateTime RegeneratedAt
) : IDomainEvent;

public sealed record JwtSecretRegeneratedDomainEvent(
    ApplicationId ApplicationId,
    DateTime RegeneratedAt
) : IDomainEvent;