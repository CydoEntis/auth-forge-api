using AuthForge.Domain.ValueObjects;

namespace AuthForge.Application.Common.Interfaces;

public interface ITenantContext
{
    TenantId? TenantId { get; }

    void SetTenant(TenantId tenantId);
}