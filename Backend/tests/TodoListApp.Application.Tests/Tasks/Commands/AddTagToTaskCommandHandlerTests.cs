using FluentValidation;
using FluentValidation.Results;
using Moq;
using TodoListApp.Application.Abstractions.Interfaces.Repositories;
using TodoListApp.Application.Abstractions.Interfaces.UnitOfWork;
using TodoListApp.Application.Tasks.Commands.AddTagToTask;
using TodoListApp.Domain.Entities;

namespace TodoListApp.Application.Tests.Tasks.Commands;

/// <summary>
/// Unit tests for <see cref="AddTagToTaskCommandHandler"/>.
/// Ensures correct behavior when adding a tag to a task.
/// </summary>
public class AddTagToTaskCommandHandlerTests
{
    /// <summary>
    /// Returns failure when validation fails.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenValidationFails()
    {
        var validatorMock = new Mock<IValidator<AddTagToTaskCommand>>();
        validatorMock.Setup(v => v.ValidateAsync(It.IsAny<AddTagToTaskCommand>(), It.IsAny<CancellationToken>()))
                     .ReturnsAsync(new ValidationResult([new ValidationFailure("TaskId", "Required")]));

        var uowMock = new Mock<IUnitOfWork>();

        var handler = new AddTagToTaskCommandHandler(uowMock.Object, validatorMock.Object);

        var command = new AddTagToTaskCommand(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());

        var result = await handler.HandleAsync(command, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("Required", result.Error!.Message);
    }

    /// <summary>
    /// Returns failure if the task does not exist.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task Handle_ShouldReturnNotFound_WhenTaskNotFound()
    {
        var validatorMock = new Mock<IValidator<AddTagToTaskCommand>>();
        validatorMock.Setup(v => v.ValidateAsync(It.IsAny<AddTagToTaskCommand>(), It.IsAny<CancellationToken>()))
                     .ReturnsAsync(new ValidationResult());

        var taskRepoMock = new Mock<ITaskRepository>();
        taskRepoMock.Setup(r => r.GetTaskByIdForUserAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), false, It.IsAny<CancellationToken>()))
                    .ReturnsAsync((TaskEntity?)null);

        var uowMock = new Mock<IUnitOfWork>();
        uowMock.Setup(u => u.Tasks).Returns(taskRepoMock.Object);

        var handler = new AddTagToTaskCommandHandler(uowMock.Object, validatorMock.Object);
        var command = new AddTagToTaskCommand(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());

        var result = await handler.HandleAsync(command, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("Task not found.", result.Error!.Message);
    }

    /// <summary>
    /// Returns success if the tag is already set on the task.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenTagAlreadySet()
    {
        var validatorMock = new Mock<IValidator<AddTagToTaskCommand>>();
        validatorMock.Setup(v => v.ValidateAsync(It.IsAny<AddTagToTaskCommand>(), It.IsAny<CancellationToken>()))
                     .ReturnsAsync(new ValidationResult());

        var userId = Guid.NewGuid();
        var task = new TaskEntity(userId, Guid.NewGuid(), "Test");
        var tag = new TagEntity("Tag", userId);

        task.SetTag(tag.Id);

        var command = new AddTagToTaskCommand(task.Id, task.OwnerId, tag.Id);

        var taskRepoMock = new Mock<ITaskRepository>();
        taskRepoMock.Setup(r => r.GetTaskByIdForUserAsync(task.Id, task.OwnerId, false, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(task);

        var tagRepoMock = new Mock<ITagRepository>();
        tagRepoMock.Setup(t => t.GetByIdAsync(tag.Id, true, It.IsAny<CancellationToken>()))
                   .ReturnsAsync(tag);

        var uowMock = new Mock<IUnitOfWork>();
        uowMock.Setup(u => u.Tasks).Returns(taskRepoMock.Object);
        uowMock.Setup(u => u.Tags).Returns(tagRepoMock.Object);

        var handler = new AddTagToTaskCommandHandler(uowMock.Object, validatorMock.Object);

        var result = await handler.HandleAsync(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
    }

    /// <summary>
    /// Returns failure when the tag does not exist.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenTagNotFound()
    {
        var validatorMock = new Mock<IValidator<AddTagToTaskCommand>>();
        validatorMock.Setup(v => v.ValidateAsync(It.IsAny<AddTagToTaskCommand>(), It.IsAny<CancellationToken>()))
                     .ReturnsAsync(new ValidationResult());

        var task = new TaskEntity(Guid.NewGuid(), Guid.NewGuid(), "Test");
        var tagId = Guid.NewGuid();
        var command = new AddTagToTaskCommand(task.Id, task.OwnerId, tagId);

        var taskRepoMock = new Mock<ITaskRepository>();
        taskRepoMock.Setup(r => r.GetTaskByIdForUserAsync(task.Id, task.OwnerId, false, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(task);

        var tagRepoMock = new Mock<ITagRepository>();
        tagRepoMock.Setup(r => r.GetByIdAsync(tagId, true, It.IsAny<CancellationToken>()))
                   .ReturnsAsync((TagEntity?)null);

        var uowMock = new Mock<IUnitOfWork>();
        uowMock.Setup(u => u.Tasks).Returns(taskRepoMock.Object);
        uowMock.Setup(u => u.Tags).Returns(tagRepoMock.Object);

        var handler = new AddTagToTaskCommandHandler(uowMock.Object, validatorMock.Object);

        var result = await handler.HandleAsync(command, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("Tag not found.", result.Error!.Message);
    }

    /// <summary>
    /// Returns failure if the tag belongs to another user.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenTagOwnedByAnotherUser()
    {
        var validatorMock = new Mock<IValidator<AddTagToTaskCommand>>();
        validatorMock.Setup(v => v.ValidateAsync(It.IsAny<AddTagToTaskCommand>(), It.IsAny<CancellationToken>()))
                     .ReturnsAsync(new ValidationResult());

        var task = new TaskEntity(Guid.NewGuid(), Guid.NewGuid(), "Test");
        var tag = new TagEntity("Tag", Guid.NewGuid()); // different user
        var command = new AddTagToTaskCommand(task.Id, task.OwnerId, tag.Id);

        var taskRepoMock = new Mock<ITaskRepository>();
        taskRepoMock.Setup(r => r.GetTaskByIdForUserAsync(task.Id, task.OwnerId, false, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(task);

        var tagRepoMock = new Mock<ITagRepository>();
        tagRepoMock.Setup(r => r.GetByIdAsync(tag.Id, true, It.IsAny<CancellationToken>()))
                   .ReturnsAsync(tag);

        var uowMock = new Mock<IUnitOfWork>();
        uowMock.Setup(u => u.Tasks).Returns(taskRepoMock.Object);
        uowMock.Setup(u => u.Tags).Returns(tagRepoMock.Object);

        var handler = new AddTagToTaskCommandHandler(uowMock.Object, validatorMock.Object);

        var result = await handler.HandleAsync(command, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("You do not own this tag.", result.Error!.Message);
    }

    /// <summary>
    /// Sets the tag successfully when all validations pass.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task Handle_ShouldSetTagSuccessfully()
    {
        var validatorMock = new Mock<IValidator<AddTagToTaskCommand>>();
        validatorMock.Setup(v => v.ValidateAsync(It.IsAny<AddTagToTaskCommand>(), It.IsAny<CancellationToken>()))
                     .ReturnsAsync(new ValidationResult());

        var task = new TaskEntity(Guid.NewGuid(), Guid.NewGuid(), "Test");
        var tag = new TagEntity("Tag", task.OwnerId);
        var command = new AddTagToTaskCommand(task.Id, task.OwnerId, tag.Id);

        var taskRepoMock = new Mock<ITaskRepository>();
        taskRepoMock.Setup(r => r.GetTaskByIdForUserAsync(task.Id, task.OwnerId, false, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(task);

        var tagRepoMock = new Mock<ITagRepository>();
        tagRepoMock.Setup(r => r.GetByIdAsync(tag.Id, true, It.IsAny<CancellationToken>()))
                   .ReturnsAsync(tag);

        var uowMock = new Mock<IUnitOfWork>();
        uowMock.Setup(u => u.Tasks).Returns(taskRepoMock.Object);
        uowMock.Setup(u => u.Tags).Returns(tagRepoMock.Object);
        uowMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var handler = new AddTagToTaskCommandHandler(uowMock.Object, validatorMock.Object);

        var result = await handler.HandleAsync(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(tag.Id, task.TagId);
        uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
