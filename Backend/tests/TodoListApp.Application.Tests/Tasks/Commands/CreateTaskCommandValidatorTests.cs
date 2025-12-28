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
    [Fact]
    public void Validate_ShouldHaveError_WhenTitleIsEmpty()
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
    [Fact]
    public void Validate_ShouldNotHaveError_WhenTitleIsNotEmpty()
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

    /// <summary>
    /// Tests that validation fails when the due date is in the past.
    /// </summary>
    [Fact]
    public void Validate_ShouldHaveError_WhenDueDateInThePast()
    {
        var command = new CreateTaskCommand(new CreateTaskDto
        {
            OwnerId = Guid.NewGuid(),
            Title = "Task",
            TaskListId = Guid.NewGuid(),
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
        var command = new CreateTaskCommand(new CreateTaskDto
        {
            OwnerId = Guid.NewGuid(),
            Title = "Task",
            TaskListId = Guid.NewGuid(),
            DueDate = DateTime.UtcNow.AddDays(1),
        });

        var result = this._validator.Validate(command);

        Assert.True(result.IsValid);
    }
}
