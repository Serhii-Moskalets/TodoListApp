using TodoListApp.Application.Tasks.Commands.UpdateTask;
using TodoListApp.Application.Tasks.Dtos;

namespace TodoListApp.Application.Tests.Tasks.Commands;

/// <summary>
/// Contains unit tests for <see cref="UpdateTaskCommandValidator"/>.
/// Ensures that the validator correctly enforces rules for updating a task list.
/// </summary>
public class UpdateTaskCommandValidatorTests
{
    private readonly UpdateTaskCommandValidator _validator = new();

    /// <summary>
    /// Tests that the validator returns an error when
    /// <see cref="UpdateTaskDto.Title"/> is empty.
    /// </summary>
    [Fact]
    public void Validate_ShouldHaveError_WhenTitleIsEmpty()
    {
        var command = new UpdateTaskCommand(new UpdateTaskDto
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
    /// <see cref="UpdateTaskDto.Title"/> is not empty.
    /// </summary>
    [Fact]
    public void Validate_ShouldHaveNotHaveError_WhenTitleIsNotEmpty()
    {
        var command = new UpdateTaskCommand(new UpdateTaskDto
        {
            OwnerId = Guid.NewGuid(),
            TaskId = Guid.NewGuid(),
            Title = "New title.",
        });

        var result = this._validator.Validate(command);

        Assert.True(result.IsValid);
    }

    /// <summary>
    /// Tests that validation fails when the due date is in the past.
    /// </summary>
    [Fact]
    public void Validate_ShouldHaveError_WhenDueDateInThePast()
    {
        var command = new UpdateTaskCommand(new Application.Tasks.Dtos.UpdateTaskDto
        {
            OwnerId = Guid.NewGuid(),
            Title = "Task",
            DueDate = DateTime.UtcNow.AddDays(-1),
        });

        var result = this._validator.Validate(command);

        Assert.False(result.IsValid);
        var dueDateError = Assert.Single(result.Errors, e => e.PropertyName == "Dto.DueDate");
        Assert.Equal("Due date cannot be in the past.", dueDateError.ErrorMessage);
    }

    /// <summary>
    /// Tests that validation passes when the due date is in the future.
    /// </summary>
    [Fact]
    public void Validate_ShouldNotHaveError_WhenDueDateInTheFuture()
    {
        var command = new UpdateTaskCommand(new UpdateTaskDto
        {
            OwnerId = Guid.NewGuid(),
            Title = "Task",
            DueDate = DateTime.UtcNow.AddDays(1),
        });

        var result = this._validator.Validate(command);

        Assert.True(result.IsValid);
    }
}
