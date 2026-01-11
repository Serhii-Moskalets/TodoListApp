using FluentValidation;
using FluentValidation.Results;
using Moq;
using TinyResult.Enums;
using TodoListApp.Application.Abstractions.Interfaces.UnitOfWork;
using TodoListApp.Application.Comment.Commands.UpdateComment;
using TodoListApp.Domain.Entities;

namespace TodoListApp.Application.Tests.Comment.Commands;

/// <summary>
/// Unit tests for <see cref="UpdateCommentCommandHandler"/>.
/// Verifies validation, existence, permission checks, and comment updating.
/// </summary>
public class UpdateCommentCommandHandlerTests
{
    /// <summary>
    /// Returns failure if validation fails.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task HandleAsync_ShouldReturnFailure_WhenValidationFails()
    {
        // Arrange
        var validatorMock = new Mock<IValidator<UpdateCommentCommand>>();
        validatorMock
            .Setup(v => v.ValidateAsync(It.IsAny<UpdateCommentCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(new[] { new ValidationFailure("NewText", "Required") }));

        var uowMock = new Mock<IUnitOfWork>();
        var handler = new UpdateCommentCommandHandler(uowMock.Object, validatorMock.Object);

        var command = new UpdateCommentCommand(Guid.NewGuid(), Guid.NewGuid(), string.Empty);

        // Act
        var result = await handler.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Required", result.Error!.Message);
    }

    /// <summary>
    /// Returns failure if the comment does not exist.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task HandleAsync_ShouldReturnFailure_WhenCommentNotFound()
    {
        // Arrange
        var validatorMock = new Mock<IValidator<UpdateCommentCommand>>();
        validatorMock
            .Setup(v => v.ValidateAsync(It.IsAny<UpdateCommentCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        var uowMock = new Mock<IUnitOfWork>();
        uowMock.Setup(u => u.Comments.GetByIdAsync(It.IsAny<Guid>(), false, It.IsAny<CancellationToken>()))
               .ReturnsAsync((CommentEntity?)null);

        var handler = new UpdateCommentCommandHandler(uowMock.Object, validatorMock.Object);

        var command = new UpdateCommentCommand(Guid.NewGuid(), Guid.NewGuid(), "New Text");

        // Act
        var result = await handler.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCode.NotFound, result.Error!.Code);
        Assert.Equal("Comment not found.", result.Error.Message);
    }

    /// <summary>
    /// Returns failure if the user is not the owner of the comment.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task HandleAsync_ShouldReturnFailure_WhenUserIsNotOwner()
    {
        // Arrange
        var validatorMock = new Mock<IValidator<UpdateCommentCommand>>();
        validatorMock
            .Setup(v => v.ValidateAsync(It.IsAny<UpdateCommentCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        var comment = new CommentEntity(Guid.NewGuid(), Guid.NewGuid(), "Old Text");

        var uowMock = new Mock<IUnitOfWork>();
        uowMock.Setup(u => u.Comments.GetByIdAsync(comment.Id, false, It.IsAny<CancellationToken>()))
               .ReturnsAsync(comment);

        var handler = new UpdateCommentCommandHandler(uowMock.Object, validatorMock.Object);

        var command = new UpdateCommentCommand(comment.Id, Guid.NewGuid(), "New Text");

        // Act
        var result = await handler.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCode.InvalidOperation, result.Error!.Code);
        Assert.Equal("You don't have permission to update this comment.", result.Error.Message);
    }

    /// <summary>
    /// Updates the comment successfully when the user is the owner.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task HandleAsync_ShouldUpdateComment_WhenUserIsOwner()
    {
        // Arrange
        var validatorMock = new Mock<IValidator<UpdateCommentCommand>>();
        validatorMock
            .Setup(v => v.ValidateAsync(It.IsAny<UpdateCommentCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        var userId = Guid.NewGuid();
        var comment = new CommentEntity(Guid.NewGuid(), userId, "Old Text");

        var uowMock = new Mock<IUnitOfWork>();
        uowMock.Setup(u => u.Comments.GetByIdAsync(comment.Id, false, It.IsAny<CancellationToken>()))
               .ReturnsAsync(comment);
        uowMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var handler = new UpdateCommentCommandHandler(uowMock.Object, validatorMock.Object);

        var command = new UpdateCommentCommand(comment.Id, userId, "New Text");

        // Act
        var result = await handler.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("New Text", comment.Text);
        uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
