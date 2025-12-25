using TodoListApp.Application.Tasks.Commands.CreateTask;
using TodoListApp.Application.Tasks.Dtos;

namespace TodoListApp.Application.Tests.Tasks.Commands;

/// <summary>
/// Unit tests for <see cref="CreateTaskCommandValidator"/>.
/// Tests validation rules for creating a task list.
/// </summary>
public class CreateTaskCommandValidatorTests
{
    private readonly CreateTaskCommandValidator _validator = new();

    /// <summary>
    /// Tests that validation fails when the task title is empty.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test operation.</returns>
    [Fact]
    public async Task Validate_ShouldHaveError_WhenTitleIsEmpty()
    {
        var command = new CreateTaskCommand(new CreateTaskDto
        {
            OwnerId = Guid.NewGuid(),
            Title = string.Empty,
            TaskListId = Guid.NewGuid(),
        });

        var result = this._validator.Validate(command);

        Assert.False(result.IsValid);
        var titleError = Assert.Single(result.Errors, e => e.PropertyName == "Dto.Title");
        Assert.Equal("Task title cannot be empty.", titleError.ErrorMessage);
    }

    /// <summary>
    /// Tests that validation passes when the task title is not empty.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test operation.</returns>
    [Fact]
    public async Task Validate_ShouldNotHaveError_WhenTitleIsNotEmpty()
    {
        var command = new CreateTaskCommand(new CreateTaskDto
        {
            OwnerId = Guid.NewGuid(),
            Title = "Task",
            TaskListId = Guid.NewGuid(),
        });

        var result = this._validator.Validate(command);

        Assert.True(result.IsValid);
    }
}
