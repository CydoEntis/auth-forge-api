namespace AuthForge.Domain.ValueObjects;

public sealed record AuthForgeUserId(Guid Value)
{
    public static AuthForgeUserId CreateUnique() => new(Guid.NewGuid());
    public static AuthForgeUserId Create(Guid value) => new(value);
}