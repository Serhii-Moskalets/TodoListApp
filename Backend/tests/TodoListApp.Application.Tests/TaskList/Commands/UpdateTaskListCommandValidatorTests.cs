using TodoListApp.Application.TaskList.Commands.UpdateTaskList;

namespace TodoListApp.Application.Tests.TaskList.Commands;

/// <summary>
/// Contains unit tests for <see cref="UpdateTaskListCommandValidator"/>.
/// Ensures that the validator correctly enforces rules for updating a task list.
/// </summary>
public class UpdateTaskListCommandValidatorTests
{
    private readonly UpdateTaskListCommandValidator _validator = new();

    /// <summary>
    /// Tests that the validator returns an error when <see cref="UpdateTaskListCommand.NewTitle"/> is empty.
    /// </summary>
    [Fact]
    public void Validate_ShouldHaveError_WhenTitleIsEmpty()
    {
        var command = new UpdateTaskListCommand(Guid.NewGuid(), Guid.NewGuid(), string.Empty);

        var result = this._validator.Validate(command);

        Assert.False(result.IsValid);
        var titleError = Assert.Single(result.Errors, e => e.PropertyName == "NewTitle");
        Assert.Equal("New title cannot be empty.", titleError.ErrorMessage);
    }

    /// <summary>
    /// Tests that the validator does not return any error when <see cref="UpdateTaskListCommand.NewTitle"/> is not empty.
    /// </summary>
    [Fact]
    public void Validate_ShouldHaveNotHaveError_WhenTitleIsNotEmpty()
    {
        var command = new UpdateTaskListCommand(Guid.NewGuid(), Guid.NewGuid(), "New task list");

        var result = this._validator.Validate(command);

        Assert.True(result.IsValid);
    }
}