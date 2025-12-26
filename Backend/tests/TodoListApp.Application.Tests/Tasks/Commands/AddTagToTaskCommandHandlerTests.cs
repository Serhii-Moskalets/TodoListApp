using Moq;
using TodoListApp.Application.Abstractions.Interfaces.UnitOfWork;
using TodoListApp.Application.Tasks.Commands.AddTagToTask;
using TodoListApp.Domain.Entities;
using TodoListApp.Domain.Interfaces.Repositories;

namespace TodoListApp.Application.Tests.Tasks.Commands;

/// <summary>
/// Unit tests for <see cref="AddTagToTaskCommandHandler"/>.
/// Verifies behavior when adding tags to tasks.
/// </summary>
public class AddTagToTaskCommandHandlerTests
{
    /// <summary>
    /// Tests that the handler returns a failure result when the specified task does not exist.
    /// Ensures that <see cref="IUnitOfWork.SaveChangesAsync"/> is not called.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test operation.</returns>
    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenTaskNotFound()
    {
        var taskId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var tagId = Guid.NewGuid();
        var command = new AddTagToTaskCommand(taskId, userId, tagId);

        var taskRepoMock = new Mock<ITaskRepository>();
        taskRepoMock.Setup(r => r.GetTaskByIdForUserAsync(taskId, userId, It.IsAny<CancellationToken>()))
                    .ReturnsAsync((TaskEntity?)null);

        var uowMock = new Mock<IUnitOfWork>();
        uowMock.Setup(u => u.Tasks).Returns(taskRepoMock.Object);

        var handler = new AddTagToTaskCommandHandler(uowMock.Object);

        var ct = CancellationToken.None;
        var result = await handler.Handle(command, ct);

        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal("Task not found.", result.Error.Message);
        Assert.Equal(TinyResult.Enums.ErrorCode.NotFound, result.Error.Code);

        uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    /// Tests that the handler successfully adds a tag to an existing task.
    /// Ensures that <see cref="TaskEntity.SetTag"/> is called and <see cref="IUnitOfWork.SaveChangesAsync"/> is invoked once.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test operation.</returns>
    [Fact]
    public async Task Handle_ShouldAddTag_WhenTaskExists()
    {
        var taskId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var tagId = Guid.NewGuid();
        var command = new AddTagToTaskCommand(taskId, userId, tagId);

        var taskEntity = new TaskEntity(userId, Guid.NewGuid(), "Test Task");
        var taskRepoMock = new Mock<ITaskRepository>();
        taskRepoMock.Setup(r => r.GetTaskByIdForUserAsync(taskId, userId, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(taskEntity);

        var uowMock = new Mock<IUnitOfWork>();
        uowMock.Setup(u => u.Tasks).Returns(taskRepoMock.Object);
        uowMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var handler = new AddTagToTaskCommandHandler(uowMock.Object);

        var ct = CancellationToken.None;
        var result = await handler.Handle(command, ct);

        Assert.True(result.IsSuccess);
        Assert.Equal(tagId, taskEntity.TagId);

        uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
