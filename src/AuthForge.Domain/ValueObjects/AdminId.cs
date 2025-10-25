namespace AuthForge.Domain.ValueObjects;

public sealed record AdminId(Guid Value) 
{
    public static AdminId CreateUnique() => new(Guid.NewGuid());
    public static AdminId Create(Guid value) => new(value);
}