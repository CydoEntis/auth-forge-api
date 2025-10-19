using AuthForge.Application.Common.Interfaces;
using AuthForge.Domain.Common;
using AuthForge.Domain.Errors;
using Mediator;
using ApplicationId = AuthForge.Domain.ValueObjects.ApplicationId;

namespace AuthForge.Application.Applications.Commands.AddAllowedOrigin;

public sealed class AddAllowedOriginCommandHandler : ICommandHandler<AddAllowedOriginCommand, Result>
{
    private readonly IApplicationRepository _applicationRepository;

    public AddAllowedOriginCommandHandler(IApplicationRepository applicationRepository)
    {
        _applicationRepository = applicationRepository;
    }

    public async ValueTask<Result> Handle(AddAllowedOriginCommand command, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(command.ApplicationId, out var appGuid))
            return Result.Failure(ValidationErrors.InvalidGuid("ApplicationId"));

        var applicationId = ApplicationId.Create(appGuid);

        var application = await _applicationRepository.GetByIdAsync(applicationId, cancellationToken);
        if (application is null)
            return Result.Failure(ApplicationErrors.NotFound);

        try
        {
            application.AddAllowedOrigin(command.Origin);
        }
        catch (ArgumentException ex)
        {
            return Result.Failure(new Error("Application.InvalidOrigin", ex.Message));
        }

        _applicationRepository.Update(application);

        return Result.Success();
    }
}