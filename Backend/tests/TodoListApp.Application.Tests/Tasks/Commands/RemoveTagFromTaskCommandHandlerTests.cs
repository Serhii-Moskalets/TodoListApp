using Moq;
using TinyResult.Enums;
using TodoListApp.Application.Abstractions.Interfaces.Repositories;
using TodoListApp.Application.Abstractions.Interfaces.UnitOfWork;
using TodoListApp.Application.Tasks.Commands.RemoveTagFromTask;
using TodoListApp.Domain.Entities;

namespace TodoListApp.Application.Tests.Tasks.Commands;

/// <summary>
/// Unit tests for <see cref="RemoveTagFromTaskCommandHandler"/>.
/// Verifies behavior when removing a tag from a task.
/// </summary>
public class RemoveTagFromTaskCommandHandlerTests
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
        var command = new RemoveTagFromTaskCommand(taskId, userId);

        var taskRepoMock = new Mock<ITaskRepository>();
        taskRepoMock.Setup(r => r.GetByIdAsync(taskId, false, It.IsAny<CancellationToken>()))
                    .ReturnsAsync((TaskEntity?)null);

        var uowMock = new Mock<IUnitOfWork>();
        uowMock.Setup(u => u.Tasks).Returns(taskRepoMock.Object);

        var handler = new RemoveTagFromTaskCommandHandler(uowMock.Object);

        var ct = CancellationToken.None;
        var result = await handler.HandleAsync(command, ct);

        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal("Task not found.", result.Error.Message);
        Assert.Equal(ErrorCode.NotFound, result.Error.Code);

        uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    /// Tests that the handler successfully removes a tag from an existing task.
    /// Ensures that <see cref="TaskEntity.SetTag"/> is called with null and <see cref="IUnitOfWork.SaveChangesAsync"/> is invoked once.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test operation.</returns>
    [Fact]
    public async Task Handle_ShouldRemoveTag_WhenTaskExists()
    {
        var taskId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var command = new RemoveTagFromTaskCommand(taskId, userId);

        var taskEntity = new TaskEntity(userId, Guid.NewGuid(), "Test Task");
        taskEntity.SetTag(Guid.NewGuid());
        var taskRepoMock = new Mock<ITaskRepository>();
        taskRepoMock.Setup(r => r.GetByIdAsync(taskId, false, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(taskEntity);

        var uowMock = new Mock<IUnitOfWork>();
        uowMock.Setup(u => u.Tasks).Returns(taskRepoMock.Object);
        uowMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var handler = new RemoveTagFromTaskCommandHandler(uowMock.Object);

        var ct = CancellationToken.None;
        var result = await handler.HandleAsync(command, ct);

        Assert.True(result.IsSuccess);
        Assert.Null(taskEntity.TagId);

        uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
