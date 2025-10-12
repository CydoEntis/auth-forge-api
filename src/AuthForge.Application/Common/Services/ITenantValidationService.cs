using AuthForge.Domain.Common;
using AuthForge.Domain.Entities;

namespace AuthForge.Application.Common.Services;

public interface ITenantValidationService
{
    Task<Result<Tenant>> ValidateTenantAsync(
        string tenantIdString,
        CancellationToken cancellationToken = default);
}