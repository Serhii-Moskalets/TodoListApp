using Moq;
using TinyResult.Enums;
using TodoListApp.Application.Abstractions.Interfaces.Repositories;
using TodoListApp.Application.Abstractions.Interfaces.UnitOfWork;
using TodoListApp.Application.Tasks.Commands.UpdateTask;
using TodoListApp.Application.Tasks.Dtos;
using TodoListApp.Domain.Entities;

namespace TodoListApp.Application.Tests.Tasks.Commands;

/// <summary>
/// Unit tests for <see cref="UpdateTaskCommandHandler"/>.
/// Ensures that the handler correctly validates, finds, and updates a task.
/// </summary>
public class UpdateTaskCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _uowMock;
    private readonly Mock<ITaskRepository> _taskRepoMock;
    private readonly UpdateTaskCommandHandler _handler;

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateTaskCommandHandlerTests"/> class.
    /// </summary>
    public UpdateTaskCommandHandlerTests()
    {
        this._uowMock = new Mock<IUnitOfWork>();
        this._taskRepoMock = new Mock<ITaskRepository>();

        this._uowMock.Setup(u => u.Tasks).Returns(this._taskRepoMock.Object);
        this._handler = new UpdateTaskCommandHandler(this._uowMock.Object);
    }

    /// <summary>
    /// Ensures the handler returns a not-found error when the task does not exist.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task Handle_ShouldReturnNotFound_WhenTaskDoesNotExist()
    {
        // Arrange
        this._taskRepoMock.Setup(
            r =>
            r.GetTaskByIdForUserAsync(
                It.IsAny<Guid>(),
                It.IsAny<Guid>(),
                It.IsAny<bool>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((TaskEntity?)null);

        var dto = new UpdateTaskDto
        {
            TaskId = Guid.NewGuid(),
            Title = "New Title",
        };

        var command = new UpdateTaskCommand(dto, Guid.NewGuid());

        // Act
        var result = await this._handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCode.NotFound, result.Error!.Code);
        Assert.Equal("Task not found.", result.Error.Message);
    }

    /// <summary>
    /// Ensures the handler updates the task and saves changes when the command is valid.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task Handle_ShouldUpdateTask_WhenCommandIsValid()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var taskListId = Guid.NewGuid();

        var task = new TaskEntity(userId, taskListId, "Old Title");

        this._taskRepoMock.Setup(
            r =>
            r.GetTaskByIdForUserAsync(
                task.Id,
                userId,
                false,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(task);

        this._uowMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var dto = new UpdateTaskDto
        {
            TaskId = task.Id,
            Title = "New Title",
            Description = "New Description",
            DueDate = DateTime.UtcNow.AddDays(1),
        };

        var command = new UpdateTaskCommand(dto, userId);

        // Act
        var result = await this._handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);

        Assert.Equal("New Title", task.Title);
        Assert.Equal("New Description", task.Description);
        Assert.Equal(dto.DueDate, task.DueDate);

        this._uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Verifies that the handler avoids unnecessary database writes by checking for data equality.
    /// If the incoming DTO data is identical to the current entity state, <c>SaveChangesAsync</c> should not be called.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test execution.</returns>
    [Fact]
    public async Task Handle_ShouldNotSave_WhenDataIsIdentical()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var title = "Same Title";
        var task = new TaskEntity(userId, Guid.NewGuid(), title);

        this._taskRepoMock.Setup(
            r =>
            r.GetTaskByIdForUserAsync(
                task.Id,
                userId,
                false,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(task);

        var dto = new UpdateTaskDto
        {
            TaskId = task.Id,
            Title = title,
            Description = task.Description,
            DueDate = task.DueDate,
        };

        var command = new UpdateTaskCommand(dto, userId);

        // Act
        var result = await this._handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        this._uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
