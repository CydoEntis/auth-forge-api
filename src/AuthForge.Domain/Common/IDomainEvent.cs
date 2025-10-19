using Mediator;

namespace AuthForge.Domain.Common;

public interface IDomainEvent : INotification
{
    DateTime OccurredOnUtc => DateTime.UtcNow;
}