using FluentValidation;

namespace AuthForge.Application.Applications.Queries.GetById;

public sealed class GetApplicationByIdQueryValidator : AbstractValidator<GetApplicationByIdQuery>
{
    public GetApplicationByIdQueryValidator()
    {
        RuleFor(x => x.ApplicationId)
            .NotEmpty();

        RuleFor(x => x.UserId)
            .NotEmpty();
    }
}