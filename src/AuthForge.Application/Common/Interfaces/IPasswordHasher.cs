using AuthForge.Domain.ValueObjects;

namespace AuthForge.Application.Common.Interfaces;

public interface IPasswordHasher
{
    HashedPassword HashPassword(string password);
    bool VerifyPassword(string password, HashedPassword hashedPassword);
}