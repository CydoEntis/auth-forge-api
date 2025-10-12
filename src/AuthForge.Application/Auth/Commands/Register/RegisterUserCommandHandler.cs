using AuthForge.Application.Common.Interfaces;
using AuthForge.Application.Common.Services;
using AuthForge.Domain.Common;
using AuthForge.Domain.Entities;
using AuthForge.Domain.Errors;
using AuthForge.Domain.ValueObjects;
using Mediator;

namespace AuthForge.Application.Auth.Commands.Register;

public sealed class RegisterUserCommandHandler : ICommandHandler<RegisterUserCommand, Result<RegisterUserResponse>>
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantValidationService _tenantValidationService;
    private readonly IEmailParser _emailParser;

    public RegisterUserCommandHandler(
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        ITenantValidationService tenantValidationService,
        IEmailParser emailParser)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _tenantValidationService = tenantValidationService;
        _emailParser = emailParser;
    }

    public async ValueTask<Result<RegisterUserResponse>> Handle(
        RegisterUserCommand command,
        CancellationToken cancellationToken)
    {
        var tenantResult = await _tenantValidationService.ValidateTenantAsync(
            command.TenantId,
            cancellationToken);
        if (tenantResult.IsFailure)
            return Result<RegisterUserResponse>.Failure(tenantResult.Error);

        var emailResult = _emailParser.ParseForRegistration(command.Email);
        if (emailResult.IsFailure)
            return Result<RegisterUserResponse>.Failure(emailResult.Error);

        var uniquenessResult = await EnsureEmailIsUniqueAsync(
            tenantResult.Value.Id,
            emailResult.Value,
            cancellationToken);
        if (uniquenessResult.IsFailure)
            return Result<RegisterUserResponse>.Failure(uniquenessResult.Error);

        var passwordResult = CreateHashedPassword(command.Password);
        if (passwordResult.IsFailure)
            return Result<RegisterUserResponse>.Failure(passwordResult.Error);

        var user = CreateUserWithVerificationToken(
            tenantResult.Value.Id,
            emailResult.Value,
            passwordResult.Value,
            command.FirstName,
            command.LastName);

        await SaveUserAsync(user, cancellationToken);

        return CreateSuccessResponse(user);
    }

    private async Task<Result> EnsureEmailIsUniqueAsync(
        TenantId tenantId,
        Email email,
        CancellationToken cancellationToken)
    {
        var emailExists = await _userRepository.ExistsAsync(
            tenantId,
            email,
            cancellationToken);

        if (emailExists)
        {
            return Result.Failure(DomainErrors.User.EmailAlreadyExists);
        }

        return Result.Success();
    }

    private static Result<HashedPassword> CreateHashedPassword(string password)
    {
        try
        {
            var hashedPassword = HashedPassword.Create(password);
            return Result<HashedPassword>.Success(hashedPassword);
        }
        catch (ArgumentException)
        {
            return Result<HashedPassword>.Failure(
                DomainErrors.Validation.InvalidPassword());
        }
    }

    private static User CreateUserWithVerificationToken(
        TenantId tenantId,
        Email email,
        HashedPassword password,
        string firstName,
        string lastName)
    {
        var user = User.Create(
            tenantId,
            email,
            password,
            firstName,
            lastName);

        var verificationToken = GenerateVerificationToken();
        var tokenExpiration = DateTime.UtcNow.AddHours(24);
        user.SetEmailVerificationToken(verificationToken, tokenExpiration);

        return user;
    }

    private async Task SaveUserAsync(User user, CancellationToken cancellationToken)
    {
        await _userRepository.AddAsync(user, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private static Result<RegisterUserResponse> CreateSuccessResponse(User user)
    {
        var response = new RegisterUserResponse
        {
            UserId = user.Id.Value.ToString(),
            Email = user.Email.Value,
            FullName = user.FullName,
            IsEmailVerified = user.IsEmailVerified,
            Message = "Registration successful. Please check your email to verify your account."
        };

        return Result<RegisterUserResponse>.Success(response);
    }

    private static string GenerateVerificationToken()
    {
        return Convert.ToBase64String(
            System.Security.Cryptography.RandomNumberGenerator.GetBytes(32));
    }
}