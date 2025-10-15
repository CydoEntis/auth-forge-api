namespace AuthForge.Application.Common.Extensions;

public static class QueryableExtensions
{
    public static List<T> Paginate<T>(
        this List<T> items,
        int pageNumber,
        int pageSize)
    {
        return items
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();
    }
}