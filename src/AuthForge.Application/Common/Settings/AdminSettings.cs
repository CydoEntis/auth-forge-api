using AuthForge.Domain.ValueObjects;

namespace AuthForge.Application.Common.Settings;

public class AdminSettings
{
    public required string Email { get; init; }
    public required string Password { get; init; } 
    
    internal HashedPassword? HashedPasswordInternal { get; set; }
}