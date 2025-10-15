namespace AuthForge.Api.Dtos.EndUsers;

public record LoginEndUserRequest(
    string ApplicationId,
    string Email,
    string Password);