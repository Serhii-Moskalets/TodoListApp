using Moq;
using TodoListApp.Application.TaskList.Commands.DeleteTaskList;
using TodoListApp.Domain.Interfaces.Repositories;
using TodoListApp.Domain.Interfaces.UnitOfWork;

namespace TodoListApp.Application.Tests.TaskList.Commands;

/// <summary>
/// Unit tests for <see cref="DeleteTaskListCommandValidator"/>.
/// Ensures that the validator correctly verifies task list ownership.
/// </summary>
public class DeleteTaskListCommandValidatorTests
{
    /// <summary>
    /// Tests that validation fails when the task list does not belong to the user.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test operation.</returns>
    [Fact]
    public async Task Validate_ShouldHaveError_WhenUserIsNotOwner()
    {
        var taskListId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var command = new DeleteTaskListCommand(taskListId, userId);

        var taskListRepoMock = new Mock<ITaskListRepository>();
        taskListRepoMock.Setup(r => r.IsTaskListOwnerAsync(taskListId, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var uowMock = new Mock<IUnitOfWork>();
        uowMock.Setup(u => u.TaskLists).Returns(taskListRepoMock.Object);

        var validator = new DeleteTaskListCommandValidator(uowMock.Object);

        var result = await validator.ValidateAsync(command);

        Assert.False(result.IsValid);
        var error = Assert.Single(result.Errors);
        Assert.NotNull(error);
        Assert.Equal("Task list not found or does not belong to the user.", error.ErrorMessage);
        Assert.Equal("TaskListId", error.PropertyName);
    }

    /// <summary>
    /// Tests that validation passes when the task list belongs to the user.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test operation.</returns>
    [Fact]
    public async Task Validate_ShouldNotHaveError_WhenUserIsOwner()
    {
        var taskListId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var command = new DeleteTaskListCommand(taskListId, userId);

        var taskListRepoMock = new Mock<ITaskListRepository>();
        taskListRepoMock.Setup(r => r.IsTaskListOwnerAsync(taskListId, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var uowMock = new Mock<IUnitOfWork>();
        uowMock.Setup(u => u.TaskLists).Returns(taskListRepoMock.Object);

        var validator = new DeleteTaskListCommandValidator(uowMock.Object);

        var result = await validator.ValidateAsync(command);

        Assert.True(result.IsValid);
    }
}
