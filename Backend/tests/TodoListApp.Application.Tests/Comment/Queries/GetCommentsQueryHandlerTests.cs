using Moq;
using TinyResult.Enums;
using TodoListApp.Application.Abstractions.Interfaces.Repositories;
using TodoListApp.Application.Abstractions.Interfaces.Services;
using TodoListApp.Application.Abstractions.Interfaces.UnitOfWork;
using TodoListApp.Application.Comment.Queries.GetComments;
using TodoListApp.Domain.Entities;

namespace TodoListApp.Application.Tests.Comment.Queries;

/// <summary>
/// Unit tests for <see cref="GetCommentsQueryHandler"/>.
/// Verifies access checks and retrieval of comments for a given task.
/// </summary>
public class GetCommentsQueryHandlerTests
{
    /// <summary>
    /// Returns failure if the user does not have access to the task.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenUserHasNoAccess()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var taskAccessMock = new Mock<ITaskAccessService>();
        taskAccessMock
            .Setup(s => s.HasAccessAsync(taskId, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var uowMock = new Mock<IUnitOfWork>();

        var handler = new GetCommentsQueryHandler(uowMock.Object, taskAccessMock.Object);

        var query = new GetCommentsQuery(taskId, userId);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCode.InvalidOperation, result.Error!.Code);
        Assert.Equal("You don't have access to this task.", result.Error.Message);
    }

    /// <summary>
    /// Returns the list of comments successfully when the user has access to the task.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task Handle_ShouldReturnComments_WhenUserHasAccess()
    {
        // Arrange
        var taskId = Guid.NewGuid();

        var user1 = new UserEntity("John", "john", "john@example.com", "hash");
        var user2 = new UserEntity("Alice", "alice", "alice@example.com", "hash");

        var comments = new List<CommentEntity>
        {
            new(taskId, user2.Id, "Comment 1", user2),
            new(taskId, user2.Id, "Comment 2", user2),
        };

        var taskAccessMock = new Mock<ITaskAccessService>();
        taskAccessMock
            .Setup(s => s.HasAccessAsync(taskId, user1.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var commentsRepoMock = new Mock<ICommentRepository>();
        commentsRepoMock
            .Setup(r => r.GetByTaskIdAsync(taskId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(comments);

        var uowMock = new Mock<IUnitOfWork>();
        uowMock.Setup(u => u.Comments).Returns(commentsRepoMock.Object);

        var handler = new GetCommentsQueryHandler(uowMock.Object, taskAccessMock.Object);

        var query = new GetCommentsQuery(taskId, user1.Id);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        var dtos = result.Value!.ToList();
        Assert.Equal(2, dtos.Count);
        Assert.Equal("Comment 1", dtos[0].Text);
        Assert.Equal("Comment 2", dtos[1].Text);
    }
}
