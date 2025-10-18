using AuthForge.Application.Applications.Commands.Create;
using AuthForge.Application.Common.Interfaces;
using AuthForge.Domain.Entities;
using AuthForge.Domain.Errors;
using AuthForge.Domain.ValueObjects;
using FluentAssertions;
using Moq;
using Xunit;

namespace AuthForge.Application.Tests.Applications.Commands;

public class CreateApplicationCommandHandlerTests
{
    private readonly Mock<IApplicationRepository> _applicationRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly CreateApplicationCommandHandler _handler;

    public CreateApplicationCommandHandlerTests()
    {
        _applicationRepositoryMock = new Mock<IApplicationRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();

        _handler = new CreateApplicationCommandHandler(
            _applicationRepositoryMock.Object,
            _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidRequest_ShouldCreateApplication()
    {
        var command = new CreateApplicationCommand("My Awesome App");

        _applicationRepositoryMock
            .Setup(x => x.SlugExistsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be("My Awesome App");
        result.Value.Slug.Should().Be("my-awesome-app");
        result.Value.IsActive.Should().BeTrue();

        _applicationRepositoryMock.Verify(
            x => x.AddAsync(It.IsAny<Domain.Entities.Application>(), It.IsAny<CancellationToken>()),
            Times.Once);

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WithDuplicateSlug_ShouldAppendGuidToSlug()
    {
        var command = new CreateApplicationCommand("My App");

        _applicationRepositoryMock
            .Setup(x => x.SlugExistsAsync("my-app", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Slug.Should().StartWith("my-app-");
        result.Value.Slug.Should().NotBe("my-app");
    }

    [Theory]
    [InlineData("Test App", "test-app")]
    [InlineData("My Awesome App", "my-awesome-app")]
    [InlineData("App With   Spaces", "app-with-spaces")]
    [InlineData("App_With_Underscores", "app-with-underscores")]
    public async Task Handle_ShouldGenerateCorrectSlug(string appName, string expectedSlug)
    {
        var command = new CreateApplicationCommand(appName);

        _applicationRepositoryMock
            .Setup(x => x.SlugExistsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Slug.Should().Be(expectedSlug);
    }
}