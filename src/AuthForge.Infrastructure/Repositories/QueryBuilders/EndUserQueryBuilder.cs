using AuthForge.Application.Common.Extensions;
using AuthForge.Application.Common.Models;
using AuthForge.Application.EndUsers.Enums;
using AuthForge.Domain.Entities;

namespace AuthForge.Infrastructure.Repositories.QueryBuilders;

public static class EndUserQueryBuilder
{
    public static IQueryable<EndUser> ApplyFilters(
        this IQueryable<EndUser> query,
        string? searchTerm,
        bool? isActive,
        bool? isEmailVerified)
    {
        query = query.WhereIf(
            !string.IsNullOrWhiteSpace(searchTerm),
            u => u.Email.Value.Contains(searchTerm!) ||
                 u.FirstName.Contains(searchTerm!) ||
                 u.LastName.Contains(searchTerm!));

        query = query.WhereIf(
            isActive.HasValue,
            u => u.IsActive == isActive!.Value);

        query = query.WhereIf(
            isEmailVerified.HasValue,
            u => u.IsEmailVerified == isEmailVerified!.Value);

        return query;
    }

    public static IQueryable<EndUser> ApplySorting(
        this IQueryable<EndUser> query,
        EndUserSortBy sortBy,
        SortOrder sortOrder)
    {
        return sortBy switch
        {
            EndUserSortBy.Email => query.OrderByDirection(u => u.Email.Value, sortOrder),
            EndUserSortBy.FirstName => query.OrderByDirection(u => u.FirstName, sortOrder),
            EndUserSortBy.LastName => query.OrderByDirection(u => u.LastName, sortOrder),
            EndUserSortBy.LastLoginAt => query.OrderByDirection(u => u.LastLoginAtUtc, sortOrder),
            _ => query.OrderByDirection(u => u.CreatedAtUtc, sortOrder)
        };
    }
}