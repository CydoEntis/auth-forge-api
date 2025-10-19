using AuthForge.Application.Common.Interfaces;
using AuthForge.Domain.ValueObjects;
using ApplicationId = AuthForge.Domain.ValueObjects.ApplicationId;

namespace AuthForge.Infrastructure.EmailProviders;

public class EmailServiceFactory : IEmailServiceFactory
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IApplicationRepository _applicationRepository;

    public EmailServiceFactory(
        IHttpClientFactory httpClientFactory,
        IApplicationRepository applicationRepository)
    {
        _httpClientFactory = httpClientFactory;
        _applicationRepository = applicationRepository;
    }

    public IEmailService? CreateForApplication(Guid applicationId)
    {
        var application = _applicationRepository.GetByIdAsync(
            ApplicationId.Create(applicationId),
            CancellationToken.None).Result;

        if (application?.ApplicationEmailSettings == null)
            return null;

        return application.ApplicationEmailSettings.Provider switch
        {
            EmailProvider.Resend => new ResendEmailService(
                _httpClientFactory.CreateClient(),
                application.ApplicationEmailSettings.ApiKey,
                application.ApplicationEmailSettings.FromEmail,
                application.ApplicationEmailSettings.FromName),
            
            _ => throw new NotSupportedException($"Email provider {application.ApplicationEmailSettings.Provider} is not supported")
        };
    }
}