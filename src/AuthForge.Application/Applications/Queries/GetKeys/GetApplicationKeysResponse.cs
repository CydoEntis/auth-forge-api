namespace AuthForge.Application.Applications.Queries.GetKeys;

public record GetApplicationKeysResponse(
    string PublicKey,
    string SecretKey,  
    DateTime CreatedAt);