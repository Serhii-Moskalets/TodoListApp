using Moq;
using TodoListApp.Application.Abstractions.Interfaces.Repositories;
using TodoListApp.Application.Abstractions.Interfaces.Services;
using TodoListApp.Application.Abstractions.Interfaces.UnitOfWork;
using TodoListApp.Application.TaskList.Commands.UpdateTaskList;
using TodoListApp.Domain.Entities;

namespace TodoListApp.Application.Tests.TaskList.Commands;

/// <summary>
/// Unit tests for <see cref="UpdateTaskListCommandHandler"/>.
/// Verifies validation, existence checks, and title update logic when updating a task list.
/// </summary>
public class UpdateTaskListCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _uowMock;
    private readonly Mock<IUniqueNameService> _uniqueNameServiceMock;
    private readonly Mock<ITaskListRepository> _taskListRepoMock;
    private readonly UpdateTaskListCommandHandler _handler;

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateTaskListCommandHandlerTests"/> class.
    /// </summary>
    public UpdateTaskListCommandHandlerTests()
    {
        this._uowMock = new Mock<IUnitOfWork>();
        this._uniqueNameServiceMock = new Mock<IUniqueNameService>();
        this._taskListRepoMock = new Mock<ITaskListRepository>();

        this._uowMock.Setup(tl => tl.TaskLists).Returns(this._taskListRepoMock.Object);
        this._handler = new UpdateTaskListCommandHandler(this._uowMock.Object, this._uniqueNameServiceMock.Object);
    }

    /// <summary>
    /// Returns failure if the task list does not exist.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task Handle_ShouldReturnNotFound_WhenTaskListDoesNotExist()
    {
        // Arrange
        this._uowMock.Setup(
            u =>
            u.TaskLists.GetTaskListByIdForUserAsync(
                It.IsAny<Guid>(),
                It.IsAny<Guid>(),
                It.IsAny<bool>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((TaskListEntity)null!);

        var command = new UpdateTaskListCommand(Guid.NewGuid(), Guid.NewGuid(), "NewTitle");

        // Act
        var result = await this._handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Task list not found.", result.Error!.Message);
    }

    /// <summary>
    /// Returns success without calling uniqueness service if the new title is unchanged.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenTitleIsUnchanged()
    {
        // Arrange
        var taskList = new TaskListEntity(Guid.NewGuid(), "SameTitle");

        this._uowMock.Setup(u => u.TaskLists.GetTaskListByIdForUserAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), false, It.IsAny<CancellationToken>()))
               .ReturnsAsync(taskList);

        var command = new UpdateTaskListCommand(taskList.Id, Guid.NewGuid(), "SameTitle");

        // Act
        var result = await this._handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        this._uniqueNameServiceMock.Verify(
            s => s.GetUniqueNameAsync(
            It.IsAny<string>(),
            It.IsAny<Func<string, CancellationToken, Task<bool>>>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    /// Updates the task list title if a new title is provided and ensures uniqueness.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task Handle_ShouldUpdateTitle_WhenNewTitleIsDifferent()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var taskList = new TaskListEntity(Guid.NewGuid(), "OldTitle");
        var newTitle = "NewTitle";

        var uowMock = new Mock<IUnitOfWork>();
        uowMock.Setup(u => u.TaskLists.GetTaskListByIdForUserAsync(taskList.Id, userId, false, It.IsAny<CancellationToken>()))
                .ReturnsAsync(taskList);

        uowMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var serviceMock = new Mock<IUniqueNameService>();
        serviceMock.Setup(s => s.GetUniqueNameAsync(
                It.Is<string>(t => t == newTitle),
                It.IsAny<Func<string, CancellationToken, Task<bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync("NewTitle Unique");

        var handler = new UpdateTaskListCommandHandler(uowMock.Object, serviceMock.Object);

        var command = new UpdateTaskListCommand(taskList.Id, userId, "NewTitle");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("NewTitle Unique", taskList.Title);
        uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
