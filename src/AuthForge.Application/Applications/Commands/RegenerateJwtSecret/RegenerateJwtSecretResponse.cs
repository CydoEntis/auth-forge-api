namespace AuthForge.Application.Applications.Commands.RegenerateJwtSecret;

public record RegenerateJwtSecretResponse(
    string JwtSecret,
    DateTime RegeneratedAt,
    string Warning);