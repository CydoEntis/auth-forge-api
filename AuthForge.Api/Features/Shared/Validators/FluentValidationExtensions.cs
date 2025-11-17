using FluentValidation;

namespace AuthForge.Api.Features.Applications.Shared.Validators
{
    public static class FluentValidationExtensions
    {
        public static IRuleBuilderOptions<T, string> MustBeValidUrl<T>(this IRuleBuilder<T, string> ruleBuilder)
        {
            return ruleBuilder.Must(url =>
                    Uri.TryCreate(url, UriKind.Absolute, out var uri) &&
                    (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps))
                .WithMessage("Invalid URL");
        }

        public static IRuleBuilderOptions<T, string> MustBeValidGuid<T>(this IRuleBuilder<T, string> ruleBuilder)
        {
            return ruleBuilder.Must(guid => Guid.TryParse(guid, out _))
                .WithMessage("Invalid GUID");
        }
    }
}