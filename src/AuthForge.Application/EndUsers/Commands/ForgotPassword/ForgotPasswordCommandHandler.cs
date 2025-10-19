using AuthForge.Application.Common.Interfaces;
using AuthForge.Domain.Common;
using Mediator;

namespace AuthForge.Application.EndUsers.Commands.ForgotPassword;

public sealed class ForgotPasswordCommandHandler
    : ICommandHandler<ForgotPasswordCommand, Result<ForgotPasswordResponse>>
{
    private readonly IEndUserRepository _endUserRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ForgotPasswordCommandHandler(
        IEndUserRepository endUserRepository,
        IUnitOfWork unitOfWork)
    {
        _endUserRepository = endUserRepository;
        _unitOfWork = unitOfWork;
    }

    public async ValueTask<Result<ForgotPasswordResponse>> Handle(
        ForgotPasswordCommand command,
        CancellationToken cancellationToken)
    {
        Console.WriteLine($"=== FORGOT PASSWORD DEBUG ===");
        Console.WriteLine($"ApplicationId: {command.ApplicationId}");
        Console.WriteLine($"Email: {command.Email}");

        var user = await _endUserRepository.GetByEmailAsync(
            command.ApplicationId,
            command.Email,
            cancellationToken);

        Console.WriteLine($"User found: {user != null}");

        if (user == null)
        {
            Console.WriteLine("=== USER IS NULL - RETURNING EARLY ===");
            return Result<ForgotPasswordResponse>.Success(
                new ForgotPasswordResponse("If an account exists, a password reset email has been sent."));
        }

        Console.WriteLine("=== USER FOUND - CONTINUING ===");

        var resetToken = GenerateResetToken();
        var expiresAt = DateTime.UtcNow.AddHours(1);

        Console.WriteLine($"=== ABOUT TO CALL SetPasswordResetToken ===");
        user.SetPasswordResetToken(resetToken, expiresAt);
        Console.WriteLine($"=== CALLED SetPasswordResetToken ===");

        _endUserRepository.Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<ForgotPasswordResponse>.Success(
            new ForgotPasswordResponse("If an account exists, a password reset email has been sent."));
    }

    private static string GenerateResetToken()
    {
        return Convert.ToBase64String(Guid.NewGuid().ToByteArray())
            .Replace("/", "_")
            .Replace("+", "-")
            .TrimEnd('=');
    }
}