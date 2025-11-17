using System.Text.Json.Serialization;

namespace AuthForge.Api.Features.Shared.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum EmailProvider
{
    Smtp,
    Resend
}