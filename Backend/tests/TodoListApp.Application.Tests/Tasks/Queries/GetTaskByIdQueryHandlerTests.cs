using FluentValidation;
using FluentValidation.Results;
using Moq;
using TinyResult.Enums;
using TodoListApp.Application.Abstractions.Interfaces.Repositories;
using TodoListApp.Application.Abstractions.Interfaces.UnitOfWork;
using TodoListApp.Application.Tasks.Queries.GetTaskById;
using TodoListApp.Domain.Entities;

namespace TodoListApp.Application.Tests.Tasks.Queries;

/// <summary>
/// Unit tests for <see cref="GetTaskByIdQueryHandler"/>.
/// Verifies the behavior of the handler for retrieving a task by its ID.
/// </summary>
public class GetTaskByIdQueryHandlerTests
{
    /// <summary>
    /// Ensures the handler returns failure when validation fails.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenValidationFails()
    {
        // Arrange
        var validatorMock = new Mock<IValidator<GetTaskByIdQuery>>();
        validatorMock
            .Setup(v => v.ValidateAsync(It.IsAny<GetTaskByIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult([new ValidationFailure("TaskId", "Task ID is required")]));

        var uowMock = new Mock<IUnitOfWork>();

        var handler = new GetTaskByIdQueryHandler(uowMock.Object, validatorMock.Object);

        var query = new GetTaskByIdQuery(Guid.Empty, Guid.NewGuid());

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal("Task ID is required", result.Error.Message);
    }

    /// <summary>
    /// Ensures the handler returns NotFound when the task does not exist.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task Handle_ShouldReturnNotFound_WhenTaskDoesNotExist()
    {
        // Arrange
        var validatorMock = new Mock<IValidator<GetTaskByIdQuery>>();
        validatorMock
            .Setup(v => v.ValidateAsync(It.IsAny<GetTaskByIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        var taskRepoMock = new Mock<ITaskRepository>();
        taskRepoMock
            .Setup(r => r.GetTaskByIdForUserAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((TaskEntity?)null);

        var uowMock = new Mock<IUnitOfWork>();
        uowMock.Setup(u => u.Tasks).Returns(taskRepoMock.Object);

        var handler = new GetTaskByIdQueryHandler(uowMock.Object, validatorMock.Object);

        var query = new GetTaskByIdQuery(Guid.NewGuid(), Guid.NewGuid());

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assertz
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCode.NotFound, result.Error!.Code);
        Assert.Equal("Task not found.", result.Error.Message);
    }

    /// <summary>
    /// Ensures the handler returns a valid task DTO when the task exists.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task Handle_ShouldReturnTaskDto_WhenTaskExists()
    {
        // Arrange
        var validatorMock = new Mock<IValidator<GetTaskByIdQuery>>();
        validatorMock
            .Setup(v => v.ValidateAsync(It.IsAny<GetTaskByIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        var userId = Guid.NewGuid();

        var task = new TaskEntity(userId, Guid.NewGuid(), "Test Task", DateTime.UtcNow.AddDays(1));

        var taskRepoMock = new Mock<ITaskRepository>();
        taskRepoMock
            .Setup(r => r.GetTaskByIdForUserAsync(
                It.IsAny<Guid>(),
                It.IsAny<Guid>(),
                true,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(task);

        var uowMock = new Mock<IUnitOfWork>();
        uowMock.Setup(u => u.Tasks).Returns(taskRepoMock.Object);

        var handler = new GetTaskByIdQueryHandler(uowMock.Object, validatorMock.Object);

        var query = new GetTaskByIdQuery(task.Id, userId);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(task.Id, result.Value!.Id);
        Assert.Equal(task.Title, result.Value.Title);
        Assert.Equal(task.Description, result.Value.Description);
        Assert.Equal(task.DueDate, result.Value.DueDate);
    }
}
