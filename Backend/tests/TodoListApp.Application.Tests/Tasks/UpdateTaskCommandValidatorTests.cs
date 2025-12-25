using TodoListApp.Application.Tasks.Commands.UpdateTask;

namespace TodoListApp.Application.Tests.Tasks;

/// <summary>
/// Contains unit tests for <see cref="UpdateTaskCommandValidator"/>.
/// Ensures that the validator correctly enforces rules for updating a task list.
/// </summary>
public class UpdateTaskCommandValidatorTests
{
    private readonly UpdateTaskCommandValidator _validator = new();

    /// <summary>
    /// Tests that the validator returns an error when
    /// <see cref="Application.Tasks.Dtos.UpdateTaskDto.Title"/> is empty.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test operation.</returns>
    [Fact]
    public async Task Validate_ShouldHaveError_WhenTitleIsEmpty()
    {
        var command = new UpdateTaskCommand(new Application.Tasks.Dtos.UpdateTaskDto
        {
            OwnerId = Guid.NewGuid(),
            TaskId = Guid.NewGuid(),
            Title = string.Empty,
        });

        var result = this._validator.Validate(command);

        Assert.False(result.IsValid);
        var titleError = Assert.Single(result.Errors, e => e.PropertyName == "Dto.Title");
        Assert.Equal("Task title cannot be empty.", titleError.ErrorMessage);
    }

    /// <summary>
    /// Tests that the validator does not return any error when
    /// <see cref="Application.Tasks.Dtos.UpdateTaskDto.Title"/> is not empty.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test operation.</returns>
    [Fact]
    public async Task Validate_ShouldHaveNotHaveError_WhenTitleIsNotEmpty()
    {
        var command = new UpdateTaskCommand(new Application.Tasks.Dtos.UpdateTaskDto
        {
            OwnerId = Guid.NewGuid(),
            TaskId = Guid.NewGuid(),
            Title = "New title.",
        });

        var result = this._validator.Validate(command);

        Assert.True(result.IsValid);
    }
}
