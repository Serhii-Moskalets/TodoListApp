using Moq;
using TodoListApp.Application.Abstractions.Interfaces.UnitOfWork;
using TodoListApp.Application.Comment.Queries.GetComments;
using TodoListApp.Application.Common.Dtos;
using TodoListApp.Domain.Entities;
using TodoListApp.Domain.Interfaces.Repositories;

namespace TodoListApp.Application.Tests.Comment.Queries;

/// <summary>
/// Unit tests for <see cref="GetCommentsQueryHandler"/>.
/// Ensures that the handler correctly retrieves all comments for a task and maps them to <see cref="CommentDto"/>.
/// </summary>
public class GetCommentsQueryHandlerTests
{
    /// <summary>
    /// Tests that the handler returns a list of <see cref="CommentDto"/> when comments exist for the task.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test operation.</returns>
    [Fact]
    public async Task Handle_ShouldReturnComments_WhenCommentsExist()
    {
        var user_1 = new UserEntity("John1", "john1", "john1@example.com", "hash");
        var user_2 = new UserEntity("John2", "john2", "john2@example.com", "hash");
        var task = new TaskEntity(user_2.Id, Guid.NewGuid(), "Task");

        var comments = new List<CommentEntity>
        {
            new(task.Id, user_1.Id, "First comment", user_1),
            new(task.Id, user_2.Id, "Second comment", user_2),
        };

        var commentRepoMock = new Mock<ICommentRepository>();
        commentRepoMock.Setup(r => r.GetByTaskIdAsync(task.Id, It.IsAny<CancellationToken>()))
                       .ReturnsAsync(comments);

        var uowMock = new Mock<IUnitOfWork>();
        uowMock.Setup(u => u.Comments).Returns(commentRepoMock.Object);

        var handler = new GetCommentsQueryHandler(uowMock.Object);
        var query = new GetCommentsQuery(task.Id);

        var result = await handler.Handle(query, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(2, result.Value!.Count());
        Assert.Contains(result.Value, c => c.Text == "First comment");
        Assert.Contains(result.Value, c => c.Text == "Second comment");
    }

    /// <summary>
    /// Tests that the handler returns an empty list when no comments exist for the task.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test operation.</returns>
    [Fact]
    public async Task Handle_ShouldReturnEmptyList_WhenNoCommentsExist()
    {
        var taskId = Guid.NewGuid();

        var commentRepoMock = new Mock<ICommentRepository>();
        commentRepoMock.Setup(r => r.GetByTaskIdAsync(taskId, It.IsAny<CancellationToken>()))
                       .ReturnsAsync([]);

        var uowMock = new Mock<IUnitOfWork>();
        uowMock.Setup(u => u.Comments).Returns(commentRepoMock.Object);

        var handler = new GetCommentsQueryHandler(uowMock.Object);
        var query = new GetCommentsQuery(taskId);

        var result = await handler.Handle(query, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Empty(result.Value);
    }
}
