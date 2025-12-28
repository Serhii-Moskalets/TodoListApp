using FluentValidation;
using Moq;
using TodoListApp.Application.Abstractions.Interfaces.Repositories;
using TodoListApp.Application.Abstractions.Interfaces.UnitOfWork;
using TodoListApp.Application.Tasks.Commands.CreateTask;
using TodoListApp.Domain.Entities;

namespace TodoListApp.Application.Tests.Tasks.Commands;

/// <summary>
/// Unit tests for <see cref="CreateTaskCommandHandler"/>.
/// Verifies validation handling, task creation.
/// </summary>
public class CreateTaskCommandHandlerTests
{
    /// <summary>
    /// Ensures that the handler returns a failure result when command validation fails.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test operation.</returns>
    /// <remarks>
    /// The validator mock is configured with <c>ReturnsAsync</c> to simulate
    /// a failed validation result with a "Required" error for the Title property.
    /// This ensures the handler short-circuits and does not call the repository or unit of work.
    /// </remarks>
    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenValidationFails()
    {
        var validatorMock = new Mock<IValidator<CreateTaskCommand>>();
        validatorMock.Setup(v => v.ValidateAsync(It.IsAny<CreateTaskCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult(
                [new FluentValidation.Results.ValidationFailure("Task title", "Required")]));

        var uowMock = new Mock<IUnitOfWork>();

        var handler = new CreateTaskCommandHandler(uowMock.Object, validatorMock.Object);

        var command = new CreateTaskCommand(new Application.Tasks.Dtos.CreateTaskDto
        {
            OwnerId = Guid.NewGuid(),
            Title = string.Empty,
            TaskListId = Guid.NewGuid(),
        });

        var ct = CancellationToken.None;
        var result = await handler.Handle(command, ct);

        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal("Required", result.Error.Message);
    }

    /// <summary>
    /// Ensures that a new task is created successfully.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test operation.</returns>
    /// <remarks>
    /// The task repository mock returns <c>false</c> for <c>ExistsByTitleAsync</c>,
    /// simulating that the title is unique. The <c>ReturnsAsync(1)</c> on
    /// <c>SaveChangesAsync</c> simulates a successful commit of one record.
    /// </remarks>
    [Fact]
    public async Task Handle_ShouldAddTask()
    {
        var validatorMock = new Mock<IValidator<CreateTaskCommand>>();
        validatorMock.Setup(v => v.ValidateAsync(It.IsAny<CreateTaskCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult());

        var taskRepoMock = new Mock<ITaskRepository>();

        var uowMock = new Mock<IUnitOfWork>();
        uowMock.Setup(u => u.Tasks).Returns(taskRepoMock.Object);
        uowMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var handler = new CreateTaskCommandHandler(uowMock.Object, validatorMock.Object);

        var userId = Guid.NewGuid();
        var taskListId = Guid.NewGuid();
        var command = new CreateTaskCommand(new Application.Tasks.Dtos.CreateTaskDto
        {
            OwnerId = userId,
            Title = "Task",
            TaskListId = taskListId,
        });

        var ct = CancellationToken.None;
        var result = await handler.Handle(command, ct);

        Assert.True(result.IsSuccess);
        taskRepoMock.Verify(
            r => r.AddAsync(
                It.Is<TaskEntity>(
                    t => t.OwnerId == userId
                    && t.TaskListId == taskListId
                    && t.Title == "Task"),
                It.IsAny<CancellationToken>()),
            Times.Once);
        uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once());
    }
}
