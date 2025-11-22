using System.Text.Json.Serialization;

namespace AuthForge.Api.Features.Applications.Shared.Models;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ApplicationSortField
{
    Name,
    Slug,
    CreatedAt,
    UpdatedAt,
    IsActive
}