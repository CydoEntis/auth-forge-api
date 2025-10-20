namespace AuthForge.Application.Applications.Queries.GetAllApplications;

public sealed record ApplicationQueryParameters
{
    public string? Search { get; init; }
    public bool? IsActive { get; init; }
    public string SortBy { get; init; } = "createdAt";
    public string SortOrder { get; init; } = "desc";
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 10;
}