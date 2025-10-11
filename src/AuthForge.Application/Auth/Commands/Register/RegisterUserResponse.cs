namespace AuthForge.Application.Auth.Commands.Register;

public sealed record RegisterUserResponse
{
    public required string UserId { get; init; }
    public required string Email { get; init; }
    public required string FullName { get; init; }
    public required bool IsEmailVerified { get; init; }
    public required string Message { get; init; }
}