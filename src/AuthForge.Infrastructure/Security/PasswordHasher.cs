using System.Security.Cryptography;
using System.Text;
using AuthForge.Application.Common.Interfaces;
using AuthForge.Domain.ValueObjects;

namespace AuthForge.Infrastructure.Security;

public class PasswordHasher : IPasswordHasher
{
    private const int SaltSize = 32;
    private const int HashSize = 32;
    private const int Iterations = 100000;

    public HashedPassword HashPassword(string password)
    {
        using var rng = RandomNumberGenerator.Create();
        var salt = new byte[SaltSize];
        rng.GetBytes(salt);

        var hash = GenerateHash(password, salt);

        return HashedPassword.FromHash(
            Convert.ToBase64String(hash),
            Convert.ToBase64String(salt));
    }

    public bool VerifyPassword(string password, HashedPassword hashedPassword)
    {
        var saltBytes = Convert.FromBase64String(hashedPassword.Salt);
        var hashBytes = Convert.FromBase64String(hashedPassword.Hash);

        var computedHash = GenerateHash(password, saltBytes);

        return CryptographicOperations.FixedTimeEquals(hashBytes, computedHash);
    }

    private static byte[] GenerateHash(string password, byte[] salt)
    {
        using var pbkdf2 = new Rfc2898DeriveBytes(
            Encoding.UTF8.GetBytes(password),
            salt,
            Iterations,
            HashAlgorithmName.SHA256);

        return pbkdf2.GetBytes(HashSize);
    }
}