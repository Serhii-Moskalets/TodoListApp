using FluentValidation;
using FluentValidation.Results;
using Moq;
using TodoListApp.Application.Abstractions.Interfaces.UnitOfWork;
using TodoListApp.Application.Comment.Commands.UpdateComment;
using TodoListApp.Domain.Entities;
using TodoListApp.Domain.Interfaces.Repositories;

namespace TodoListApp.Application.Tests.Comment.Commands;

/// <summary>
/// Contains unit tests for <see cref="UpdateCommentCommandHandler"/>.
/// Ensures that the command handler correctly validates, finds, and updates comments.
/// </summary>
public class UpdateCommentCommandHandlerTests
{
    /// <summary>
    /// Tests that the handler returns a failure result when validation fails.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test operation.</returns>
    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenValidationFails()
    {
        var validatorMock = new Mock<IValidator<UpdateCommentCommand>>();
        validatorMock
            .Setup(v => v.ValidateAsync(
                It.IsAny<UpdateCommentCommand>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(
                [new ValidationFailure("NewText", "Required")]));

        var uowMock = new Mock<IUnitOfWork>();

        var handler = new UpdateCommentCommandHandler(
            uowMock.Object,
            validatorMock.Object);

        var command = new UpdateCommentCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            string.Empty);

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal("Required", result.Error.Message);
    }

    /// <summary>
    /// Tests that the handler returns a failure result when the comment is not found.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test operation.</returns>
    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenCommentNotFound()
    {
        var validatorMock = new Mock<IValidator<UpdateCommentCommand>>();
        validatorMock
            .Setup(v => v.ValidateAsync(It.IsAny<UpdateCommentCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        var commentRepositoryMock = new Mock<ICommentRepository>();
        commentRepositoryMock
            .Setup(r => r.GetByIdAsync(
                It.IsAny<Guid>(),
                asNoTracking: true,
                cancellationToken: It.IsAny<CancellationToken>()))
            .ReturnsAsync((CommentEntity?)null);

        var uowMock = new Mock<IUnitOfWork>();
        uowMock.Setup(u => u.Comments).Returns(commentRepositoryMock.Object);

        var handler = new UpdateCommentCommandHandler(
            uowMock.Object,
            validatorMock.Object);

        var command = new UpdateCommentCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "New text");

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("Comment not found.", result.Error?.Message);
    }

    /// <summary>
    /// Tests that the handler updates the comment when validation passes and the comment exists.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test operation.</returns>
    [Fact]
    public async Task Handle_ShouldUpdateComment_WhenValidationPasses()
    {
        var commentId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var validatorMock = new Mock<IValidator<UpdateCommentCommand>>();
        validatorMock
            .Setup(v => v.ValidateAsync(It.IsAny<UpdateCommentCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        var commentEntity = new CommentEntity(userId, Guid.NewGuid(), "Old text");

        var commentRepositoryMock = new Mock<ICommentRepository>();
        commentRepositoryMock
            .Setup(r => r.GetByIdAsync(
                commentId,
                asNoTracking: true,
                cancellationToken: It.IsAny<CancellationToken>()))
            .ReturnsAsync(commentEntity);

        var uowMock = new Mock<IUnitOfWork>();
        uowMock.Setup(u => u.Comments).Returns(commentRepositoryMock.Object);
        uowMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
               .ReturnsAsync(1);

        var handler = new UpdateCommentCommandHandler(
            uowMock.Object,
            validatorMock.Object);

        var command = new UpdateCommentCommand(
            commentId,
            userId,
            "Updated text");

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("Updated text", commentEntity.Text);
        uowMock.Verify(
            u => u.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
