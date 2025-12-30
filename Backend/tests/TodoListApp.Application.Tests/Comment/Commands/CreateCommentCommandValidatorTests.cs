using Moq;
using TodoListApp.Application.Abstractions.Interfaces.UnitOfWork;
using TodoListApp.Application.Comment.Commands.CreateComment;

namespace TodoListApp.Application.Tests.Comment.Commands;

/// <summary>
/// Unit tests for <see cref="CreateCommentCommandValidator"/>.
/// Tests validation rules for creating a tag.
/// </summary>
public class CreateCommentCommandValidatorTests
{
    private readonly CreateCommentCommandValidator _validator;

    /// <summary>
    /// Initializes a new instance of the <see cref="CreateCommentCommandValidatorTests"/> class.
    /// </summary>
    public CreateCommentCommandValidatorTests()
    {
        // Мокаємо IUnitOfWork
        var unitOfWorkMock = new Mock<IUnitOfWork>();

        // Налаштовуємо ExistsAsync для будь-якого TaskId
        unitOfWorkMock.Setup(u => u.Tasks.ExistsAsync(It.IsAny<Guid>(), default))
            .ReturnsAsync(true);

        this._validator = new CreateCommentCommandValidator(unitOfWorkMock.Object);
    }

    /// <summary>
    /// Tests that validation fails when the comments text is empty.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task Validate_ShouldHaveError_WhenTextIsEmpty()
    {
        var command = new CreateCommentCommand(Guid.NewGuid(), Guid.NewGuid(), string.Empty);

        var result = await this._validator.ValidateAsync(command);

        Assert.False(result.IsValid);
        var error = Assert.Single(result.Errors, e => e.PropertyName == "Text");
        Assert.Equal("Comment text cannot be null or empty.", error.ErrorMessage);
    }

    /// <summary>
    /// Tests that validation fails when the comments text exceeds the maximum length (1000 characters).
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task Validate_ShouldHaveError_WhenTextIsTooLong()
    {
        var text = new string('a', 1001);
        var command = new CreateCommentCommand(Guid.NewGuid(), Guid.NewGuid(), text);

        var result = await this._validator.ValidateAsync(command);

        Assert.False(result.IsValid);
        var error = Assert.Single(result.Errors, e => e.PropertyName == "Text");
        Assert.Equal("Comment text cannot exceed 1000 characters.", error.ErrorMessage);
    }

    /// <summary>
    /// Tests that validation passes when the tag name is not empty and within length limit.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task Validate_ShouldNotHaveError_WhenTextIsValid()
    {
        var command = new CreateCommentCommand(Guid.NewGuid(), Guid.NewGuid(), "Text");

        var result = await this._validator.ValidateAsync(command);

        Assert.True(result.IsValid);
    }

    /// <summary>
    /// Tests that validation fails when the task does not exist.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task Validate_ShouldHaveError_WhenTaskDoesNotExist()
    {
        var unitOfWorkMock = new Mock<IUnitOfWork>();
        unitOfWorkMock.Setup(u => u.Tasks.ExistsAsync(It.IsAny<Guid>(), default))
                      .ReturnsAsync(false); // Task не існує

        var validator = new CreateCommentCommandValidator(unitOfWorkMock.Object);
        var command = new CreateCommentCommand(Guid.NewGuid(), Guid.NewGuid(), "Valid text");

        var result = await validator.ValidateAsync(command);

        Assert.False(result.IsValid);
        var error = Assert.Single(result.Errors, e => e.PropertyName == "TaskId");
        Assert.Equal("Task not found.", error.ErrorMessage);
    }
}
