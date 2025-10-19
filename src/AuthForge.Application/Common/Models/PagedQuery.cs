namespace AuthForge.Application.Common.Models;

public abstract record PagedQuery
{
    public int? PageNumber { get; init; }
    public int? PageSize { get; init; }
    public string? SearchTerm { get; init; }
}