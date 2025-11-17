using AuthForge.Api.Common.Interfaces;
using Microsoft.AspNetCore.DataProtection;

namespace AuthForge.Api.Common.Services;

public class EncryptionService : IEncryptionService
{
    private readonly IDataProtector _protector;
    private readonly ILogger<EncryptionService> _logger;

    public EncryptionService(
        IDataProtectionProvider dataProtectionProvider,
        ILogger<EncryptionService> logger)
    {
        _protector = dataProtectionProvider.CreateProtector("AuthForge.Secrets");
        _logger = logger;
        
        _logger.LogInformation("EncryptionService initialized with protector: AuthForge.Secrets");
    }

    public string Encrypt(string plainText)
    {
        if (string.IsNullOrEmpty(plainText))
            return plainText;

        var encrypted = _protector.Protect(plainText);
        _logger.LogInformation("Encrypted data. Length: {Length}", encrypted?.Length ?? 0);
        return encrypted;
    }

    public string Decrypt(string cipherText)
    {
        if (string.IsNullOrEmpty(cipherText))
            return cipherText;

        try
        {
            _logger.LogInformation("Attempting to decrypt data. Length: {Length}", cipherText?.Length ?? 0);
            var decrypted = _protector.Unprotect(cipherText);
            _logger.LogInformation("Successfully decrypted data");
            return decrypted;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Decryption failed! CipherText: {Cipher}", cipherText?.Substring(0, Math.Min(50, cipherText?.Length ?? 0)));
            throw;
        }
    }
}