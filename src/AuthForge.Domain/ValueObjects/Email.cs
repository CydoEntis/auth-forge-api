namespace AuthForge.Domain.ValueObjects;

public sealed record Email
{
    public string Value { get; }

    private Email(string value)
    {
        Value = value;
    }

    public static Email Create(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email cannot be empty", nameof(email));

        email = email.Trim().ToLowerInvariant();
        
        if(!email.Contains("@")) 
            throw new ArgumentException("Email must contain @", nameof(email)); 
        
        if(!email.Contains('.')) 
            throw new ArgumentException("Email must contain a domain", nameof(email));

        return new Email(email);
    }
    
    public static implicit operator string(Email email) => email.Value;
    
    public override string ToString() => Value; 
}