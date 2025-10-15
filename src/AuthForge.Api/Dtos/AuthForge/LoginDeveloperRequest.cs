namespace AuthForge.Api.Dtos.AuthForge;

public record LoginDeveloperRequest(
    string Email,
    string Password);