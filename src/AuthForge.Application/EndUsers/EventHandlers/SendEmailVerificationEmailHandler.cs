using AuthForge.Application.Common.Interfaces;
using AuthForge.Domain.Events;
using Mediator;

namespace AuthForge.Application.EndUsers.EventHandlers;

public sealed class SendEmailVerificationEmailHandler
    : INotificationHandler<EndUserEmailVerificationRequestedDomainEvent>
{
    private readonly IEndUserRepository _endUserRepository;
    private readonly IApplicationRepository _applicationRepository;
    private readonly IEmailServiceFactory _emailServiceFactory;

    public SendEmailVerificationEmailHandler(
        IEndUserRepository endUserRepository,
        IApplicationRepository applicationRepository,
        IEmailServiceFactory emailServiceFactory)
    {
        _endUserRepository = endUserRepository;
        _applicationRepository = applicationRepository;
        _emailServiceFactory = emailServiceFactory;
    }

    public async ValueTask Handle(
        EndUserEmailVerificationRequestedDomainEvent notification,
        CancellationToken cancellationToken)
    {
        var user = await _endUserRepository.GetByIdAsync(notification.UserId, cancellationToken);
        if (user == null || user.EmailVerificationToken == null) return;

        var application = await _applicationRepository.GetByIdAsync(user.ApplicationId, cancellationToken);
        if (application == null) return;

        var emailService = _emailServiceFactory.CreateForApplication(user.ApplicationId.Value);
        if (emailService == null) return;

        try
        {
            await emailService.SendEmailVerificationEmailAsync(
                user.Email.Value,
                user.FullName,
                user.EmailVerificationToken,
                application.Name,
                cancellationToken);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to send email verification email: {ex.Message}");
        }
    }
}