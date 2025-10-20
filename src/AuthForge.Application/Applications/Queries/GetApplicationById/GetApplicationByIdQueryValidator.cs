using FluentValidation;

namespace AuthForge.Application.Applications.Queries.GetApplicationById;

public sealed class GetApplicationByIdQueryValidator : AbstractValidator<GetApplicationByIdQuery>
{
    public GetApplicationByIdQueryValidator()
    {
        RuleFor(x => x.ApplicationId)
            .NotEmpty();

    }
}