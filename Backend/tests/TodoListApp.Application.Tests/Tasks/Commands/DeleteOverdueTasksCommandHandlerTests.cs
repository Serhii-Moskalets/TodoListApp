using Moq;
using TinyResult.Enums;
using TodoListApp.Application.Abstractions.Interfaces.Repositories;
using TodoListApp.Application.Abstractions.Interfaces.UnitOfWork;
using TodoListApp.Application.Tasks.Commands.DeleteOverdueTasks;
using TodoListApp.Domain.Entities;

namespace TodoListApp.Application.Tests.Tasks.Commands;

/// <summary>
/// Unit tests for <see cref="DeleteOverdueTasksCommandHandler"/>.
/// Verifies the behavior of the handler for deleting task.
/// </summary>
public class DeleteOverdueTasksCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _uowMock;
    private readonly Mock<ITaskListRepository> _taskListRepoMock;
    private readonly DeleteOverdueTasksCommandHandler _handler;

    /// <summary>
    /// Initializes a new instance of the <see cref="DeleteOverdueTasksCommandHandlerTests"/> class.
    /// </summary>
    public DeleteOverdueTasksCommandHandlerTests()
    {
        this._uowMock = new Mock<IUnitOfWork>();
        this._taskListRepoMock = new Mock<ITaskListRepository>();

        this._uowMock.Setup(u => u.TaskLists).Returns(this._taskListRepoMock.Object);

        this._handler = new DeleteOverdueTasksCommandHandler(this._uowMock.Object);
    }

    /// <summary>
    /// Ensures the handler returns a not-found error when the task list does not exist.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task Handle_ShouldReturnNotFound_WhenTaskListDoesNotExist()
    {
        // Arrange
        this._taskListRepoMock
            .Setup(r => r.GetTaskListByIdForUserAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), false, It.IsAny<CancellationToken>()))
            .ReturnsAsync((TaskListEntity?)null);

        this._uowMock.Setup(u => u.TaskLists).Returns(this._taskListRepoMock.Object);

        var command = new DeleteOverdueTasksCommand(Guid.NewGuid(), Guid.NewGuid());

        // Act
        var result = await this._handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCode.NotFound, result.Error!.Code);
    }

    /// <summary>
    /// Ensures the handler deletes overdue tasks when the task list exists.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task Handle_ShouldDeleteOverdueTasks_WhenTaskListExists()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var taskListMock = new Mock<TaskListEntity>(userId, "My list") { CallBase = true };

        this._taskListRepoMock
            .Setup(r => r.GetTaskListByIdForUserAsync(taskListMock.Object.Id, userId, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(taskListMock.Object);

        this._uowMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var command = new DeleteOverdueTasksCommand(taskListMock.Object.Id, userId);

        // Act
        var result = await this._handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        taskListMock.Verify(t => t.DeleteOverdueTasks(It.IsAny<DateTime>()), Times.Once);
        this._uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
