namespace AuthForge.Domain.Common;

public interface IDomainEvent
{
    DateTime OccurredOnUtc => DateTime.UtcNow;
}