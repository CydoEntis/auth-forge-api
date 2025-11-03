using System.Security.Cryptography;
using System.Text;
using AuthForge.Application.Common.Interfaces;
using AuthForge.Infrastructure.Data;
using Microsoft.Extensions.Logging;

namespace AuthForge.Infrastructure.Security;

public sealed class AesEncryptionService : IEncryptionService
{
    private readonly byte[] _key;
    private readonly byte[] _iv;
    private readonly ILogger<AesEncryptionService> _logger;

    public AesEncryptionService(
        ConfigurationDatabase configDb, 
        ILogger<AesEncryptionService> logger)
    {
        _logger = logger;

        var encryptionKey = configDb.GetAsync("encryption_key").GetAwaiter().GetResult();
        var encryptionIv = configDb.GetAsync("encryption_iv").GetAwaiter().GetResult();

        if (string.IsNullOrWhiteSpace(encryptionKey))
        {
            throw new InvalidOperationException(
                "Encryption key not found. Please complete setup first.");
        }

        if (string.IsNullOrWhiteSpace(encryptionIv))
        {
            throw new InvalidOperationException(
                "Encryption IV not found. Please complete setup first.");
        }

        try
        {
            _key = Convert.FromBase64String(encryptionKey);
            _iv = Convert.FromBase64String(encryptionIv);
        }
        catch (FormatException ex)
        {
            throw new InvalidOperationException(
                "Invalid encryption key or IV format. Must be Base64-encoded strings.", ex);
        }

        if (_key.Length != 32)
        {
            throw new InvalidOperationException(
                $"Invalid encryption key size. Expected 32 bytes (256 bits), got {_key.Length} bytes.");
        }

        if (_iv.Length != 16)
        {
            throw new InvalidOperationException(
                $"Invalid IV size. Expected 16 bytes (128 bits), got {_iv.Length} bytes.");
        }

        _logger.LogInformation("AES-256-CBC encryption service initialized successfully");
    }

    public string Encrypt(string plainText)
    {
        if (string.IsNullOrEmpty(plainText))
        {
            return plainText;
        }

        try
        {
            using var aes = Aes.Create();
            aes.Key = _key;
            aes.IV = _iv;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            using var encryptor = aes.CreateEncryptor();
            var plainBytes = Encoding.UTF8.GetBytes(plainText);
            var cipherBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);

            return Convert.ToBase64String(cipherBytes);
        }
        catch (CryptographicException ex)
        {
            _logger.LogError(ex, "Failed to encrypt data");
            throw new InvalidOperationException("Encryption failed. See inner exception for details.", ex);
        }
    }

    public string Decrypt(string cipherText)
    {
        if (string.IsNullOrEmpty(cipherText))
        {
            return cipherText;
        }

        try
        {
            using var aes = Aes.Create();
            aes.Key = _key;
            aes.IV = _iv;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            using var decryptor = aes.CreateDecryptor();
            var cipherBytes = Convert.FromBase64String(cipherText);
            var plainBytes = decryptor.TransformFinalBlock(cipherBytes, 0, cipherBytes.Length);

            return Encoding.UTF8.GetString(plainBytes);
        }
        catch (CryptographicException ex)
        {
            _logger.LogError(ex, "Failed to decrypt data");
            throw new InvalidOperationException("Decryption failed. See inner exception for details.", ex);
        }
        catch (FormatException ex)
        {
            _logger.LogError(ex, "Invalid ciphertext format (not valid Base64)");
            throw new InvalidOperationException("Invalid encrypted data format.", ex);
        }
    }
}