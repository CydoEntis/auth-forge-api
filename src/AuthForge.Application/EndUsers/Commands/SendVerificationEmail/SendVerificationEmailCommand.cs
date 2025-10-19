using AuthForge.Domain.Common;
using AuthForge.Domain.ValueObjects;
using Mediator;

namespace AuthForge.Application.EndUsers.Commands.SendVerificationEmail;

public record SendVerificationEmailCommand(
    EndUserId UserId) : ICommand<Result<SendVerificationEmailResponse>>;

