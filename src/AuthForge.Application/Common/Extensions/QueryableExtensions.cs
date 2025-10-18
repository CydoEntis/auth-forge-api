using System.Linq.Expressions;
using AuthForge.Application.Common.Models;

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

    public static IQueryable<T> Paginate<T>(
        this IQueryable<T> query,
        int pageNumber,
        int pageSize)
    {
        return query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize);
    }

    public static IQueryable<T> WhereIf<T>(
        this IQueryable<T> query,
        bool condition,
        Expression<Func<T, bool>> predicate)
    {
        return condition ? query.Where(predicate) : query;
    }

    public static IQueryable<T> OrderByDirection<T, TKey>(
        this IQueryable<T> query,
        Expression<Func<T, TKey>> keySelector,
        SortOrder sortOrder)
    {
        return sortOrder == SortOrder.Asc
            ? query.OrderBy(keySelector)
            : query.OrderByDescending(keySelector);
    }
}