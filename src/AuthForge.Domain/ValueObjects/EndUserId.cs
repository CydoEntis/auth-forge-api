namespace AuthForge.Domain.ValueObjects;

public sealed record EndUserId(Guid Value)
{
    public static EndUserId CreateUnique() => new(Guid.NewGuid());
    public static EndUserId Create(Guid value) => new(value);
}