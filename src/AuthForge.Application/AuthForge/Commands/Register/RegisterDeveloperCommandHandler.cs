using AuthForge.Application.Common.Interfaces;
using AuthForge.Application.Common.Services;
using AuthForge.Domain.Common;
using AuthForge.Domain.Entities;
using AuthForge.Domain.Errors;
using Mediator;

namespace AuthForge.Application.AuthForge.Commands.Register;

public sealed class RegisterDeveloperCommandHandler
    : ICommandHandler<RegisterDeveloperCommand, Result<RegisterDeveloperResponse>>
{
    private readonly IAuthForgeUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IEmailParser _emailParser;
    private readonly IUnitOfWork _unitOfWork;

    public RegisterDeveloperCommandHandler(
        IAuthForgeUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IEmailParser emailParser,
        IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _emailParser = emailParser;
        _unitOfWork = unitOfWork;
    }

    public async ValueTask<Result<RegisterDeveloperResponse>> Handle(
        RegisterDeveloperCommand command,
        CancellationToken cancellationToken)
    {
        var emailResult = _emailParser.ParseForAuthentication(command.Email);
        if (emailResult.IsFailure)
            return Result<RegisterDeveloperResponse>.Failure(emailResult.Error);

        var emailExists = await _userRepository.ExistsAsync(emailResult.Value, cancellationToken);
        if (emailExists)
            return Result<RegisterDeveloperResponse>.Failure(AuthForgeUserErrors.DuplicateEmail);

        var hashedPassword = _passwordHasher.HashPassword(command.Password);

        var user = AuthForgeUser.Create(
            emailResult.Value,
            hashedPassword,
            command.FirstName,
            command.LastName);

        await _userRepository.AddAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var response = new RegisterDeveloperResponse(
            user.Id.Value.ToString(),
            user.Email.Value,
            user.FullName,
            "Developer account created successfully. Please verify your email.");

        return Result<RegisterDeveloperResponse>.Success(response);
    }
}