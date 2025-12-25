using Moq;
using TodoListApp.Application.Tasks.Commands.DeleteOverdueTasks;
using TodoListApp.Domain.Interfaces.Repositories;
using TodoListApp.Domain.Interfaces.UnitOfWork;

namespace TodoListApp.Application.Tests.Tasks.Commands;

/// <summary>
/// Unit tests for <see cref="DeleteOverdueTasksCommandValidator"/>.
/// Ensures that the validator correctly checks task list ownership.
/// </summary>
public class DeleteOverdueTasksCommandValidatorTests
{
    /// <summary>
    /// Validates that the command fails when the user is not the owner of the task list.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test operation.</returns>
    [Fact]
    public async Task Validate_ShouldHaveError_WhenUserIsNotOwner()
    {
        var taskListId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var command = new DeleteOverdueTasksCommand(taskListId, userId);

        var taskRepoMock = new Mock<ITaskListRepository>();
        taskRepoMock.Setup(r => r.IsTaskListOwnerAsync(taskListId, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var uowMock = new Mock<IUnitOfWork>();
        uowMock.Setup(u => u.TaskLists).Returns(taskRepoMock.Object);

        var validator = new DeleteOverdueTasksCommandValidator(uowMock.Object);

        var result = await validator.ValidateAsync(command);

        Assert.False(result.IsValid);
        var error = Assert.Single(result.Errors);
        Assert.NotNull(error);
        Assert.Equal("TaskList not found or does not belong to the user.", error.ErrorMessage);
        Assert.Equal("TaskListId", error.PropertyName);
    }

    /// <summary>
    /// Validates that the command passes when the user is the owner of the task list.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test operation.</returns>
    [Fact]
    public async Task Validate_ShouldNotHaveError_WhenUserIsOwner()
    {
        var taskListId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var command = new DeleteOverdueTasksCommand(taskListId, userId);

        var taskRepoMock = new Mock<ITaskListRepository>();
        taskRepoMock.Setup(r => r.IsTaskListOwnerAsync(taskListId, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var uowMock = new Mock<IUnitOfWork>();
        uowMock.Setup(u => u.TaskLists).Returns(taskRepoMock.Object);

        var validator = new DeleteOverdueTasksCommandValidator(uowMock.Object);

        var result = await validator.ValidateAsync(command);

        Assert.True(result.IsValid);
    }
}
