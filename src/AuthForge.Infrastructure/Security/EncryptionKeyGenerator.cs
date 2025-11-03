using System.Security.Cryptography;

namespace AuthForge.Infrastructure.Security;


public static class EncryptionKeyGenerator
{
    public static string GenerateKey()
    {
        var key = new byte[32]; // 256 bits
        RandomNumberGenerator.Fill(key);
        return Convert.ToBase64String(key);
    }

    public static string GenerateIV()
    {
        var iv = new byte[16]; // 128 bits
        RandomNumberGenerator.Fill(iv);
        return Convert.ToBase64String(iv);
    }

  // TODO: Might remove and think of a better way to display/generate the keys
    public static void GenerateAndPrint()
    {
        Console.WriteLine("=".PadRight(70, '='));
        Console.WriteLine("AES-256 Encryption Key Generator");
        Console.WriteLine("=".PadRight(70, '='));
        Console.WriteLine();
        Console.WriteLine("⚠️  SECURITY WARNING:");
        Console.WriteLine("   - Store these values in Azure Key Vault or AWS Secrets Manager");
        Console.WriteLine("   - NEVER commit these to source control");
        Console.WriteLine("   - If lost, you cannot decrypt existing data");
        Console.WriteLine();
        Console.WriteLine("Add these to your appsettings.json or environment variables:");
        Console.WriteLine();
        Console.WriteLine("\"Encryption\": {");
        Console.WriteLine($"  \"Key\": \"{GenerateKey()}\",");
        Console.WriteLine($"  \"IV\": \"{GenerateIV()}\"");
        Console.WriteLine("}");
        Console.WriteLine();
        Console.WriteLine("=".PadRight(70, '='));
    }
}