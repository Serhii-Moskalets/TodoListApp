using FluentValidation;
using FluentValidation.Results;
using Moq;
using TodoListApp.Application.Comment.Commands.CreateComment;
using TodoListApp.Domain.Entities;
using TodoListApp.Domain.Interfaces.Repositories;
using TodoListApp.Domain.Interfaces.UnitOfWork;

namespace TodoListApp.Application.Tests.Comment.Commands;

/// <summary>
/// Unit tests for <see cref="CreateCommentCommandHandler"/>.
/// Verifies validation handling, comment creation.
/// </summary>
public class CreateCommentCommandHandlerTests
{
    /// <summary>
    /// Ensures that the handler returns a failure result when validation fails.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test operation.</returns>
    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenValidationFails()
    {
        var validatorMock = new Mock<IValidator<CreateCommentCommand>>();
        validatorMock.Setup(v => v.ValidateAsync(It.IsAny<CreateCommentCommand>(), It.IsAny<CancellationToken>()))
                     .ReturnsAsync(new ValidationResult([new ValidationFailure("Text", "Required")]));

        var uowMock = new Mock<IUnitOfWork>();

        var handler = new CreateCommentCommandHandler(uowMock.Object, validatorMock.Object);
        var command = new CreateCommentCommand(Guid.NewGuid(), Guid.NewGuid(), string.Empty);

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal("Required", result.Error.Message);
    }

    /// <summary>
    /// Ensures that a new comment is created successfully when validation passes.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test operation.</returns>
    [Fact]
    public async Task Handle_ShouldAddComment_WhenValidationPasses()
    {
        var validatorMock = new Mock<IValidator<CreateCommentCommand>>();
        validatorMock.Setup(v => v.ValidateAsync(It.IsAny<CreateCommentCommand>(), It.IsAny<CancellationToken>()))
                     .ReturnsAsync(new ValidationResult());

        var commentsRepoMock = new Mock<ICommentRepository>();
        commentsRepoMock.Setup(r => r.AddAsync(It.IsAny<CommentEntity>(), It.IsAny<CancellationToken>()))
                        .Returns(Task.CompletedTask);

        var uowMock = new Mock<IUnitOfWork>();
        uowMock.Setup(u => u.Comments).Returns(commentsRepoMock.Object);
        uowMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var handler = new CreateCommentCommandHandler(uowMock.Object, validatorMock.Object);

        var taskId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var command = new CreateCommentCommand(taskId, userId, "Comments...");

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        commentsRepoMock.Verify(
            r => r.AddAsync(
                It.Is<CommentEntity>(c =>
                    c.TaskId == taskId &&
                    c.UserId == userId &&
                    c.Text == "Comments..."),
                It.IsAny<CancellationToken>()), Times.Once);

        uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
