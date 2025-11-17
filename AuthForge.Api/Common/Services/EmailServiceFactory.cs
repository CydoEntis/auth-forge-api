using AuthForge.Api.Common.Interfaces;
using AuthForge.Api.Entities;
using AuthForge.Providers.Email;
using AuthForge.Providers.Interfaces;
using Resend;

namespace AuthForge.Api.Common.Services;

public class EmailServiceFactory : IEmailServiceFactory
{
    private readonly IConfigurationService _configService;
    private readonly ILoggerFactory _loggerFactory;
    private readonly IEncryptionService _encryptionService;

    public EmailServiceFactory(
        IConfigurationService configService,
        ILoggerFactory loggerFactory, IEncryptionService encryptionService)
    {
        _configService = configService;
        _loggerFactory = loggerFactory;
        _encryptionService = encryptionService;
    }

    public async Task<IEmailService> CreateAsync(CancellationToken ct = default)
    {
        var config = await _configService.GetAsync(ct);

        if (config == null || !config.IsSetupComplete)
        {
            var logger = _loggerFactory.CreateLogger<EmailServiceFactory>();
            logger.LogWarning("Email service requested but setup is not complete");
            return new NoOpEmailService(_loggerFactory.CreateLogger<NoOpEmailService>());
        }

        var provider = config.EmailProvider?.ToLower();

        return provider switch
        {
            "smtp" => CreateSmtpService(config),
            "resend" => CreateResendService(config),
            _ => throw new InvalidOperationException($"Unknown email provider: {provider}")
        };
    }

    public async Task<string> GetFromAddressAsync(CancellationToken ct = default)
    {
        var config = await _configService.GetAsync(ct);

        if (string.IsNullOrEmpty(config?.EmailFromAddress))
        {
            throw new InvalidOperationException("Email from address is not configured");
        }

        return config.EmailFromAddress;
    }

    public async Task<string?> GetFromNameAsync(CancellationToken ct = default)
    {
        var config = await _configService.GetAsync(ct);
        return config?.EmailFromName;
    }

    private IEmailService CreateSmtpService(Configuration config)
    {
        if (string.IsNullOrEmpty(config.SmtpHost))
        {
            throw new InvalidOperationException("SMTP host is not configured");
        }

        var decryptedPassword = _encryptionService.Decrypt(config.SmtpPasswordEncrypted ?? "");

        return new SmtpEmailService(
            host: config.SmtpHost,
            port: config.SmtpPort ?? 587,
            username: config.SmtpUsername ?? "",
            password: decryptedPassword,
            enableSsl: config.SmtpUseSsl,
            logger: _loggerFactory.CreateLogger<SmtpEmailService>()
        );
    }

    private IEmailService CreateResendService(Configuration config)
    {
        if (string.IsNullOrEmpty(config.ResendApiKeyEncrypted))
        {
            throw new InvalidOperationException("Resend API key is not configured");
        }

        var decryptedResendApiKey = _encryptionService.Decrypt(config.ResendApiKeyEncrypted ?? "");
        
        var resendClient = ResendClient.Create(decryptedResendApiKey);

        return new ResendEmailService(
            resendClient,
            _loggerFactory.CreateLogger<ResendEmailService>()
        );
    }
}