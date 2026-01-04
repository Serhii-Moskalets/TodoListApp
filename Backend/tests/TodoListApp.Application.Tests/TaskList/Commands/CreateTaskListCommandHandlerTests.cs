using FluentValidation;
using Moq;
using TodoListApp.Application.Abstractions.Interfaces.Repositories;
using TodoListApp.Application.Abstractions.Interfaces.UnitOfWork;
using TodoListApp.Application.TaskList.Commands.CreateTaskList;
using TodoListApp.Domain.Entities;

namespace TodoListApp.Application.Tests.TaskList.Commands;

/// <summary>
/// Unit tests for <see cref="CreateTaskListCommandHandler"/>.
/// Verifies validation handling, task list creation,
/// and automatic title suffixing when duplicates exist.
/// </summary>
public class CreateTaskListCommandHandlerTests
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
        var validatorMock = new Mock<IValidator<CreateTaskListCommand>>();
        validatorMock.Setup(v => v.ValidateAsync(It.IsAny<CreateTaskListCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult(
                [new FluentValidation.Results.ValidationFailure("Title", "Required")]));

        var uowMock = new Mock<IUnitOfWork>();

        var handler = new CreateTaskListCommandHandler(uowMock.Object, validatorMock.Object);

        var command = new CreateTaskListCommand(Guid.NewGuid(), string.Empty);

        var ct = CancellationToken.None;
        var result = await handler.HandleAsync(command, ct);

        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal("Required", result.Error.Message);
    }

    /// <summary>
    /// Ensures that a new task list is created successfully when the provided title is unique for the user.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test operation.</returns>
    /// <remarks>
    /// The task list repository mock returns <c>false</c> for <c>ExistsByTitleAsync</c>,
    /// simulating that the title is unique. The <c>ReturnsAsync(1)</c> on
    /// <c>SaveChangesAsync</c> simulates a successful commit of one record.
    /// </remarks>
    [Fact]
    public async Task Handle_ShouldAddTaskList_WhenTitleIsUnique()
    {
        var validatorMock = new Mock<IValidator<CreateTaskListCommand>>();
        validatorMock.Setup(v => v.ValidateAsync(It.IsAny<CreateTaskListCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult());

        var taskListRepoMock = new Mock<ITaskListRepository>();
        taskListRepoMock.Setup(r => r.ExistsByTitleAsync(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var uowMock = new Mock<IUnitOfWork>();
        uowMock.Setup(u => u.TaskLists).Returns(taskListRepoMock.Object);
        uowMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var handler = new CreateTaskListCommandHandler(uowMock.Object, validatorMock.Object);

        var userId = Guid.NewGuid();
        var command = new CreateTaskListCommand(userId, "My List");

        var ct = CancellationToken.None;
        var result = await handler.HandleAsync(command, ct);

        Assert.True(result.IsSuccess);
        taskListRepoMock.Verify(r => r.AddAsync(It.Is<TaskListEntity>(t => t.OwnerId == userId && t.Title == "My List"), It.IsAny<CancellationToken>()));
        uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Ensures that a numeric suffix is appended to the task list title when a list with the same title already exists for the user.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test operation.</returns>
    /// <remarks>
    /// The repository mock uses <c>SetupSequence(...).ReturnsAsync(...)</c> to simulate:
    /// <list type="bullet">
    /// <item>First call returns true → title exists</item>
    /// <item>Second call returns false → title is now available</item>
    /// </list>
    /// This ensures that the handler appends "(1)" to the title before adding it.
    /// <para>
    /// The <c>ReturnsAsync(1)</c> on <c>SaveChangesAsync</c> simulates a successful commit.
    /// </para>
    /// </remarks>
    [Fact]
    public async Task Handle_ShouldAppendSuffix_WhenTitleExists()
    {
        var validatorMock = new Mock<IValidator<CreateTaskListCommand>>();
        validatorMock.Setup(v => v.ValidateAsync(It.IsAny<CreateTaskListCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult());

        var taskListRepoMock = new Mock<ITaskListRepository>();
        taskListRepoMock.SetupSequence(u => u.ExistsByTitleAsync(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true)
            .ReturnsAsync(false);

        var uowMock = new Mock<IUnitOfWork>();
        uowMock.Setup(u => u.TaskLists).Returns(taskListRepoMock.Object);
        uowMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var handler = new CreateTaskListCommandHandler(uowMock.Object, validatorMock.Object);

        var userId = Guid.NewGuid();
        var command = new CreateTaskListCommand(userId, "My List");

        var ct = CancellationToken.None;
        var result = await handler.HandleAsync(command, ct);

        Assert.True(result.IsSuccess);
        taskListRepoMock.Verify(r => r.AddAsync(It.Is<TaskListEntity>(t => t.Title == "My List (1)"), It.IsAny<CancellationToken>()), Times.Once());
    }
}
