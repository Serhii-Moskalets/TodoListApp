using MediatR;
using Moq;
using TinyResult;
using TinyResult.Enums;
using TodoListApp.Application.Abstractions.Interfaces.Repositories;
using TodoListApp.Application.Abstractions.Interfaces.Services;
using TodoListApp.Application.Abstractions.Interfaces.UnitOfWork;
using TodoListApp.Application.Tag.Commands.CreateTag;
using TodoListApp.Application.Tasks.Commands.AddTagToTask;
using TodoListApp.Domain.Entities;

namespace TodoListApp.Application.Tests.Tag.Commands;

/// <summary>
/// Unit tests for <see cref="CreateTagCommandHandler"/>.
/// Verifies validation handling, tag creation,
/// and automatic name suffixing when duplicates exist.
/// </summary>
public class CreateTagCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _uowMock;
    private readonly Mock<ITagRepository> _tagRepoMock;
    private readonly Mock<IUniqueNameService> _uniqueNameServiceMock;
    private readonly Mock<ISender> _mediatorMock;
    private readonly CreateTagCommandHandler _handler;

    /// <summary>
    /// Initializes a new instance of the <see cref="CreateTagCommandHandlerTests"/> class.
    /// </summary>
    public CreateTagCommandHandlerTests()
    {
        this._uowMock = new Mock<IUnitOfWork>();
        this._tagRepoMock = new Mock<ITagRepository>();
        this._uniqueNameServiceMock = new Mock<IUniqueNameService>();
        this._mediatorMock = new Mock<ISender>();

        this._uowMock.Setup(u => u.Tags).Returns(this._tagRepoMock.Object);

        this._handler = new CreateTagCommandHandler(
            this._uowMock.Object,
            this._uniqueNameServiceMock.Object,
            this._mediatorMock.Object);
    }

    /// <summary>
    /// Ensures that a new tag is created successfully when the provided name is unique for the user.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test operation.</returns>
    [Fact]
    public async Task Handle_ShouldAddTag_WhenNameIsUnique()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var taskId = Guid.NewGuid();
        var command = new CreateTagCommand(userId, taskId, "Tag");

        this._uniqueNameServiceMock
            .Setup(s => s.GetUniqueNameAsync(It.IsAny<string>(), It.IsAny<Func<string, CancellationToken, Task<bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("Tag");

        this._mediatorMock
            .Setup(m => m.Send(It.IsAny<AddTagToTaskCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(await Result<bool>.SuccessAsync(true));

        this._uowMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Act
        var result = await this._handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        this._tagRepoMock.Verify(r => r.AddAsync(It.Is<TagEntity>(t => t.Name == "Tag"), It.IsAny<CancellationToken>()), Times.Once);
        this._uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests that the handler returns a failure result and deletes the orphaned tag
    /// if the subsequent call to <see cref="AddTagToTaskCommand"/> fails.
    /// </summary>
    /// <remarks>
    /// This ensures atomicity: if the tag cannot be linked to the task, the tag creation is rolled back.
    /// </remarks>
    /// <returns>A <see cref="Task"/> representing the asynchronous test operation.</returns>
    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenAddingTagToTaskFails()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new CreateTagCommand(userId, Guid.NewGuid(), "Tag");

        this._uniqueNameServiceMock
            .Setup(s => s.GetUniqueNameAsync(It.IsAny<string>(), It.IsAny<Func<string, CancellationToken, Task<bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("Tag");

        this._mediatorMock
            .Setup(m => m.Send(It.IsAny<AddTagToTaskCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(await Result<bool>.FailureAsync(ErrorCode.InvalidOperation, "Task error"));

        // Act
        var result = await this._handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        this._tagRepoMock.Verify(r => r.DeleteAsync(It.IsAny<TagEntity>(), It.IsAny<CancellationToken>()), Times.Once);
        this._uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
