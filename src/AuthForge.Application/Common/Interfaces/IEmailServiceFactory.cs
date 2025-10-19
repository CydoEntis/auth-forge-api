namespace AuthForge.Application.Common.Interfaces;

public interface IEmailServiceFactory
{
    IEmailService? CreateForApplication(Guid applicationId);
}