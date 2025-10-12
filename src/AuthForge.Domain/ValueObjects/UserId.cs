namespace AuthForge.Domain.ValueObjects;

public sealed record UserId
{
    public Guid Value { get; init; }

    private UserId()
    {
        Value = Guid.Empty;
    }

    private UserId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("UserId cannot be empty", nameof(value));

        Value = value;
    }

    public static UserId CreateUnique() => new(Guid.NewGuid());

    public static UserId Create(Guid value) => new(value);

    public static implicit operator Guid(UserId userId) => userId.Value;

    public override string ToString() => Value.ToString();
}