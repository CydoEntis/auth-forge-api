using AuthForge.Api.Common;
using AuthForge.Api.Common.Exceptions.Http;
using AuthForge.Api.Common.Interfaces;
using AuthForge.Api.Data;
using AuthForge.Api.Features.Shared.Enums;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace AuthForge.Api.Features.Applications;

public sealed record ListUsersRequest(
    Guid ApplicationId,
    string? Search = null,
    bool? EmailVerified = null,
    UserSortField SortBy = UserSortField.CreatedAt,
    SortOrder SortOrder = SortOrder.Desc,
    int Page = 1,
    int PageSize = 20
);

public sealed record UserListItem(
    Guid Id,
    string Email,
    string? FirstName,
    string? LastName,
    bool EmailVerified,
    int FailedLoginAttempts,
    bool IsLockedOut,
    DateTime? LockedOutUntil,
    DateTime? LastLoginAtUtc,
    DateTime CreatedAtUtc
);

public sealed record ListUsersResponse(
    List<UserListItem> Users,
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages
);

public sealed class ListUsersValidator : AbstractValidator<ListUsersRequest>
{
    public ListUsersValidator()
    {
        RuleFor(x => x.ApplicationId)
            .NotEmpty().WithMessage("Application ID is required");

        RuleFor(x => x.Page)
            .GreaterThan(0).WithMessage("Page must be greater than 0");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100).WithMessage("Page size must be between 1 and 100");
    }
}

public sealed class ListUsersHandler
{
    private readonly AppDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly ILogger<ListUsersHandler> _logger;

    public ListUsersHandler(
        AppDbContext context,
        ICurrentUserService currentUser,
        ILogger<ListUsersHandler> logger)
    {
        _context = context;
        _currentUser = currentUser;
        _logger = logger;
    }

    public async Task<ListUsersResponse> HandleAsync(
        ListUsersRequest request,
        CancellationToken ct)
    {
        var application = await _context.Applications
            .FirstOrDefaultAsync(a => a.Id == request.ApplicationId, ct);

        if (application == null)
        {
            throw new NotFoundException($"Application {request.ApplicationId} not found");
        }

        var query = _context.Users
            .Where(u => u.ApplicationId == request.ApplicationId);

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var searchLower = request.Search.ToLower();
            query = query.Where(u =>
                u.Email.ToLower().Contains(searchLower) ||
                (u.FirstName != null && u.FirstName.ToLower().Contains(searchLower)) ||
                (u.LastName != null && u.LastName.ToLower().Contains(searchLower))
            );
        }

        if (request.EmailVerified.HasValue)
        {
            query = query.Where(u => u.EmailVerified == request.EmailVerified.Value);
        }

        query = ApplySorting(query, request.SortBy, request.SortOrder);

        var totalCount = await query.CountAsync(ct);

        var page = Math.Max(1, request.Page);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);
        var skip = (page - 1) * pageSize;

        var users = await query
            .Skip(skip)
            .Take(pageSize)
            .Select(u => new UserListItem(
                u.Id,
                u.Email,
                u.FirstName,
                u.LastName,
                u.EmailVerified,
                u.FailedLoginAttempts,
                u.LockedOutUntil.HasValue && u.LockedOutUntil > DateTime.UtcNow,
                u.LockedOutUntil,
                u.LastLoginAtUtc,
                u.CreatedAtUtc
            ))
            .ToListAsync(ct);

        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        _logger.LogInformation(
            "Admin {AdminId} listed {Count} users for application {AppId} (Page {Page} of {TotalPages})",
            _currentUser.UserId,
            users.Count,
            request.ApplicationId,
            page,
            totalPages);

        return new ListUsersResponse(
            users,
            totalCount,
            page,
            pageSize,
            totalPages
        );
    }

    private static IQueryable<Entities.User> ApplySorting(
        IQueryable<Entities.User> query,
        UserSortField sortBy,
        SortOrder sortOrder)
    {
        var isDescending = sortOrder == SortOrder.Desc;

        return sortBy switch
        {
            UserSortField.Email => isDescending
                ? query.OrderByDescending(u => u.Email)
                : query.OrderBy(u => u.Email),

            UserSortField.EmailVerified => isDescending
                ? query.OrderByDescending(u => u.EmailVerified)
                : query.OrderBy(u => u.EmailVerified),

            UserSortField.LastLogin => isDescending
                ? query.OrderByDescending(u => u.LastLoginAtUtc)
                : query.OrderBy(u => u.LastLoginAtUtc),

            UserSortField.CreatedAt or _ => isDescending
                ? query.OrderByDescending(u => u.CreatedAtUtc)
                : query.OrderBy(u => u.CreatedAtUtc),
        };
    }
}

public static class ListUsers
{
    public static void MapEndpoints(WebApplication app, string prefix = "/api/v1")
    {
        app.MapGet($"{prefix}/applications/{{appId:guid}}/users", async (
                Guid appId,
                [AsParameters] ListUsersRequest request,
                ListUsersHandler handler,
                CancellationToken ct) =>
            {
                var requestWithAppId = request with { ApplicationId = appId };

                var validator = new ListUsersValidator();
                var validationResult = await validator.ValidateAsync(requestWithAppId, ct);

                if (!validationResult.IsValid)
                    throw new ValidationException(validationResult.Errors);

                var response = await handler.HandleAsync(requestWithAppId, ct);
                return Results.Ok(ApiResponse<ListUsersResponse>.Ok(response));
            })
            .WithName("ListUsers")
            .WithTags("User Management")
            .RequireAuthorization("Admin");
    }
}