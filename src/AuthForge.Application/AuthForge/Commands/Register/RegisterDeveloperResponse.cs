namespace AuthForge.Application.AuthForge.Commands.Register;

public sealed record RegisterDeveloperResponse(
    string UserId,
    string Email,
    string FullName,
    string Message);