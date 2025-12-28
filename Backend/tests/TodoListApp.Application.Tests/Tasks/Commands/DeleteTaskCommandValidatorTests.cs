using Moq;
using TodoListApp.Application.Abstractions.Interfaces.Repositories;
using TodoListApp.Application.Abstractions.Interfaces.UnitOfWork;
using TodoListApp.Application.Tasks.Commands.DeleteTask;

namespace TodoListApp.Application.Tests.Tasks.Commands;

/// <summary>
/// Unit tests for <see cref="DeleteTaskCommandValidator"/>.
/// Ensures that the validator correctly verifies task ownership.
/// </summary>
public class DeleteTaskCommandValidatorTests
{
    /// <summary>
    /// Tests that validation fails when the task does not belong to the user.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test operation.</returns>
    [Fact]
    public async Task Validate_ShouldHaveError_WhenUserIsNotOwner()
    {
        var taskId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var command = new DeleteTaskCommand(taskId, userId);

        var taskRepoMock = new Mock<ITaskRepository>();
        taskRepoMock.Setup(r => r.ExistsForUserAsync(taskId, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var uowMock = new Mock<IUnitOfWork>();
        uowMock.Setup(u => u.Tasks).Returns(taskRepoMock.Object);

        var validator = new DeleteTaskCommandValidator(uowMock.Object);

        var result = await validator.ValidateAsync(command);

        Assert.False(result.IsValid);
        var error = Assert.Single(result.Errors);
        Assert.NotNull(error);
        Assert.Equal("Task not found or does not belong to the user.", error.ErrorMessage);
        Assert.Equal("TaskId", error.PropertyName);
    }

    /// <summary>
    /// Tests that validation passes when the task belongs to the user.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test operation.</returns>
    [Fact]
    public async Task Validate_ShouldNotHaveError_WhenUserIsOwner()
    {
        var taskId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var command = new DeleteTaskCommand(taskId, userId);

        var taskRepoMock = new Mock<ITaskRepository>();
        taskRepoMock.Setup(r => r.ExistsForUserAsync(taskId, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var uowMock = new Mock<IUnitOfWork>();
        uowMock.Setup(u => u.Tasks).Returns(taskRepoMock.Object);

        var validator = new DeleteTaskCommandValidator(uowMock.Object);

        var result = await validator.ValidateAsync(command);

        Assert.True(result.IsValid);
    }
}
