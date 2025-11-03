namespace AuthForge.Application.Common.Interfaces;

// Service for encrypting and decrypting sensitive data at rest.
// Uses AES-256-CBC encryption.
public interface IEncryptionService
{
    string Encrypt(string plainText);
    string Decrypt(string cipherText);
}