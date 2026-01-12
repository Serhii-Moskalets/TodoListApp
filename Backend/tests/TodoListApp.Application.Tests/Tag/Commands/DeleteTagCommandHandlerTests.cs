using FluentValidation;
using FluentValidation.Results;
using Moq;
using TinyResult.Enums;
using TodoListApp.Application.Abstractions.Interfaces.Repositories;
using TodoListApp.Application.Abstractions.Interfaces.UnitOfWork;
using TodoListApp.Application.Tag.Commands.DeleteTag;
using TodoListApp.Domain.Entities;

namespace TodoListApp.Application.Tests.Tag.Commands;

/// <summary>
/// Unit tests for <see cref="DeleteTagCommandHandler"/>.
/// Verifies validation handling, tag existence checks, and deletion behavior.
/// </summary>
public class DeleteTagCommandHandlerTests
{
    /// <summary>
    /// Returns a failure result when the command validation fails.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenValidationFails()
    {
        // Arrange
        var validatorMock = new Mock<IValidator<DeleteTagCommand>>();
        validatorMock.Setup(v => v.ValidateAsync(It.IsAny<DeleteTagCommand>(), It.IsAny<CancellationToken>()))
                     .ReturnsAsync(new ValidationResult([new ValidationFailure("TagId", "Invalid Id")]));

        var uowMock = new Mock<IUnitOfWork>();

        var handler = new DeleteTagCommandHandler(uowMock.Object, validatorMock.Object);
        var command = new DeleteTagCommand(Guid.NewGuid(), Guid.NewGuid());

        // Act
        var result = await handler.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal("Invalid Id", result.Error.Message);
    }

    /// <summary>
    /// Returns a failure result when the tag does not exist for the user.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenTagNotFound()
    {
        // Arrange
        var validatorMock = new Mock<IValidator<DeleteTagCommand>>();
        validatorMock.Setup(v => v.ValidateAsync(It.IsAny<DeleteTagCommand>(), It.IsAny<CancellationToken>()))
                     .ReturnsAsync(new ValidationResult());

        var tagRepoMock = new Mock<ITagRepository>();
        tagRepoMock.Setup(r => r.GetTagByIdForUserAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), false, It.IsAny<CancellationToken>()))
                   .ReturnsAsync((TagEntity?)null);

        var uowMock = new Mock<IUnitOfWork>();
        uowMock.Setup(u => u.Tags).Returns(tagRepoMock.Object);

        var handler = new DeleteTagCommandHandler(uowMock.Object, validatorMock.Object);
        var command = new DeleteTagCommand(Guid.NewGuid(), Guid.NewGuid());

        // Act
        var result = await handler.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCode.NotFound, result.Error!.Code);
        Assert.Equal("Tag not found.", result.Error.Message);
    }

    /// <summary>
    /// Deletes the tag successfully when validation passes and the tag exists.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task Handle_ShouldDeleteTag_WhenValidationPassesAndTagExists()
    {
        // Arrange
        var validatorMock = new Mock<IValidator<DeleteTagCommand>>();
        validatorMock.Setup(v => v.ValidateAsync(It.IsAny<DeleteTagCommand>(), It.IsAny<CancellationToken>()))
                     .ReturnsAsync(new ValidationResult());

        var userId = Guid.NewGuid();
        var tagEntity = new TagEntity("TagName", userId);

        var tagRepoMock = new Mock<ITagRepository>();
        tagRepoMock.Setup(r => r.GetTagByIdForUserAsync(tagEntity.Id, userId, false, It.IsAny<CancellationToken>()))
                   .ReturnsAsync(tagEntity);
        tagRepoMock.Setup(r => r.DeleteAsync(tagEntity, It.IsAny<CancellationToken>()))
                   .Returns(Task.CompletedTask);

        var uowMock = new Mock<IUnitOfWork>();
        uowMock.Setup(u => u.Tags).Returns(tagRepoMock.Object);
        uowMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var handler = new DeleteTagCommandHandler(uowMock.Object, validatorMock.Object);
        var command = new DeleteTagCommand(tagEntity.Id, userId);

        // Act
        var result = await handler.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        tagRepoMock.Verify(r => r.DeleteAsync(tagEntity, It.IsAny<CancellationToken>()), Times.Once);
        uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
