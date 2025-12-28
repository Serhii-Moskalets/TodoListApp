using FluentValidation;
using FluentValidation.Results;
using Moq;
using TodoListApp.Application.Abstractions.Interfaces.Repositories;
using TodoListApp.Application.Abstractions.Interfaces.UnitOfWork;
using TodoListApp.Application.Tag.Commands.DeleteTag;

namespace TodoListApp.Application.Tests.Tag.Commands;

/// <summary>
/// Unit tests for <see cref="DeleteTagCommandHandler"/>.
/// Verifies the behavior of the handler for deleting tags.
/// </summary>
public class DeleteTagCommandHandlerTests
{
    /// <summary>
    /// Tests that the handler returns a failure result when validation fails.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test operation.</returns>
    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenValidationFails()
    {
        var validatorMock = new Mock<IValidator<DeleteTagCommand>>();
        validatorMock.Setup(v => v.ValidateAsync(It.IsAny<DeleteTagCommand>(), It.IsAny<CancellationToken>()))
                     .ReturnsAsync(new ValidationResult([new ValidationFailure("TagId", "Invalid Id")]));

        var uowMock = new Mock<IUnitOfWork>();

        var handler = new DeleteTagCommandHandler(uowMock.Object, validatorMock.Object);

        var command = new DeleteTagCommand(Guid.NewGuid(), Guid.NewGuid());

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal("Invalid Id", result.Error.Message);
    }

    /// <summary>
    /// Tests that the handler deletes the tag successfully when validation passes.
    /// Ensures that DeleteAsync and SaveChangesAsync are called exactly once.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test operation.</returns>
    [Fact]
    public async Task Handle_ShouldDeleteTag_WhenValidationPasses()
    {
        var validatorMock = new Mock<IValidator<DeleteTagCommand>>();
        validatorMock.Setup(v => v.ValidateAsync(It.IsAny<DeleteTagCommand>(), It.IsAny<CancellationToken>()))
                     .ReturnsAsync(new ValidationResult());

        var tagRepoMock = new Mock<ITagRepository>();
        tagRepoMock.Setup(r => r.DeleteAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                   .Returns(Task.CompletedTask);

        var uowMock = new Mock<IUnitOfWork>();
        uowMock.Setup(u => u.Tags).Returns(tagRepoMock.Object);
        uowMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var handler = new DeleteTagCommandHandler(uowMock.Object, validatorMock.Object);

        var tagId = Guid.NewGuid();
        var command = new DeleteTagCommand(tagId, Guid.NewGuid());

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        tagRepoMock.Verify(r => r.DeleteAsync(tagId, It.IsAny<CancellationToken>()), Times.Once);
        uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
