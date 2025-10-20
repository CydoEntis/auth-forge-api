using AuthForge.Application.Applications.Enums;
using AuthForge.Application.Applications.Queries.GetAll;
using AuthForge.Application.Applications.Queries.GetAllApplications;
using AuthForge.Application.Common.Models;
using FluentAssertions;

namespace AuthForge.Application.Tests.Applications.Queries;

public sealed class ApplicationFilterParametersValidatorTests
{
    private readonly ApplicationFilterParametersValidator _validator = new();

    [Fact]
    public void Validate_WithValidParameters_ShouldSucceed()
    {
        var parameters = new ApplicationFilterParameters
        {
            PageNumber = 1,
            PageSize = 10,
            SearchTerm = "test",
            IsActive = true,
            SortBy = ApplicationSortBy.Name,
            SortOrder = SortOrder.Asc
        };

        var result = _validator.Validate(parameters);

        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Validate_WithInvalidPageNumber_ShouldFail(int pageNumber)
    {
        var parameters = new ApplicationFilterParameters
        {
            PageNumber = pageNumber,
            PageSize = 10
        };

        var result = _validator.Validate(parameters);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(ApplicationFilterParameters.PageNumber));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(101)]
    public void Validate_WithInvalidPageSize_ShouldFail(int pageSize)
    {
        var parameters = new ApplicationFilterParameters
        {
            PageNumber = 1,
            PageSize = pageSize
        };

        var result = _validator.Validate(parameters);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(ApplicationFilterParameters.PageSize));
    }

    [Fact]
    public void Validate_WithSearchTermTooLong_ShouldFail()
    {
        // Arrange
        var parameters = new ApplicationFilterParameters
        {
            PageNumber = 1,
            PageSize = 10,
            SearchTerm = new string('a', 101) 
        };

        var result = _validator.Validate(parameters);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(ApplicationFilterParameters.SearchTerm));
    }

    [Fact]
    public void Validate_WithEmptySearchTerm_ShouldSucceed()
    {
        var parameters = new ApplicationFilterParameters
        {
            PageNumber = 1,
            PageSize = 10,
            SearchTerm = ""
        };

        var result = _validator.Validate(parameters);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithNullSearchTerm_ShouldSucceed()
    {
        var parameters = new ApplicationFilterParameters
        {
            PageNumber = 1,
            PageSize = 10,
            SearchTerm = null
        };

        var result = _validator.Validate(parameters);

        result.IsValid.Should().BeTrue();
    }
}