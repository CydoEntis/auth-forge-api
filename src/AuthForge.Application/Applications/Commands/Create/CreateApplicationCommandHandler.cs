using AuthForge.Application.Common.Interfaces;
using AuthForge.Domain.Common;
using AuthForge.Domain.Errors;
using AuthForge.Domain.ValueObjects;
using Mediator;

using App = AuthForge.Domain.Entities.Application;

namespace AuthForge.Application.Applications.Commands.Create;

public sealed class CreateApplicationCommandHandler 
    : ICommandHandler<CreateApplicationCommand, Result<CreateApplicationResponse>>
{
    private readonly IApplicationRepository _applicationRepository;
    private readonly IAuthForgeUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateApplicationCommandHandler(
        IApplicationRepository applicationRepository,
        IAuthForgeUserRepository userRepository,
        IUnitOfWork unitOfWork)
    {
        _applicationRepository = applicationRepository;
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
    }

    public async ValueTask<Result<CreateApplicationResponse>> Handle(
        CreateApplicationCommand command,
        CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(command.UserId, out var userGuid))
            return Result<CreateApplicationResponse>.Failure(ValidationErrors.InvalidGuid("UserId"));

        var userId = AuthForgeUserId.Create(userGuid);
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user is null)
            return Result<CreateApplicationResponse>.Failure(AuthForgeUserErrors.NotFound);

        if (!user.IsActive)
            return Result<CreateApplicationResponse>.Failure(AuthForgeUserErrors.Inactive);

        var slug = GenerateSlug(command.Name);

        var slugExists = await _applicationRepository.SlugExistsAsync(slug, cancellationToken);
        if (slugExists)
        {
            slug = $"{slug}-{Guid.NewGuid().ToString()[..8]}";
        }

        var application = App.Create(userId, command.Name, slug);

        await _applicationRepository.AddAsync(application, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var response = new CreateApplicationResponse(
            application.Id.Value.ToString(),
            application.Name,
            application.Slug,
            application.IsActive,
            application.CreatedAtUtc);

        return Result<CreateApplicationResponse>.Success(response);
    }

    private static string GenerateSlug(string name)
    {
        return name
            .ToLowerInvariant()
            .Trim()
            .Replace(" ", "-")
            .Replace("_", "-");
    }
}