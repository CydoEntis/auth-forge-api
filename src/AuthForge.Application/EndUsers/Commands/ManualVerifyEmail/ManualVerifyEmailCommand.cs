using AuthForge.Domain.Common;
using AuthForge.Domain.ValueObjects;
using Mediator;

namespace AuthForge.Application.EndUsers.Commands.ManualVerifyEmail;

public record ManualVerifyEmailCommand(
    EndUserId UserId) : ICommand<Result<ManualVerifyEmailResponse>>;