using FluentValidation;

namespace AuthForge.Application.Applications.Queries.GetMy;

public sealed class GetMyApplicationsQueryValidator : AbstractValidator<GetMyApplicationsQuery>
{
    public GetMyApplicationsQueryValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty();

        RuleFor(x => x.PageNumber)
            .GreaterThan(0);

        RuleFor(x => x.PageSize)
            .GreaterThan(0)
            .LessThanOrEqualTo(100);

        RuleFor(x => x.SortBy)
            .IsInEnum();

        RuleFor(x => x.SortOrder)
            .IsInEnum();
    }
}