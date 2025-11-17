using AuthForge.Api.Common.Interfaces;
using AuthForge.Api.Data;
using AuthForge.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace AuthForge.Api.Common.Services;

public class ConfigurationService : IConfigurationService
{
    private readonly ConfigDbContext _configDb;
    private readonly IEncryptionService _encryptionService;

    public ConfigurationService(
        ConfigDbContext configDb,
        IEncryptionService encryptionService)
    {
        _configDb = configDb;
        _encryptionService = encryptionService;
    }

    public async Task<Configuration?> GetAsync(CancellationToken ct = default)
    {
        return await _configDb.Configuration.FirstOrDefaultAsync(ct);
    }

    public async Task<string?> GetDecryptedSmtpPasswordAsync(CancellationToken ct = default)
    {
        var config = await GetAsync(ct);
        if (string.IsNullOrEmpty(config?.SmtpPasswordEncrypted))
            return null;

        return _encryptionService.Decrypt(config.SmtpPasswordEncrypted);
    }

    public async Task<string?> GetDecryptedResendApiKeyAsync(CancellationToken ct = default)
    {
        var config = await GetAsync(ct);
        if (string.IsNullOrEmpty(config?.ResendApiKeyEncrypted))
            return null;

        return _encryptionService.Decrypt(config.ResendApiKeyEncrypted);
    }

    public async Task<bool> IsSetupCompleteAsync(CancellationToken ct = default)
    {
        var config = await _configDb.Configuration.FirstOrDefaultAsync(ct);
        return config?.IsSetupComplete ?? false;
    }
}