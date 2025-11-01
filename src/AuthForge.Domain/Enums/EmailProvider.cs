using System.Text.Json.Serialization;

namespace AuthForge.Domain.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))] 
public enum EmailProvider
{
    Smtp = 0,
    Resend = 1
}