using AuthForge.Application.Common.Interfaces;
using AuthForge.Domain.Events;
using Mediator;

namespace AuthForge.Application.EndUsers.EventHandlers;

public sealed class SendWelcomeEmailOnRegistrationHandler 
    : INotificationHandler<EndUserRegisteredDomainEvent>
{
    private readonly IEndUserRepository _endUserRepository;
    private readonly IEmailServiceFactory _emailServiceFactory;

    public SendWelcomeEmailOnRegistrationHandler(
        IEndUserRepository endUserRepository,
        IEmailServiceFactory emailServiceFactory)
    {
        _endUserRepository = endUserRepository;
        _emailServiceFactory = emailServiceFactory;
    }

    public async ValueTask Handle(
        EndUserRegisteredDomainEvent notification,
        CancellationToken cancellationToken)
    {
        var user = await _endUserRepository.GetByIdAsync(notification.UserId, cancellationToken);
        if (user == null) return;

        var emailService = _emailServiceFactory.CreateForApplication(notification.ApplicationId.Value);
        if (emailService == null) return;

        try
        {
            await emailService.SendWelcomeEmailAsync(
                user.Email.Value,
                user.FullName,
                "App Name", 
                cancellationToken);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to send welcome email: {ex.Message}");
        }
    }
}