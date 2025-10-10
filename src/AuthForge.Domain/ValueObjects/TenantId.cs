namespace AuthForge.Domain.ValueObjects;

public sealed record TenantId
{
    public Guid Value { get; }

    private TenantId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("TenantId cannot be empty", nameof(value));

        Value = value;
    }

    public static TenantId CreateUnique() => new(Guid.NewGuid());

    public static TenantId Create(Guid value) => new(value);
    
    public static implicit operator Guid(TenantId tenantId) => tenantId.Value;
    
    public override string ToString() => Value.ToString();
}