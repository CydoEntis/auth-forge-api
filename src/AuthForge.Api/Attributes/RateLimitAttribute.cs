namespace AuthForge.Api.Attributes;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class RateLimitAttribute : Attribute
{
    public int Requests { get; }
    public int WindowMinutes { get; }

    public RateLimitAttribute(int requests, int windowMinutes)
    {
        if (requests <= 0)
            throw new ArgumentException("Requests must be greater than zero", nameof(requests));

        if (windowMinutes <= 0)
            throw new ArgumentException("Window Minutes must be greater than zero", nameof(windowMinutes));
        
        Requests = requests;
        WindowMinutes = windowMinutes;
    }
}