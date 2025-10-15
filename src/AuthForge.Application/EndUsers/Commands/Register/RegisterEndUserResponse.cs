namespace AuthForge.Application.EndUsers.Commands.Register;

public sealed record RegisterEndUserResponse(
    string UserId,
    string Email,
    string FullName,
    string Message);