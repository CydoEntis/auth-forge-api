using AuthForge.Application.Common.Interfaces;

namespace AuthForge.Infrastructure.Security;

public sealed class NoOpEncryptionService : IEncryptionService
{
    public string Encrypt(string plainText) => plainText;
    public string Decrypt(string cipherText) => cipherText;
}