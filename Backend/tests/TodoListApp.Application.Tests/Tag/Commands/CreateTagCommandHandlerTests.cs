using FluentValidation;
using FluentValidation.Results;
using Moq;
using TodoListApp.Application.Abstractions.Interfaces.UnitOfWork;
using TodoListApp.Application.Tag.Commands.CreateTag;
using TodoListApp.Domain.Entities;
using TodoListApp.Domain.Interfaces.Repositories;

namespace TodoListApp.Application.Tests.Tag.Commands;

/// <summary>
/// Unit tests for <see cref="CreateTagCommandHandler"/>.
/// Verifies validation handling, tag creation,
/// and automatic name suffixing when duplicates exist.
/// </summary>
public class CreateTagCommandHandlerTests
{
    /// <summary>
    /// Ensures that the handler returns a failure result when command validation fails.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test operation.</returns>
    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenValidationFails()
    {
        var validatorMock = new Mock<IValidator<CreateTagCommand>>();
        validatorMock.Setup(v => v.ValidateAsync(It.IsAny<CreateTagCommand>(), It.IsAny<CancellationToken>()))
                     .ReturnsAsync(new ValidationResult([new ValidationFailure("Name", "Required")]));

        var uowMock = new Mock<IUnitOfWork>();

        var handler = new CreateTagCommandHandler(uowMock.Object, validatorMock.Object);

        var command = new CreateTagCommand(Guid.NewGuid(), string.Empty);

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal("Required", result.Error.Message);
    }

    /// <summary>
    /// Ensures that a new tag is created successfully when the provided name is unique for the user.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test operation.</returns>
    [Fact]
    public async Task Handle_ShouldAddTag_WhenNameIsUnique()
    {
        var validatorMock = new Mock<IValidator<CreateTagCommand>>();
        validatorMock.Setup(v => v.ValidateAsync(It.IsAny<CreateTagCommand>(), It.IsAny<CancellationToken>()))
                     .ReturnsAsync(new ValidationResult());

        var tagRepoMock = new Mock<ITagRepository>();
        tagRepoMock.Setup(r => r.ExistsByNameAsync(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                   .ReturnsAsync(false);

        var uowMock = new Mock<IUnitOfWork>();
        uowMock.Setup(u => u.Tags).Returns(tagRepoMock.Object);
        uowMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var handler = new CreateTagCommandHandler(uowMock.Object, validatorMock.Object);

        var userId = Guid.NewGuid();
        var command = new CreateTagCommand(userId, "Tag");

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        tagRepoMock.Verify(r => r.AddAsync(It.Is<TagEntity>(t => t.UserId == userId && t.Name == "Tag"), It.IsAny<CancellationToken>()), Times.Once);
        uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Ensures that a numeric suffix is appended to the tag name when a tag with the same name already exists for the user.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test operation.</returns>
    [Fact]
    public async Task Handle_ShouldAppendSuffix_WhenNameExists()
    {
        var validatorMock = new Mock<IValidator<CreateTagCommand>>();
        validatorMock.Setup(v => v.ValidateAsync(It.IsAny<CreateTagCommand>(), It.IsAny<CancellationToken>()))
                     .ReturnsAsync(new ValidationResult());

        var tagRepoMock = new Mock<ITagRepository>();
        tagRepoMock.SetupSequence(r => r.ExistsByNameAsync(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                   .ReturnsAsync(true)
                   .ReturnsAsync(false);

        var uowMock = new Mock<IUnitOfWork>();
        uowMock.Setup(u => u.Tags).Returns(tagRepoMock.Object);
        uowMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var handler = new CreateTagCommandHandler(uowMock.Object, validatorMock.Object);

        var userId = Guid.NewGuid();
        var command = new CreateTagCommand(userId, "Tag");

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        tagRepoMock.Verify(r => r.AddAsync(It.Is<TagEntity>(t => t.Name == "Tag (1)" && t.UserId == userId), It.IsAny<CancellationToken>()), Times.Once);
    }
}
