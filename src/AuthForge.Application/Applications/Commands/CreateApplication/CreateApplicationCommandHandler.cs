using AuthForge.Application.Common.Interfaces;
using AuthForge.Domain.Common;
using Mediator;
using Microsoft.Extensions.Logging;
using App = AuthForge.Domain.Entities.Application;

namespace AuthForge.Application.Applications.Commands.CreateApplication;

public sealed class CreateApplicationCommandHandler
    : ICommandHandler<CreateApplicationCommand, Result<CreateApplicationResponse>>
{
    private readonly IApplicationRepository _applicationRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CreateApplicationCommandHandler> _logger;

    public CreateApplicationCommandHandler(
        IApplicationRepository applicationRepository,
        IUnitOfWork unitOfWork,
        ILogger<CreateApplicationCommandHandler> logger)
    {
        _applicationRepository = applicationRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async ValueTask<Result<CreateApplicationResponse>> Handle(
        CreateApplicationCommand command,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating application with name {ApplicationName}", command.Name);

        var slug = GenerateSlug(command.Name);

        var slugExists = await _applicationRepository.SlugExistsAsync(slug, cancellationToken);
        if (slugExists)
        {
            _logger.LogInformation("Slug {Slug} already exists, generating unique slug", slug);
            slug = $"{slug}-{Guid.NewGuid().ToString()[..8]}";
        }

        var application = App.Create(command.Name, slug);

        await _applicationRepository.AddAsync(application, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Successfully created application {ApplicationId} ({ApplicationName}) with slug {Slug}",
            application.Id.Value, application.Name, application.Slug);

        var response = new CreateApplicationResponse(
            application.Id.Value.ToString(),
            application.Name,
            application.Slug,
            application.PublicKey,
            application.SecretKey,
            application.IsActive,
            application.CreatedAtUtc);

        return Result<CreateApplicationResponse>.Success(response);
    }

    private static string GenerateSlug(string name)
    {
        return System.Text.RegularExpressions.Regex.Replace(
            name
                .ToLowerInvariant()
                .Trim()
                .Replace("_", "-"),
            @"\s+",
            "-");
    }
}