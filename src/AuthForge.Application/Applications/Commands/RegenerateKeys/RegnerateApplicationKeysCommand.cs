using AuthForge.Domain.Common;
using Mediator;
using ApplicationId = AuthForge.Domain.ValueObjects.ApplicationId;

namespace AuthForge.Application.Applications.Commands.RegenerateKeys;

public record RegenerateApplicationKeysCommand(ApplicationId ApplicationId) 
    : ICommand<Result<RegenerateApplicationKeysResponse>>;