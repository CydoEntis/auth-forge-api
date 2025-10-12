using AuthForge.Application.Common.Interfaces;
using AuthForge.Domain.Common;
using AuthForge.Domain.Entities;
using AuthForge.Domain.Errors;
using AuthForge.Domain.ValueObjects;

namespace AuthForge.Application.Common.Services;

public sealed class TenantValidationService : ITenantValidationService
{
    private readonly ITenantRepository _tenantRepository;

    public TenantValidationService(ITenantRepository tenantRepository)
    {
        _tenantRepository = tenantRepository;
    }

    public async Task<Result<Tenant>> ValidateTenantAsync(
        string tenantIdString,
        CancellationToken cancellationToken = default)
    {
        if (!Guid.TryParse(tenantIdString, out var tenantGuid))
        {
            return Result<Tenant>.Failure(
                DomainErrors.Validation.InvalidGuid("Tenant ID"));
        }

        var tenantId = TenantId.Create(tenantGuid);

        var tenant = await _tenantRepository.GetByIdAsync(tenantId, cancellationToken);
        if (tenant is null)
        {
            return Result<Tenant>.Failure(DomainErrors.Tenant.NotFound);
        }

        if (!tenant.IsActive)
        {
            return Result<Tenant>.Failure(DomainErrors.Tenant.Inactive);
        }

        return Result<Tenant>.Success(tenant);
    }
}