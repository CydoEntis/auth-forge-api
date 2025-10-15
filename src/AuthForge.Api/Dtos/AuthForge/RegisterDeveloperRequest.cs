namespace AuthForge.Api.Dtos.AuthForge;

public record RegisterDeveloperRequest(
    string Email,
    string Password,
    string FirstName,
    string LastName);