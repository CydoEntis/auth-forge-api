namespace AuthForge.Domain.Errors;

public class Error : IEquatable<Error>
{
    public static readonly Error None = new(string.Empty, string.Empty);

    public Error(string code, string message)
    {
        Code = code;
        Message = message;
    }

    public string Code { get; }
    public string Message { get; }

    public bool Equals(Error? other)
    {
        if (other is null)
            return false;

        return Code == other.Code && Message == other.Message;
    }

    public override bool Equals(object? obj)
    {
        return obj is Error error && Equals(error);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Code, Message);
    }

    public override string ToString() => $"{Code}: {Message}";

    public static bool operator ==(Error? left, Error? right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(Error? left, Error? right)
    {
        return !Equals(left, right);
    }
}