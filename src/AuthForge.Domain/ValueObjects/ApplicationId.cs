namespace AuthForge.Domain.ValueObjects;

public sealed record ApplicationId(Guid Value)
{
    public static ApplicationId CreateUnique() => new(Guid.NewGuid());
    public static ApplicationId Create(Guid value) => new(value);
}