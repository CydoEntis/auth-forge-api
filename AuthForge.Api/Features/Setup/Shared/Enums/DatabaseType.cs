using System.Text.Json.Serialization;

namespace AuthForge.Api.Features.Setup.Shared.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum DatabaseType
{
    Sqlite,
    PostgreSql,
    SqlServer,
    MySql
}