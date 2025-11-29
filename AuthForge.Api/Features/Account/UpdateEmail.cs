using AuthForge.Api.Common;
using AuthForge.Api.Common.Exceptions.Http;
using AuthForge.Api.Common.Interfaces;
using AuthForge.Api.Data;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace AuthForge.Api.Features.Account;

public sealed record UpdateEmailRequest(string Email);

public sealed record UpdateEmailResponse(string Message);

public class UpdateEmailValidator : AbstractValidator<UpdateEmailRequest>
{
    public UpdateEmailValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("Email is required")
            .EmailAddress()
            .WithMessage("Invalid email format");
    }
}

public class UpdateEmailHandler
{
    private readonly AppDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly ILogger<UpdateEmailHandler> _logger;

    public UpdateEmailHandler(
        AppDbContext context,
        ICurrentUserService currentUser,
        ILogger<UpdateEmailHandler> logger)
    {
        _context = context;
        _currentUser = currentUser;
        _logger = logger;
    }

    public async Task<UpdateEmailResponse> HandleAsync(
        UpdateEmailRequest request,
        CancellationToken ct)
    {
        if (!Guid.TryParse(_currentUser.UserId, out var adminId))
        {
            throw new UnauthorizedException("Invalid user ID");
        }

        var admin = await _context.Admins.FindAsync(new object[] { adminId }, ct);

        if (admin == null)
            throw new NotFoundException("Account not found");

        var emailExists = await _context.Admins
            .AnyAsync(a => a.Email == request.Email && a.Id != adminId, ct);

        if (emailExists)
            throw new ConflictException("Email already in use");

        admin.Email = request.Email;
        admin.UpdatedAtUtc = DateTime.UtcNow;

        await _context.SaveChangesAsync(ct);

        _logger.LogInformation("Account email updated: {AdminId}", adminId);

        return new UpdateEmailResponse("Email updated successfully");
    }
}

public static class UpdateEmail
{
    public static void MapEndpoints(WebApplication app, string prefix = "/api/v1")
    {
        app.MapPut($"{prefix}/email", async (
                UpdateEmailRequest request,
                UpdateEmailHandler handler,
                CancellationToken ct) =>
            {
                var validator = new UpdateEmailValidator();
                var validationResult = await validator.ValidateAsync(request, ct);

                if (!validationResult.IsValid)
                    throw new ValidationException(validationResult.Errors);

                var response = await handler.HandleAsync(request, ct);
                return Results.Ok(ApiResponse<UpdateEmailResponse>.Ok(response));
            })
            .WithName("UpdateEmail")
            .WithTags("Account")
            .RequireAuthorization();
    }
}