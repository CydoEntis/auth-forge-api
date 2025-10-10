using AuthForge.Domain.Common;
using AuthForge.Domain.ValueObjects;

namespace AuthForge.Domain.Events;

public class TenantEvents
{
    public sealed record TenantCreatedDomainEvent(TenantId TenantId, string Name) : IDomainEvent;
    public sealed record TenantDeactivatedDomainEvent(TenantId TenantId) : IDomainEvent;
    public sealed record TenantActivatedDomainEvent(TenantId TenantId) : IDomainEvent;
}