namespace AuthForge.Api.Dtos.EndUsers;

public record RegisterEndUserRequest(
    string ApplicationId,
    string Email,
    string Password,
    string FirstName,
    string LastName);