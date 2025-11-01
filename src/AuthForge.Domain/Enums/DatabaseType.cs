using System.Text.Json.Serialization;

namespace AuthForge.Domain.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum DatabaseType
{
    Sqlite = 0,
    PostgreSql = 1
}