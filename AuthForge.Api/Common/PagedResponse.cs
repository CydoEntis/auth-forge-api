namespace AuthForge.Api.Common;

public sealed record PagedResponse<T>(
    List<T> Items,
    int TotalCount,
    int PageNumber,
    int PageSize,
    int TotalPages,
    bool HasNextPage,
    bool HasPreviousPage
)
{
    public static PagedResponse<T> Create(
        List<T> items,
        int totalCount,
        int pageNumber,
        int pageSize)
    {
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        return new PagedResponse<T>(
            Items: items,
            TotalCount: totalCount,
            PageNumber: pageNumber,
            PageSize: pageSize,
            TotalPages: totalPages,
            HasNextPage: pageNumber < totalPages,
            HasPreviousPage: pageNumber > 1
        );
    }
}