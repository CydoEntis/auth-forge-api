using AuthForge.Domain.Common;
using Mediator;

namespace AuthForge.Application.Applications.Commands.RegenerateJwtSecret;

public record RegenerateJwtSecretCommand(string ApplicationId) 
    : ICommand<Result<RegenerateJwtSecretResponse>>;