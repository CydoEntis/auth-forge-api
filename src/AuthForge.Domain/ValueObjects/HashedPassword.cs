using System.Security.Cryptography;

namespace AuthForge.Domain.ValueObjects;

public sealed record HashedPassword
{
    public string Hash { get; init; } = string.Empty;
    public string Salt { get; init; } = string.Empty;

    private HashedPassword()
    {
    }

    private HashedPassword(string hash, string salt)
    {
        Hash = hash;
        Salt = salt;
    }

    public static HashedPassword Create(string plainTextPassword)
    {
        if (string.IsNullOrWhiteSpace(plainTextPassword))
            throw new ArgumentException("Password cannot be empty", nameof(plainTextPassword));

        if (plainTextPassword.Length < 8)
            throw new ArgumentException("Password must be at least 8 characters", nameof(plainTextPassword));

        byte[] saltBytes = RandomNumberGenerator.GetBytes(128 / 8);
        string salt = Convert.ToBase64String(saltBytes);

        string hash = HashPassword(plainTextPassword, saltBytes);

        return new HashedPassword(hash, salt);
    }

    public static HashedPassword FromHash(string hash, string salt)
    {
        if (string.IsNullOrWhiteSpace(hash))
            throw new ArgumentException("Hash cannot be empty", nameof(hash));

        if (string.IsNullOrWhiteSpace(salt))
            throw new ArgumentException("Salt cannot be empty", nameof(salt));

        return new HashedPassword(hash, salt);
    }

    public bool Verify(string plainTextPassword)
    {
        if (string.IsNullOrWhiteSpace(plainTextPassword))
            return false;

        byte[] saltBytes = Convert.FromBase64String(Salt);
        string hashToVerify = HashPassword(plainTextPassword, saltBytes);

        return Hash == hashToVerify;
    }

    private static string HashPassword(string password, byte[] salt)
    {
        using var pbkdf2 = new Rfc2898DeriveBytes(
            password: password,
            salt: salt,
            iterations: 100000,
            hashAlgorithm: HashAlgorithmName.SHA256);

        byte[] hashBytes = pbkdf2.GetBytes(256 / 8);

        return Convert.ToBase64String(hashBytes);
    }
}