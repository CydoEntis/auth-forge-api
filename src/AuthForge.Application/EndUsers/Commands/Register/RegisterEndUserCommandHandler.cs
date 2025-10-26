using AuthForge.Application.Common.Interfaces;
using AuthForge.Application.Common.Services;
using AuthForge.Domain.Common;
using AuthForge.Domain.Entities;
using AuthForge.Domain.Errors;
using AuthForge.Domain.ValueObjects;
using Mediator;
using Microsoft.Extensions.Logging;
using ApplicationId = AuthForge.Domain.ValueObjects.ApplicationId;

namespace AuthForge.Application.EndUsers.Commands.Register;

public sealed class RegisterEndUserCommandHandler
    : ICommandHandler<RegisterEndUserCommand, Result<RegisterEndUserResponse>>
{
    private readonly IEndUserRepository _endUserRepository;
    private readonly IApplicationRepository _applicationRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IEmailParser _emailParser;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<RegisterEndUserCommandHandler> _logger;

    public RegisterEndUserCommandHandler(
        IEndUserRepository endUserRepository,
        IApplicationRepository applicationRepository,
        IPasswordHasher passwordHasher,
        IEmailParser emailParser,
        IUnitOfWork unitOfWork,
        ILogger<RegisterEndUserCommandHandler> logger)
    {
        _endUserRepository = endUserRepository;
        _applicationRepository = applicationRepository;
        _passwordHasher = passwordHasher;
        _emailParser = emailParser;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async ValueTask<Result<RegisterEndUserResponse>> Handle(
        RegisterEndUserCommand command,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Registration attempt for email {Email}", command.Email);

        if (!Guid.TryParse(command.ApplicationId, out var appGuid))
        {
            _logger.LogWarning("Registration failed - Invalid ApplicationId format: {ApplicationId}", command.ApplicationId);
            return Result<RegisterEndUserResponse>.Failure(ValidationErrors.InvalidGuid("ApplicationId"));
        }

        var applicationId = ApplicationId.Create(appGuid);
        var application = await _applicationRepository.GetByIdAsync(applicationId, cancellationToken);
        if (application is null)
        {
            _logger.LogWarning("Registration failed - Application not found: {ApplicationId}", applicationId);
            return Result<RegisterEndUserResponse>.Failure(ApplicationErrors.NotFound);
        }

        if (!application.IsActive)
        {
            _logger.LogWarning("Registration failed - Application inactive: {ApplicationName} ({ApplicationId})",
                application.Name, applicationId);
            return Result<RegisterEndUserResponse>.Failure(ApplicationErrors.Inactive);
        }

        var emailResult = _emailParser.ParseForAuthentication(command.Email);
        if (emailResult.IsFailure)
        {
            _logger.LogWarning("Registration failed - Invalid email format: {Email}", command.Email);
            return Result<RegisterEndUserResponse>.Failure(emailResult.Error);
        }

        var emailExists = await _endUserRepository.ExistsAsync(
            applicationId,
            emailResult.Value,
            cancellationToken);
        if (emailExists)
        {
            _logger.LogWarning("Registration failed - Email already exists: {Email} in application {ApplicationName}",
                command.Email, application.Name);
            return Result<RegisterEndUserResponse>.Failure(EndUserErrors.DuplicateEmail);
        }

        var hashedPassword = _passwordHasher.HashPassword(command.Password);

        var user = EndUser.Create(
            applicationId,
            emailResult.Value,
            hashedPassword,
            command.FirstName,
            command.LastName);

        await _endUserRepository.AddAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("User successfully registered: {UserId} ({Email}) in application {ApplicationName}",
            user.Id, user.Email.Value, application.Name);

        var response = new RegisterEndUserResponse(
            user.Id.Value.ToString(),
            user.Email.Value,
            user.FullName,
            "User registered successfully. Please verify your email.");

        return Result<RegisterEndUserResponse>.Success(response);
    }
}