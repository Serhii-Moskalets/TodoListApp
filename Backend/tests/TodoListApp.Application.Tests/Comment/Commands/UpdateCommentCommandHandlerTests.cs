using Moq;
using TinyResult.Enums;
using TodoListApp.Application.Abstractions.Interfaces.Repositories;
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
    private readonly Mock<IUnitOfWork> _uowMock;
    private readonly Mock<ICommentRepository> _commentsRepoMock;
    private readonly UpdateCommentCommandHandler _handler;

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateCommentCommandHandlerTests"/> class.
    /// </summary>
    public UpdateCommentCommandHandlerTests()
    {
        this._uowMock = new Mock<IUnitOfWork>();
        this._commentsRepoMock = new Mock<ICommentRepository>();

        this._uowMock.Setup(u => u.Comments).Returns(this._commentsRepoMock.Object);

        this._handler = new UpdateCommentCommandHandler(this._uowMock.Object);
    }

    /// <summary>
    /// Returns failure if the comment does not exist.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task HandleAsync_ShouldReturnFailure_WhenCommentNotFound()
    {
        // Arrange
        this._uowMock.Setup(u => u.Comments.GetByIdAsync(It.IsAny<Guid>(), false, It.IsAny<CancellationToken>()))
               .ReturnsAsync((CommentEntity?)null);

        var command = new UpdateCommentCommand(Guid.NewGuid(), Guid.NewGuid(), "New Text");

        // Act
        var result = await this._handler.Handle(command, CancellationToken.None);

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
        var comment = new CommentEntity(Guid.NewGuid(), Guid.NewGuid(), "Old Text");

        this._uowMock.Setup(u => u.Comments.GetByIdAsync(comment.Id, false, It.IsAny<CancellationToken>()))
               .ReturnsAsync(comment);

        var command = new UpdateCommentCommand(comment.Id, Guid.NewGuid(), "New Text");

        // Act
        var result = await this._handler.Handle(command, CancellationToken.None);

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
        var userId = Guid.NewGuid();
        var comment = new CommentEntity(Guid.NewGuid(), userId, "Old Text");

        this._uowMock.Setup(u => u.Comments.GetByIdAsync(comment.Id, false, It.IsAny<CancellationToken>()))
               .ReturnsAsync(comment);
        this._uowMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var handler = new UpdateCommentCommandHandler(this._uowMock.Object);

        var command = new UpdateCommentCommand(comment.Id, userId, "New Text");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("New Text", comment.Text);
        this._uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
