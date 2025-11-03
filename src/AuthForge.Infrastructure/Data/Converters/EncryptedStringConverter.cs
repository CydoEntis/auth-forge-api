using AuthForge.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace AuthForge.Infrastructure.Data.Converters;


public sealed class EncryptedStringConverter : ValueConverter<string, string>
{
    public EncryptedStringConverter(IEncryptionService encryptionService)
        : base(
            plainText => encryptionService.Encrypt(plainText),
            cipherText => encryptionService.Decrypt(cipherText))
    {
    }
}