using AuthForge.Domain.Common;
using AuthForge.Domain.ValueObjects;
using Mediator;

namespace AuthForge.Application.EndUsers.Commands.ChangePassword;

public record ChangePasswordCommand(
    EndUserId UserId,
    string CurrentPassword,
    string NewPassword,
    string ConfirmPassword) : ICommand<Result<ChangePasswordResponse>>;