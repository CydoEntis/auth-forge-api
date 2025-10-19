using AuthForge.Application.Common.Interfaces;
using AuthForge.Domain.ValueObjects;

namespace AuthForge.Infrastructure.EmailProviders;

public class EmailServiceFactory
{
    private readonly IHttpClientFactory _httpClientFactory;

    public EmailServiceFactory(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public IEmailService? Create(ApplicationEmailSettings? settings)
    {
        if (settings == null)
            return null;

        //TODO: In the future I will add other email providers
        return settings.Provider switch
        {
            EmailProvider.Resend => new ResendEmailService(
                _httpClientFactory.CreateClient(),
                settings.ApiKey,
                settings.FromEmail,
                settings.FromName),
            
            _ => throw new NotSupportedException($"Email provider {settings.Provider} is not supported")
        };
    }
}