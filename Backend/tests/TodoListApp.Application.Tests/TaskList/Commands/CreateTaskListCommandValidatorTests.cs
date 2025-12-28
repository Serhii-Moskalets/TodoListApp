using Moq;
using TodoListApp.Application.Abstractions.Interfaces.Repositories;
using TodoListApp.Application.TaskList.Commands.CreateTaskList;

namespace TodoListApp.Application.Tests.TaskList.Commands;

/// <summary>
/// Unit tests for <see cref="CreateTaskListCommandValidator"/>.
/// Tests validation rules for creating a task list.
/// </summary>
public class CreateTaskListCommandValidatorTests
{
    private readonly CreateTaskListCommandValidator _validator;

    /// <summary>
    /// Initializes a new instance of the <see cref="CreateTaskListCommandValidatorTests"/> class.
    /// </summary>
    public CreateTaskListCommandValidatorTests()
    {
        var userRepoMock = new Mock<IUserRepository>();
        userRepoMock
            .Setup(r => r.ExistsAsync(It.IsAny<Guid>(), default))
            .ReturnsAsync(true);

        this._validator = new CreateTaskListCommandValidator(userRepoMock.Object);
    }

    /// <summary>
    /// Tests that validation fails when the task list title is empty.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task Validate_ShouldHaveError_WhenTitleIsEmpty()
    {
        var command = new CreateTaskListCommand(Guid.NewGuid(), string.Empty);

        var result = await this._validator.ValidateAsync(command);

        Assert.False(result.IsValid);
        var titleError = Assert.Single(result.Errors, e => e.PropertyName == "Title");
        Assert.Equal("Title cannot be null or empty.", titleError.ErrorMessage);
    }

    /// <summary>
    /// Tests that validation passes when the task list title is not empty.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task Validate_ShouldNotHaveError_WhenTitleIsNotEmpty()
    {
        var command = new CreateTaskListCommand(Guid.NewGuid(), "Task List");

        var result = await this._validator.ValidateAsync(command);

        Assert.True(result.IsValid);
    }
}
