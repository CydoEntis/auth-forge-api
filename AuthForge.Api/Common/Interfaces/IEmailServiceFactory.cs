using AuthForge.Providers.Interfaces;

namespace AuthForge.Api.Common.Interfaces;

public interface IEmailServiceFactory
{
    Task<IEmailService> CreateAsync(CancellationToken ct = default);
    Task<string> GetFromAddressAsync(CancellationToken ct = default);
    Task<string?> GetFromNameAsync(CancellationToken ct = default);
}