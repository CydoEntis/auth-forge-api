using AuthForge.Domain.Common;
using Mediator;

namespace AuthForge.Application.Auth.Commands.Register;

public sealed record RegisterUserCommand : ICommand<Result<RegisterUserResponse>>
{
    public required string TenantId { get; init; }
    public required string Email { get; init; }
    public required string Password { get; init; }
    public required string FirstName { get; init; }
    public required string LastName { get; init; }
}