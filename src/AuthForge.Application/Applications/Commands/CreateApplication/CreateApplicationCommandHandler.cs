using AuthForge.Application.Common.Interfaces;
using AuthForge.Domain.Common;
using AuthForge.Domain.ValueObjects;
using Mediator;
using Microsoft.Extensions.Logging;

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

        var application = Domain.Entities.Application.Create(
            command.Name,
            slug,
            command.Description,
            command.AllowedOrigins,
            command.JwtSecret);

        if (command.EmailSettings != null)
        {
            var emailSettings = ApplicationEmailSettings.Create(
                command.EmailSettings.Provider,
                command.EmailSettings.ApiKey,
                command.EmailSettings.FromEmail,
                command.EmailSettings.FromName,
                command.EmailSettings.PasswordResetCallbackUrl,
                command.EmailSettings.EmailVerificationCallbackUrl);

            application.ConfigureEmail(emailSettings);
        }

        if (command.OAuthSettings != null)
        {
            var oauthSettings = OAuthSettings.Create(
                command.OAuthSettings.GoogleEnabled,
                command.OAuthSettings.GoogleClientId,
                command.OAuthSettings.GoogleClientSecret,
                command.OAuthSettings.GithubEnabled,
                command.OAuthSettings.GithubClientId,
                command.OAuthSettings.GithubClientSecret);

            application.ConfigureOAuth(oauthSettings);
        }

        await _applicationRepository.AddAsync(application, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Successfully created application {ApplicationId} ({ApplicationName}) with slug {Slug}",
            application.Id.Value, application.Name, application.Slug);

        var response = new CreateApplicationResponse(
            application.Id.Value.ToString(),
            application.Name,
            application.Slug,
            application.Description,
            application.PublicKey,
            application.SecretKey,
            application.JwtSecret,
            application.AllowedOrigins.ToList(),
            application.IsActive,
            application.CreatedAtUtc);

        return Result<CreateApplicationResponse>.Success(response);
    }

    private static string GenerateSlug(string name)
    {
        return System.Text.RegularExpressions.Regex.Replace(
            name.ToLowerInvariant().Trim().Replace("_", "-"),
            @"\s+",
            "-");
    }
}