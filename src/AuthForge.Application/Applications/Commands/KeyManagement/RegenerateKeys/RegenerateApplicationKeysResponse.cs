namespace AuthForge.Application.Applications.Commands.KeyManagement.RegenerateKeys;

public record RegenerateApplicationKeysResponse(
    string PublicKey,
    string SecretKey,
    DateTime RegeneratedAt,
    string Warning);