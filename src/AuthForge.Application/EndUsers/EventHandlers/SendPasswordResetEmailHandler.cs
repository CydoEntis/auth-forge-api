using AuthForge.Application.Common.Interfaces;
using AuthForge.Domain.Events;
using Mediator;

namespace AuthForge.Application.EndUsers.EventHandlers;

public sealed class SendPasswordResetEmailHandler 
    : INotificationHandler<EndUserPasswordResetRequestedDomainEvent>
{
    private readonly IEndUserRepository _endUserRepository;
    private readonly IApplicationRepository _applicationRepository;
    private readonly IEmailServiceFactory _emailServiceFactory;

    public SendPasswordResetEmailHandler(
        IEndUserRepository endUserRepository,
        IApplicationRepository applicationRepository,
        IEmailServiceFactory emailServiceFactory)
    {
        _endUserRepository = endUserRepository;
        _applicationRepository = applicationRepository;
        _emailServiceFactory = emailServiceFactory;
    }

    public async ValueTask Handle(
        EndUserPasswordResetRequestedDomainEvent notification,
        CancellationToken cancellationToken)
    {
        var user = await _endUserRepository.GetByIdAsync(notification.UserId, cancellationToken);
        if (user == null || user.PasswordResetToken == null) return;

        var application = await _applicationRepository.GetByIdAsync(user.ApplicationId, cancellationToken);
        if (application == null) return;

        var emailService = _emailServiceFactory.CreateForApplication(user.ApplicationId.Value);
        if (emailService == null) return;

        try
        {
            await emailService.SendPasswordResetEmailAsync(
                user.Email.Value,
                user.FullName,
                user.PasswordResetToken,
                application.Name,
                cancellationToken);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to send password reset email: {ex.Message}");
        }
    }
}