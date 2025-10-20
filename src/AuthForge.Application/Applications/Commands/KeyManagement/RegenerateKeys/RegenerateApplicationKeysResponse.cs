namespace AuthForge.Application.Applications.Commands.RegenerateKeys;

public record RegenerateApplicationKeysResponse(
    string PublicKey,
    string SecretKey,
    DateTime RegeneratedAt,
    string Warning);