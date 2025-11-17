using AuthForge.Api.Common;
using AuthForge.Api.Common.Exceptions.Http;
using AuthForge.Api.Common.Interfaces;
using AuthForge.Api.Data;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace AuthForge.Api.Features.Admin;

public sealed record AdminUpdateEmailRequest(string Email);

public sealed record AdminUpdateEmailResponse(string Message);

public class AdminUpdateEmailValidator : AbstractValidator<AdminUpdateEmailRequest>
{
    public AdminUpdateEmailValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("Email is required")
            .EmailAddress()
            .WithMessage("Invalid email format");
    }
}

public class AdminUpdateEmailHandler
{
    private readonly AppDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly ILogger<AdminUpdateEmailHandler> _logger;

    public AdminUpdateEmailHandler(
        AppDbContext context,
        ICurrentUserService currentUser,
        ILogger<AdminUpdateEmailHandler> logger)
    {
        _context = context;
        _currentUser = currentUser;
        _logger = logger;
    }

    public async Task<AdminUpdateEmailResponse> HandleAsync(
        AdminUpdateEmailRequest request,
        CancellationToken ct)
    {
        if (!Guid.TryParse(_currentUser.UserId, out var adminId))
        {
            throw new UnauthorizedException("Invalid user ID");
        }

        var admin = await _context.Admins.FindAsync(new object[] { adminId }, ct);

        if (admin == null)
            throw new NotFoundException("Admin not found");

        var emailExists = await _context.Admins
            .AnyAsync(a => a.Email == request.Email && a.Id != adminId, ct);

        if (emailExists)
            throw new ConflictException("Email already in use");

        admin.Email = request.Email;
        admin.UpdatedAtUtc = DateTime.UtcNow;

        await _context.SaveChangesAsync(ct);

        _logger.LogInformation("Admin email updated: {AdminId}", adminId);

        return new AdminUpdateEmailResponse("Email updated successfully");
    }
}

public static class AdminUpdateEmail
{
    public static void MapEndpoints(WebApplication app, string prefix = "/api/v1")
    {
        app.MapPut($"{prefix}/admin/email", async (
                AdminUpdateEmailRequest request,
                AdminUpdateEmailHandler handler,
                CancellationToken ct) =>
            {
                var validator = new AdminUpdateEmailValidator();
                var validationResult = await validator.ValidateAsync(request, ct);

                if (!validationResult.IsValid)
                    throw new ValidationException(validationResult.Errors);

                var response = await handler.HandleAsync(request, ct);
                return Results.Ok(ApiResponse<AdminUpdateEmailResponse>.Ok(response));
            })
            .WithName("AdminUpdateEmail")
            .WithTags("Admin")
            .RequireAuthorization();
    }
}