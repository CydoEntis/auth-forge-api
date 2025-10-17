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