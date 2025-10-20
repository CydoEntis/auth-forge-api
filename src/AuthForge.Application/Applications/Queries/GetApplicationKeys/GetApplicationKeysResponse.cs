namespace AuthForge.Application.Applications.Queries.GetApplicationKeys;

public record GetApplicationKeysResponse(
    string PublicKey,
    string SecretKey,  
    DateTime CreatedAt);