using TodoListApp.Application.TaskList.Commands.CreateTaskList;

namespace TodoListApp.Application.Tests.TaskList.Commands;

/// <summary>
/// Unit tests for <see cref="CreateTaskListCommandValidator"/>.
/// Tests validation rules for creating a task list.
/// </summary>
public class CreateTaskListCommandValidatorTests
{
    private readonly CreateTaskListCommandValidator _validator = new();

    /// <summary>
    /// Tests that validation fails when the task list title is empty.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test operation.</returns>
    [Fact]
    public async Task ShouldHaveError_WhenTitleIsEmpty()
    {
        var command = new CreateTaskListCommand(Guid.NewGuid(), string.Empty);

        var result = this._validator.Validate(command);

        Assert.False(result.IsValid);
        var titleError = Assert.Single(result.Errors, e => e.PropertyName == "Title");
        Assert.Equal("Title cannot be empty.", titleError.ErrorMessage);
    }

    /// <summary>
    /// Tests that validation passes when the task list title is not empty.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test operation.</returns>
    [Fact]
    public async Task ShouldNotHaveError_WhenTitleIsNotEmpty()
    {
        var command = new CreateTaskListCommand(Guid.NewGuid(), "Task List");

        var result = this._validator.Validate(command);

        Assert.True(result.IsValid);
    }
}
