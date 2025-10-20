namespace AuthForge.Application.Applications.Commands.RegenerateApplicationKeys;

public record RegenerateApplicationKeysResponse(
    string PublicKey,
    string SecretKey,
    DateTime RegeneratedAt,
    string Warning);