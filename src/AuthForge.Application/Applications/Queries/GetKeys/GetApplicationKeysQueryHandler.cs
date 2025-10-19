using AuthForge.Application.Common.Interfaces;
using AuthForge.Domain.Common;
using AuthForge.Domain.Errors;
using Mediator;

namespace AuthForge.Application.Applications.Queries.GetKeys;

public sealed class GetApplicationKeysQueryHandler 
    : IQueryHandler<GetApplicationKeysQuery, Result<GetApplicationKeysResponse>>
{
    private readonly IApplicationRepository _applicationRepository;

    public GetApplicationKeysQueryHandler(IApplicationRepository applicationRepository)
    {
        _applicationRepository = applicationRepository;
    }

    public async ValueTask<Result<GetApplicationKeysResponse>> Handle(
        GetApplicationKeysQuery query, 
        CancellationToken cancellationToken)
    {
        var application = await _applicationRepository.GetByIdAsync(query.ApplicationId, cancellationToken);

        if (application == null)
            return Result<GetApplicationKeysResponse>.Failure(ApplicationErrors.NotFound);

        var maskedSecret = MaskSecretKey(application.SecretKey);

        var response = new GetApplicationKeysResponse(
            application.PublicKey,
            maskedSecret,
            application.CreatedAtUtc);

        return Result<GetApplicationKeysResponse>.Success(response);
    }

    private static string MaskSecretKey(string secretKey)
    {
        if (string.IsNullOrEmpty(secretKey) || secretKey.Length < 4)
            return "••••••••••••••••";

        var last4 = secretKey[^4..];
        return $"sk_••••••••••••{last4}";
    }
}