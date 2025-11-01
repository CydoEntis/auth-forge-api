using AuthForge.Domain.Enums;

namespace AuthForge.Application.Common.Models;

public record DatabaseConfiguration(
    DatabaseType DatabaseType,
    string? ConnectionString);