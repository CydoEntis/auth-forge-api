using FluentValidation;

namespace AuthForge.Application.Applications.Queries.GetAllApplications;

public sealed class ApplicationFilterParametersValidator : AbstractValidator<ApplicationFilterParameters>
{
    public ApplicationFilterParametersValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThan(0)
            .WithMessage("Page number must be greater than 0");

        RuleFor(x => x.PageSize)
            .GreaterThan(0)
            .WithMessage("Page size must be greater than 0")
            .LessThanOrEqualTo(100)
            .WithMessage("Page size cannot exceed 100");

        RuleFor(x => x.SearchTerm)
            .MaximumLength(100)
            .When(x => !string.IsNullOrWhiteSpace(x.SearchTerm))
            .WithMessage("Search term cannot exceed 100 characters");

        RuleFor(x => x.SortBy)
            .IsInEnum()
            .WithMessage("Invalid sort by value");

        RuleFor(x => x.SortOrder)
            .IsInEnum()
            .WithMessage("Invalid sort order value");
    }
}