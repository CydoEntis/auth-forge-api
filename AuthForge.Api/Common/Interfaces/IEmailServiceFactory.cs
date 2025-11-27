using AuthForge.Api.Entities;
using AuthForge.Providers.Interfaces;

namespace AuthForge.Api.Common.Interfaces;

public interface IEmailServiceFactory
{
    Task<IEmailService> CreateAsync(CancellationToken cancellationToken = default);
    Task<string> GetFromAddressAsync(CancellationToken cancellationToken = default);
    Task<string?> GetFromNameAsync(CancellationToken cancellationToken = default);

    Task<IEmailService> CreateForApplicationAsync(
        Application application,
        CancellationToken cancellationToken = default);

    Task<(string FromAddress, string? FromName)> GetFromDetailsForApplicationAsync(
        Application application,
        CancellationToken cancellationToken = default);
}