using AuthForge.Domain.ValueObjects;

namespace AuthForge.Infrastructure.Settings;

public class AdminSettings
{
    public required string Email { get; init; }
    public required string Password { get; init; } 
    
    internal HashedPassword? HashedPasswordInternal { get; set; }
}