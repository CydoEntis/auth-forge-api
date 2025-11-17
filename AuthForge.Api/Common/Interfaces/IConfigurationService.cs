using AuthForge.Api.Entities;

namespace AuthForge.Api.Common.Interfaces;

public interface IConfigurationService
{
    Task<Configuration?> GetAsync(CancellationToken ct = default);
    Task<bool> IsSetupCompleteAsync(CancellationToken ct = default);
}