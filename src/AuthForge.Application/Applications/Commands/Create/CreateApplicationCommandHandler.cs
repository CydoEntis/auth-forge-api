using AuthForge.Application.Common.Interfaces;
using AuthForge.Domain.Common;
using AuthForge.Domain.Errors;
using Mediator;
using App = AuthForge.Domain.Entities.Application;

namespace AuthForge.Application.Applications.Commands.Create;

public sealed class CreateApplicationCommandHandler
    : ICommandHandler<CreateApplicationCommand, Result<CreateApplicationResponse>>
{
    private readonly IApplicationRepository _applicationRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateApplicationCommandHandler(
        IApplicationRepository applicationRepository,
        IUnitOfWork unitOfWork)
    {
        _applicationRepository = applicationRepository;
        _unitOfWork = unitOfWork;
    }

    public async ValueTask<Result<CreateApplicationResponse>> Handle(
        CreateApplicationCommand command,
        CancellationToken cancellationToken)
    {
        var slug = GenerateSlug(command.Name);

        var slugExists = await _applicationRepository.SlugExistsAsync(slug, cancellationToken);
        if (slugExists)
        {
            slug = $"{slug}-{Guid.NewGuid().ToString()[..8]}";
        }

        var application = App.Create(command.Name, slug);

        await _applicationRepository.AddAsync(application, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var response = new CreateApplicationResponse(
            application.Id.Value.ToString(),
            application.Name,
            application.Slug,
            application.PublicKey,
            application.SecretKey, 
            application.IsActive,
            application.CreatedAtUtc);

        return Result<CreateApplicationResponse>.Success(response);
    }

    private static string GenerateSlug(string name)
    {
        return System.Text.RegularExpressions.Regex.Replace(
            name
                .ToLowerInvariant()
                .Trim()
                .Replace("_", "-"),
            @"\s+",
            "-");
    }
}